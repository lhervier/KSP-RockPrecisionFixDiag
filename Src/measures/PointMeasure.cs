using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag.measures
{
    /// <summary>
    /// Where one vertex of a rock stands against the ground, as it is drawn. Heights are distances from the
    /// centre of the body, in metres.
    /// </summary>
    internal class PointMeasure
    {
        /// <summary>Rank of the vertex in the model of the rock: the same vertex at every load, and in every rock of its kind.</summary>
        public int VertexIndex;

        /// <summary>Height of the vertex, as it is drawn.</summary>
        public double HeightM;

        /// <summary>
        /// Height of the terrain collision surface under the vertex, or NaN when no terrain was found under it.
        /// </summary>
        public double GroundM;

        /// <summary>Adds the point to a record, on a line of its own that names the rock it belongs to.</summary>
        public void Log(RecordLog log, RockMeasure rock)
        {
            // NaN when no terrain was found. Negative: the vertex is below the ground, which stock does on
            // purpose to some extent.
            log.Line(1, "rock '{0}' '{1}' #{2} vertex {3}: ground {4}, vertex {5}, {6} mm above the ground",
                rock.QuadName, rock.ScatterName, rock.Index, VertexIndex, FormatUtils.FormatHeight(GroundM),
                FormatUtils.FormatHeight(HeightM), FormatUtils.FormatSigned((HeightM - GroundM) * 1000.0));
        }

        /// <summary>
        /// Measures a vertex of a rock, given in the coordinates of its holder and drawn through the given
        /// matrix. <paramref name="vertexIndex"/> is only recorded.
        /// </summary>
        public static PointMeasure Take(Vector3 vertex, int vertexIndex, Matrix4x4 toWorld, CelestialBody body)
        {
            Vector3d point = toWorld.MultiplyPoint3x4(vertex);

            // Down the vertical, from high above the vertex, onto the terrain only. The rock itself has no
            // collider and cannot stop the ray.
            Vector3d up = (point - body.position).normalized;
            RaycastHit hit;
            Vector3 start = (Vector3)(point + up * Constants.RAY_START_HEIGHT);
            bool onGround = Physics.Raycast(start, -(Vector3)up, out hit, 2f * Constants.RAY_START_HEIGHT,
                    1 << Constants.TERRAIN_LAYER, QueryTriggerInteraction.Ignore)
                && hit.collider.GetComponent<PQ>() != null;

            return new PointMeasure
            {
                VertexIndex = vertexIndex,
                HeightM = HeightUtils.HeightOf(point, body),
                GroundM = onGround ? HeightUtils.HeightOf(hit.point, body) : double.NaN
            };
        }
    }
}
