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
        /// <summary>Name of the terrain quad the rock's holder is attached to.</summary>
        public string QuadName;

        /// <summary>Name of the kind of scatter of the rock's holder (the rock or tree type).</summary>
        public string ScatterName;

        /// <summary>Rank of the rock among the rocks of its holder, from 0, in the order stock built them.</summary>
        public int Index;

        /// <summary>Height of the lowest vertex of the rock, as it is drawn.</summary>
        public double LowestM;

        /// <summary>
        /// Height of the terrain collision surface under the lowest vertex of the rock, or NaN when no terrain
        /// was found under it.
        /// </summary>
        public double GroundM;

        /// <summary>Adds the rock to a record, on a line of its own.</summary>
        public void Log(RecordLog log)
        {
            // NaN when no terrain was found. Negative: the lowest vertex is below the ground, which stock does
            // on purpose to some extent.
            log.Line(1, "rock '{0}' '{1}' #{2}: ground {3}, lowest point {4}, {5} mm above the ground",
                QuadName, ScatterName, Index, FormatUtils.FormatHeight(GroundM), FormatUtils.FormatHeight(LowestM),
                FormatUtils.FormatSigned((LowestM - GroundM) * 1000.0));
        }

        /// <summary>
        /// Measures every rock of a holder, in the order stock built them. Returns an empty list when the rocks
        /// of the holder are not built yet or their mesh does not have the expected layout.
        /// </summary>
        public static List<RockMeasure> TakeAll(PQSMod_LandClassScatterQuad holder, CelestialBody body)
        {
            List<RockMeasure> rocks = new List<RockMeasure>();
            if (!holder.isBuilt || holder.mesh == null)
            {
                return rocks;
            }

            // Stock writes the rocks into the mesh one after the other, each as a copy of the scatter's mesh
            // (moved, turned and scaled), and fills the rest of the mesh with zeros.
            Mesh baseMesh = holder.scatter != null ? holder.scatter.baseMesh : null;
            int stride = baseMesh != null ? baseMesh.vertexCount : Constants.FALLBACK_ROCK_VERTICES;
            List<Vector3> vertices = new List<Vector3>();
            holder.mesh.GetVertices(vertices);
            if (stride <= 0 || holder.count < 0 || holder.count * stride > vertices.Count)
            {
                return rocks;
            }

            string quadName = holder.quad.name;
            string scatterName = HolderFinder.ScatterNameOf(holder);
            Matrix4x4 toWorld = holder.transform.localToWorldMatrix;
            for (int rock = 0; rock < holder.count; rock++)
            {
                RockMeasure measure = Take(vertices, rock * stride, stride, toWorld, body);
                measure.QuadName = quadName;
                measure.ScatterName = scatterName;
                measure.Index = rock;
                rocks.Add(measure);
            }
            return rocks;
        }

        /// <summary>
        /// Measures the heights of the rock whose vertices are the <paramref name="stride"/> vertices of
        /// <paramref name="vertices"/> starting at <paramref name="first"/>, drawn through the given matrix.
        /// The names and the rank of the rock are left to the caller.
        /// </summary>
        private static RockMeasure Take(List<Vector3> vertices, int first, int stride, Matrix4x4 toWorld,
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
            bool onGround = Physics.Raycast(start, -(Vector3)up, out hit, 2f * Constants.RAY_START_HEIGHT,
                    1 << Constants.TERRAIN_LAYER, QueryTriggerInteraction.Ignore)
                && hit.collider.GetComponent<PQ>() != null;

            return new RockMeasure
            {
                LowestM = lowest,
                GroundM = onGround ? HeightUtils.HeightOf(hit.point, body) : double.NaN
            };
        }
    }
}
