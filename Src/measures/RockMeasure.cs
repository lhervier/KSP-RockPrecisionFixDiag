using System.Collections.Generic;
using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag.measures
{
    /// <summary>
    /// Measure 3: where one rock stands against the ground, as it is drawn. Heights are distances from the
    /// centre of the body, in metres.
    /// </summary>
    internal class RockMeasure
    {
        // Reused from one call to the next: a rock mesh holds thousands of vertices.
        private static readonly List<Vector3> VERTICES = new List<Vector3>();

        /// <summary>Rank of the rock among the rocks of its holder, from 0, in the order stock built them.</summary>
        public int Index;

        /// <summary>Height of the lowest vertex of the rock, as it is drawn.</summary>
        public double LowestM;

        /// <summary>Height of the terrain collision surface under the lowest vertex of the rock.</summary>
        public double GroundM;

        /// <summary>
        /// Height of the lowest vertex of the rock above the ground under it, in millimetres. Negative: that
        /// vertex is below the ground, which stock does on purpose to some extent.
        /// </summary>
        public double AboveGroundMm => (LowestM - GroundM) * 1000.0;

        /// <summary>
        /// Measures every rock of a holder, in the order stock built them. Rocks without terrain under their
        /// lowest vertex are left out and counted in <paramref name="missed"/>. Returns null, with nothing
        /// missed, when the rocks of the holder are not built yet or their mesh does not have the expected
        /// layout.
        /// </summary>
        public static List<RockMeasure> TakeAll(PQSMod_LandClassScatterQuad holder, CelestialBody body, out int missed)
        {
            missed = 0;
            if (!holder.isBuilt || holder.mesh == null)
            {
                return null;
            }

            // Stock writes the rocks into the mesh one after the other, each as a copy of the scatter's mesh
            // (moved, turned and scaled), and fills the rest of the mesh with zeros.
            Mesh baseMesh = holder.scatter != null ? holder.scatter.baseMesh : null;
            int stride = baseMesh != null ? baseMesh.vertexCount : Constants.FALLBACK_ROCK_VERTICES;
            holder.mesh.GetVertices(VERTICES);
            if (stride <= 0 || holder.count < 0 || holder.count * stride > VERTICES.Count)
            {
                return null;
            }

            List<RockMeasure> rocks = new List<RockMeasure>(holder.count);
            Matrix4x4 toWorld = holder.transform.localToWorldMatrix;
            for (int rock = 0; rock < holder.count; rock++)
            {
                RockMeasure measure = Take(rock, VERTICES, rock * stride, stride, toWorld, body);
                if (measure == null)
                {
                    missed++;
                    continue;
                }
                rocks.Add(measure);
            }
            return rocks;
        }

        /// <summary>
        /// Measures the rock of the given rank, whose vertices are the <paramref name="stride"/> vertices of
        /// <paramref name="vertices"/> starting at <paramref name="first"/>, drawn through the given matrix.
        /// Returns null when there is no terrain under its lowest vertex.
        /// </summary>
        private static RockMeasure Take(int index, List<Vector3> vertices, int first, int stride, Matrix4x4 toWorld,
            CelestialBody body)
        {
            // The lowest vertex is the one nearest to the centre of the body, as drawn.
            Vector3d bottom = Vector3d.zero;
            double lowest = double.PositiveInfinity;
            for (int i = first; i < first + stride; i++)
            {
                Vector3d point = toWorld.MultiplyPoint3x4(vertices[i]);
                double height = HeightUtils.HeightOf(point, body);
                if (height < lowest)
                {
                    lowest = height;
                    bottom = point;
                }
            }

            // Down the vertical, from high above the lowest vertex, onto the terrain only. The rock itself has
            // no collider and cannot stop the ray.
            Vector3d up = (bottom - body.position).normalized;
            RaycastHit hit;
            Vector3 start = (Vector3)(bottom + up * Constants.RAY_START_HEIGHT);
            if (!Physics.Raycast(start, -(Vector3)up, out hit, 2f * Constants.RAY_START_HEIGHT,
                    1 << Constants.TERRAIN_LAYER, QueryTriggerInteraction.Ignore)
                || hit.collider.GetComponent<PQ>() == null)
            {
                return null;
            }

            return new RockMeasure
            {
                Index = index,
                LowestM = lowest,
                GroundM = HeightUtils.HeightOf(hit.point, body)
            };
        }
    }
}
