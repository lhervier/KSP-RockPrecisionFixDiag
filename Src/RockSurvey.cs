using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.github.lhervier.ksp.rockoffsetprobe
{
    /// <summary>
    /// Measures the rocks around the craft against the ground, in two independent ways:
    /// - for every terrain quad carrying rocks, the gap between the object holding the rocks and the quad;
    /// - for every rock of the quad nearest to the craft, the height of its lowest point above the terrain
    ///   collision surface under it.
    /// </summary>
    internal static class RockSurvey
    {
        /// <summary>
        /// How far above the lowest point of a rock the ray looking for the ground starts, in metres. The
        /// ground is a surface without thickness that a ray only hits from above, so the ray must start
        /// above it even when the rock is sunk into it.
        /// </summary>
        private const float RAY_START_HEIGHT = 100f;

        /// <summary>Layer of the terrain colliders.</summary>
        private const int TERRAIN_LAYER = 15;

        /// <summary>
        /// Vertices per rock when a kind of scatter has no mesh of its own: stock then uses two back to back
        /// squares, four vertices each.
        /// </summary>
        private const int FALLBACK_ROCK_VERTICES = 8;

        // Reused from one survey to the next: a rock mesh holds thousands of vertices.
        private static readonly List<Vector3> VERTICES = new List<Vector3>();

        /// <summary>
        /// Takes a reading of the rocks on the body the vessel is on, or returns null when there is no
        /// vessel or no terrain to read.
        /// </summary>
        public static Reading Take(Vessel vessel)
        {
            if (vessel == null || vessel.mainBody == null || vessel.mainBody.pqsController == null)
            {
                return null;
            }
            CelestialBody body = vessel.mainBody;
            PQS sphere = body.pqsController;
            Reading reading = new Reading
            {
                BodyName = body.bodyName,
                ScatterEnabled = PQS.Global_AllowScatter
            };

            // Vector3d throughout: a gap of a few millimetres is read between positions that are floats,
            // and the subtraction must not lose what they hold. Those positions are world positions, close
            // to the craft, so the floats themselves are good to a fraction of a millimetre.
            Vector3d craft = vessel.vesselTransform.position;
            HashSet<PQ> quads = new HashSet<PQ>();
            List<PQSMod_LandClassScatterQuad> holders = new List<PQSMod_LandClassScatterQuad>();
            PQ nearestQuad = null;
            double nearestDistance = double.PositiveInfinity;

            // The holders hang from the terrain sphere, in pools. Inactive ones are included: a holder is
            // placed as soon as its quad is built, and only shown once the quad is visible.
            foreach (PQSMod_LandClassScatterQuad rocks in sphere.GetComponentsInChildren<PQSMod_LandClassScatterQuad>(true))
            {
                PQ quad = rocks.quad;
                // A holder waiting in its pool has no quad.
                if (quad == null || quad.sphereRoot != sphere)
                {
                    continue;
                }
                quads.Add(quad);
                holders.Add(rocks);

                Vector3d ground = quad.transform.position;
                Vector3d gap = (Vector3d)rocks.transform.position - ground;
                Vector3d up = (ground - body.position).normalized;
                double upM = Vector3d.Dot(gap, up);
                double distance = (ground - craft).magnitude;
                reading.Gaps.Add(new RockGap
                {
                    QuadName = quad.name,
                    ScatterName = ScatterNameOf(rocks),
                    DistanceM = distance,
                    UpMm = upM * 1000.0,
                    AcrossMm = (gap - up * upM).magnitude * 1000.0,
                    LengthMm = gap.magnitude * 1000.0,
                    MatrixUpMm = Vector3d.Dot(MatrixLessPosition(rocks.transform), up) * 1000.0,
                    QuadMatrixUpMm = Vector3d.Dot(MatrixLessPosition(quad.transform), up) * 1000.0
                });
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestQuad = quad;
                }
            }

            reading.QuadCount = quads.Count;
            if (reading.Gaps.Count == 0)
            {
                return reading;
            }

            reading.Gaps.Sort((a, b) => a.DistanceM.CompareTo(b.DistanceM));
            reading.NearestUpMm = reading.Gaps[0].UpMm;
            reading.NearestMatrixUpMm = reading.Gaps[0].MatrixUpMm;
            reading.LowestUpMm = double.PositiveInfinity;
            reading.HighestUpMm = double.NegativeInfinity;
            reading.LargestMm = 0.0;
            foreach (RockGap gap in reading.Gaps)
            {
                reading.LowestUpMm = Math.Min(reading.LowestUpMm, gap.UpMm);
                reading.HighestUpMm = Math.Max(reading.HighestUpMm, gap.UpMm);
                reading.LargestMm = Math.Max(reading.LargestMm, gap.LengthMm);
            }

            // Only the rocks of one quad: that is enough to compare with the gap of its holders, and reading
            // every rock mesh around the craft twice a second would cost far more.
            foreach (PQSMod_LandClassScatterQuad rocks in holders)
            {
                if (rocks.quad == nearestQuad)
                {
                    MeasureRocks(reading, rocks, body);
                }
            }
            if (reading.Rocks.Count > 0)
            {
                double sum = 0.0;
                double sumLessMatrix = 0.0;
                foreach (RockDepth rock in reading.Rocks)
                {
                    sum += rock.BottomMm;
                    sumLessMatrix += rock.BottomLessMatrixMm;
                }
                reading.RocksMeanBottomMm = sum / reading.Rocks.Count;
                reading.RocksMeanBottomLessMatrixMm = sumLessMatrix / reading.Rocks.Count;
            }
            return reading;
        }

        /// <summary>
        /// The translation of a transform's local to world matrix minus its position, in metres. Unity
        /// computes the two separately, and they may round differently.
        /// </summary>
        private static Vector3d MatrixLessPosition(Transform transform)
        {
            Vector3 translation = transform.localToWorldMatrix.GetColumn(3);
            return (Vector3d)translation - (Vector3d)transform.position;
        }

        /// <summary>The name of the kind of scatter a holder is for, or "?" when it has none.</summary>
        private static string ScatterNameOf(PQSMod_LandClassScatterQuad rocks)
        {
            return rocks.scatter != null ? rocks.scatter.scatterName : "?";
        }

        /// <summary>
        /// Adds to the reading, for every rock of one holder, the height of its lowest point above the
        /// ground under it. Rocks without ground under them are counted as missed. A holder whose rocks are
        /// not built yet, or whose mesh does not have the expected layout, adds nothing.
        /// </summary>
        private static void MeasureRocks(Reading reading, PQSMod_LandClassScatterQuad rocks, CelestialBody body)
        {
            if (!rocks.isBuilt || rocks.mesh == null)
            {
                return;
            }

            // Stock writes the rocks into the mesh one after the other, each as a copy of the scatter's mesh
            // (moved, turned and scaled), and fills the rest of the mesh with zeros.
            Mesh baseMesh = rocks.scatter != null ? rocks.scatter.baseMesh : null;
            int stride = baseMesh != null ? baseMesh.vertexCount : FALLBACK_ROCK_VERTICES;
            rocks.mesh.GetVertices(VERTICES);
            if (stride <= 0 || rocks.count < 0 || rocks.count * stride > VERTICES.Count)
            {
                return;
            }

            Matrix4x4 toWorld = rocks.transform.localToWorldMatrix;
            Vector3d matrixLessPosition = MatrixLessPosition(rocks.transform);
            string scatterName = ScatterNameOf(rocks);
            for (int rock = 0; rock < rocks.count; rock++)
            {
                int first = rock * stride;

                // The vertical of the rock, taken at one of its vertices: a rock is a few metres wide, over
                // which the vertical does not turn enough to matter.
                Vector3d up = ((Vector3d)toWorld.MultiplyPoint3x4(VERTICES[first]) - body.position).normalized;

                Vector3d bottom = Vector3d.zero;
                double lowest = double.PositiveInfinity;
                for (int i = first; i < first + stride; i++)
                {
                    Vector3d point = toWorld.MultiplyPoint3x4(VERTICES[i]);
                    double height = Vector3d.Dot(point, up);
                    if (height < lowest)
                    {
                        lowest = height;
                        bottom = point;
                    }
                }

                // Down the vertical, from high above the lowest point, onto the terrain only. The rock itself
                // has no collider and cannot stop the ray.
                RaycastHit hit;
                Vector3 start = (Vector3)(bottom + up * RAY_START_HEIGHT);
                if (!Physics.Raycast(start, -(Vector3)up, out hit, 2f * RAY_START_HEIGHT, 1 << TERRAIN_LAYER,
                        QueryTriggerInteraction.Ignore)
                    || hit.collider.GetComponent<PQ>() == null)
                {
                    reading.RocksMissed++;
                    continue;
                }

                double bottomM = Vector3d.Dot(bottom - (Vector3d)hit.point, up);
                reading.Rocks.Add(new RockDepth
                {
                    ScatterName = scatterName,
                    Index = rock,
                    BottomMm = bottomM * 1000.0,
                    BottomLessMatrixMm = (bottomM - Vector3d.Dot(matrixLessPosition, up)) * 1000.0
                });
            }
        }
    }
}
