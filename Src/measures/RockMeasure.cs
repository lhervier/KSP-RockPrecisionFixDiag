using System.Collections.Generic;
using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag.measures
{
    /// <summary>
    /// Where one rock stands against the ground, as it is drawn, at a few of its vertices spread over
    /// the whole rock.
    /// </summary>
    internal class RockMeasure
    {
        /// <summary>Name of the terrain quad the rock's holder is attached to.</summary>
        public string QuadName;

        /// <summary>Name of the kind of scatter of the rock's holder (the rock or tree type).</summary>
        public string ScatterName;

        /// <summary>Rank of the rock among the rocks of its holder, from 0, in the order stock built them.</summary>
        public int Index;

        /// <summary>The measured vertices of the rock, the same ones in every rock of its kind and at every load.</summary>
        public readonly List<PointMeasure> Points = new List<PointMeasure>();

        /// <summary>The collider of the rock, or null when it has none: stock rocks have none.</summary>
        public ColliderMeasure Collider;

        /// <summary>Adds the rock to a record: one line per measured vertex, then its collider if it has one.</summary>
        public void Log(RecordLog log)
        {
            foreach (PointMeasure point in Points)
            {
                point.Log(log, this);
            }
            if (Collider != null)
            {
                Collider.Log(log, this);
            }
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

            // Stock writes the rocks into the mesh one after the other, each as a copy of the scatter's model
            // (moved, turned and scaled), vertex by vertex in the order of the model, and fills the rest of the
            // mesh with zeros. Kopernicus lays the rocks out the same way, but writes only those it placed,
            // which can be fewer than count: the mesh then ends with the last rock.
            Mesh baseMesh = holder.scatter != null ? holder.scatter.baseMesh : null;
            int stride = baseMesh != null ? baseMesh.vertexCount : Constants.FALLBACK_ROCK_VERTICES;
            List<Vector3> vertices = new List<Vector3>();
            holder.mesh.GetVertices(vertices);
            if (stride <= 0 || holder.count < 0)
            {
                return rocks;
            }
            int rockCount = Mathf.Min(holder.count, vertices.Count / stride);

            // The vertices to measure are chosen on the model, which is the same at every load, rather than on
            // the drawn rocks, whose tiny differences between loads could tip a close choice. A rock is its
            // model turned and scaled evenly, so vertices far apart on the model are far apart on the rock.
            // Without a model, stock draws a rock with fewer vertices than the points wanted: all are measured.
            int[] picked;
            if (baseMesh != null)
            {
                picked = VertexPicker.PickSpread(baseMesh.vertices, Constants.POINTS_PER_ROCK);
            }
            else
            {
                picked = new int[stride];
                for (int i = 0; i < stride; i++)
                {
                    picked[i] = i;
                }
            }

            string quadName = holder.quad.name;
            string scatterName = HolderFinder.ScatterNameOf(holder);
            Matrix4x4 toWorld = holder.transform.localToWorldMatrix;
            for (int rock = 0; rock < rockCount; rock++)
            {
                RockMeasure measure = new RockMeasure
                {
                    QuadName = quadName,
                    ScatterName = scatterName,
                    Index = rock
                };
                foreach (int vertex in picked)
                {
                    measure.Points.Add(PointMeasure.Take(vertices[rock * stride + vertex], vertex, toWorld, body));
                }

                // A mod that gives the rocks a collider puts it on an object of its own under the holder, one
                // per rock, in the order the rocks were built. Stock builds none, and a holder can keep more of
                // them than it has rocks, from a quad it served before.
                if (rock < holder.transform.childCount)
                {
                    measure.Collider = ColliderMeasure.Take(
                        holder.transform.GetChild(rock), toWorld, holder.quad, body);
                }
                rocks.Add(measure);
            }
            return rocks;
        }
    }
}
