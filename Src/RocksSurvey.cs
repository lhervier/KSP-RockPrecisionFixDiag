using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag
{
    /// <summary>
    /// Measures the rocks around the craft against the ground:
    /// - for every terrain quad carrying rocks, the heights of the quad and of each of its holders, from their
    ///   transform positions and from their matrices, and where each holder hangs in the scene hierarchy;
    /// - for every rock of the quad nearest to the craft, the height of its lowest point and the height of
    ///   the terrain collision surface under it.
    /// </summary>
    internal static class RocksSurvey
    {
        // Reused from one survey to the next: a rock mesh holds thousands of vertices.
        private static readonly List<Vector3> VERTICES = new List<Vector3>();

        // Reused from one survey to the next as well: hundreds of quads carry rocks around the craft.
        private static readonly Dictionary<PQ, QuadReading> QUADS = new Dictionary<PQ, QuadReading>();
        private static readonly Dictionary<PQSMod_LandClassScatterQuad, HolderReading> HOLDERS =
            new Dictionary<PQSMod_LandClassScatterQuad, HolderReading>();

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

            // Vector3d throughout: a gap of a few millimetres is read between heights of hundreds of
            // kilometres, and between positions that are floats, and the subtraction must not lose what they
            // hold. body.position is the exact position of the centre of the body, kept in double precision
            // before Unity rounds it into the body's transform. The positions read from transforms are world
            // positions, close to the craft, so the floats themselves are good to a fraction of a millimetre.
            Vector3d craft = vessel.vesselTransform.position;

            // Still read after CountOnPooledQuads below, which does not touch it.
            List<PQSMod_LandClassScatterQuad> holders = HolderFinder.Find(sphere);
            QUADS.Clear();
            HOLDERS.Clear();
            foreach (PQSMod_LandClassScatterQuad holder in holders)
            {
                PQ quad = holder.quad;
                Vector3d quadPosition = quad.transform.position;
                QuadReading quadReading;
                if (!QUADS.TryGetValue(quad, out quadReading))
                {
                    quadReading = new QuadReading
                    {
                        Name = quad.name,
                        DistanceM = (quadPosition - craft).magnitude,
                        HeightM = HeightOf(quadPosition, body),
                        MatrixHeightM = HeightOf(MatrixTranslation(quad.transform), body)
                    };
                    QUADS.Add(quad, quadReading);
                    reading.Quads.Add(quadReading);
                }

                // The gap that moves the rocks against the ground: they are drawn from the holder's matrix,
                // and the ground is placed by the quad's transform. The quad's own matrix is read above, so
                // that a quad whose matrix left its position would show.
                Vector3d holderMatrix = MatrixTranslation(holder.transform);
                Vector3d gap = holderMatrix - quadPosition;
                Vector3d up = (quadPosition - body.position).normalized;
                double upM = Vector3d.Dot(gap, up);
                HolderReading holderReading = new HolderReading
                {
                    ScatterName = ScatterNameOf(holder),
                    Hang = HangOf(holder, sphere),
                    HeightM = HeightOf(holder.transform.position, body),
                    MatrixHeightM = HeightOf(holderMatrix, body),
                    UpMm = upM * 1000.0,
                    AcrossMm = (gap - up * upM).magnitude * 1000.0
                };
                quadReading.Holders.Add(holderReading);
                HOLDERS.Add(holder, holderReading);
                CountHolder(reading, holderReading);
            }
            reading.HoldersOnPooledQuads = HolderFinder.CountOnPooledQuads();

            if (reading.Quads.Count == 0)
            {
                return reading;
            }

            // The holders of one quad come in no set order: the name of the kind of scatter orders them, so
            // that the lines keep their order from one reading to the next.
            reading.Quads.Sort((a, b) => a.DistanceM.CompareTo(b.DistanceM));
            foreach (QuadReading quadReading in reading.Quads)
            {
                quadReading.Holders.Sort((a, b) => string.CompareOrdinal(a.ScatterName, b.ScatterName));
            }

            // Only the rocks of one quad: that is enough to compare with the heights of its holders, and
            // reading every rock mesh around the craft twice a second would cost far more.
            QuadReading nearest = reading.Nearest;
            foreach (PQSMod_LandClassScatterQuad holder in holders)
            {
                if (QUADS[holder.quad] == nearest)
                {
                    MeasureRocks(HOLDERS[holder], holder, body);
                }
            }
            return reading;
        }

        /// <summary>Adds a holder to the counts and extremes of the reading.</summary>
        private static void CountHolder(Reading reading, HolderReading holder)
        {
            reading.HolderCount++;
            switch (holder.Hang)
            {
                case HolderHang.OwnQuad:
                    reading.HoldersOnOwnQuad++;
                    break;
                case HolderHang.Sphere:
                    reading.HoldersUnderSphere++;
                    break;
                default:
                    reading.HoldersElsewhere++;
                    break;
            }

            // The extremes start unknown (NaN), which Math.Min and Math.Max would carry along.
            bool first = reading.HolderCount == 1;
            reading.LowestUpMm = first ? holder.UpMm : Math.Min(reading.LowestUpMm, holder.UpMm);
            reading.HighestUpMm = first ? holder.UpMm : Math.Max(reading.HighestUpMm, holder.UpMm);
            reading.LargestAcrossMm = first ? holder.AcrossMm : Math.Max(reading.LargestAcrossMm, holder.AcrossMm);
        }

        /// <summary>Where a holder attached to a quad hangs in the scene hierarchy.</summary>
        private static HolderHang HangOf(PQSMod_LandClassScatterQuad holder, PQS sphere)
        {
            if (holder.transform.parent == holder.quad.transform)
            {
                return HolderHang.OwnQuad;
            }
            return holder.transform.IsChildOf(sphere.transform) ? HolderHang.Sphere : HolderHang.Elsewhere;
        }

        /// <summary>The distance from the centre of a body to a world position, in metres.</summary>
        private static double HeightOf(Vector3d position, CelestialBody body)
        {
            return (position - body.position).magnitude;
        }

        /// <summary>The translation of a transform's local to world matrix: where Unity draws it, in the world.</summary>
        private static Vector3d MatrixTranslation(Transform transform)
        {
            Vector3 translation = transform.localToWorldMatrix.GetColumn(3);
            return (Vector3d)translation;
        }

        /// <summary>The name of the kind of scatter a holder is for, or "?" when it has none.</summary>
        private static string ScatterNameOf(PQSMod_LandClassScatterQuad holder)
        {
            return holder.scatter != null ? holder.scatter.scatterName : "?";
        }

        /// <summary>
        /// Adds to a holder, for every one of its rocks, the height of the rock's lowest point and the height
        /// of the ground under it, and their mean gap. Rocks without ground under them are counted as missed.
        /// A holder whose rocks are not built yet, or whose mesh does not have the expected layout, is left
        /// as not measured.
        /// </summary>
        private static void MeasureRocks(HolderReading holderReading, PQSMod_LandClassScatterQuad holder, CelestialBody body)
        {
            if (!holder.isBuilt || holder.mesh == null)
            {
                return;
            }

            // Stock writes the rocks into the mesh one after the other, each as a copy of the scatter's mesh
            // (moved, turned and scaled), and fills the rest of the mesh with zeros.
            Mesh baseMesh = holder.scatter != null ? holder.scatter.baseMesh : null;
            int stride = baseMesh != null ? baseMesh.vertexCount : Constants.FALLBACK_ROCK_VERTICES;
            holder.mesh.GetVertices(VERTICES);
            if (stride <= 0 || holder.count < 0 || holder.count * stride > VERTICES.Count)
            {
                return;
            }
            holderReading.RocksMeasured = true;

            Matrix4x4 toWorld = holder.transform.localToWorldMatrix;
            double sum = 0.0;
            for (int rock = 0; rock < holder.count; rock++)
            {
                // The lowest point is the vertex nearest to the centre of the body, as drawn.
                int first = rock * stride;
                Vector3d bottom = Vector3d.zero;
                double lowest = double.PositiveInfinity;
                for (int i = first; i < first + stride; i++)
                {
                    Vector3d point = toWorld.MultiplyPoint3x4(VERTICES[i]);
                    double height = HeightOf(point, body);
                    if (height < lowest)
                    {
                        lowest = height;
                        bottom = point;
                    }
                }

                // Down the vertical, from high above the lowest point, onto the terrain only. The rock itself
                // has no collider and cannot stop the ray.
                Vector3d up = (bottom - body.position).normalized;
                RaycastHit hit;
                Vector3 start = (Vector3)(bottom + up * Constants.RAY_START_HEIGHT);
                if (!Physics.Raycast(start, -(Vector3)up, out hit, 2f * Constants.RAY_START_HEIGHT,
                        1 << Constants.TERRAIN_LAYER, QueryTriggerInteraction.Ignore)
                    || hit.collider.GetComponent<PQ>() == null)
                {
                    holderReading.RocksMissed++;
                    continue;
                }

                RockReading reading = new RockReading
                {
                    Index = rock,
                    GroundM = HeightOf(hit.point, body),
                    LowestM = lowest
                };
                holderReading.Rocks.Add(reading);
                sum += reading.AboveGroundMm;
            }
            if (holderReading.Rocks.Count > 0)
            {
                holderReading.RocksMeanMm = sum / holderReading.Rocks.Count;
            }
        }
    }
}
