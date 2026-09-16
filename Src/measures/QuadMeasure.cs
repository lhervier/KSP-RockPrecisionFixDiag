using System.Collections.Generic;
using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag.measures
{
    /// <summary>
    /// Measure 1: where a terrain quad carrying rocks stands, where Unity draws it, and the same for each of
    /// its holders. Heights are distances from the centre of the body, in metres.
    /// </summary>
    internal class QuadMeasure
    {
        /// <summary>Name of the terrain quad.</summary>
        public string Name;

        /// <summary>Height of the quad's transform position: its origin, on the ground under its centre.</summary>
        public double HeightM;

        /// <summary>The quad's matrix, against its own transform position.</summary>
        public MatrixMeasure Matrix;

        /// <summary>The holders of the quad, one per kind of scatter, in the order they were given.</summary>
        public readonly List<HolderMeasure> Holders = new List<HolderMeasure>();

        /// <summary>Measures a terrain quad of a body and the given holders of rocks attached to it.</summary>
        public static QuadMeasure Take(PQ quad, List<PQSMod_LandClassScatterQuad> holders, CelestialBody body)
        {
            QuadMeasure measure = new QuadMeasure
            {
                Name = quad.name,
                HeightM = HeightUtils.HeightOf(quad.transform.position, body),
                // Read so that a quad whose matrix left its position would show.
                Matrix = MatrixMeasure.Take(quad.transform, quad, body)
            };
            foreach (PQSMod_LandClassScatterQuad holder in holders)
            {
                measure.Holders.Add(HolderMeasure.Take(holder, body));
            }
            return measure;
        }

        /// <summary>Adds the quad to a record: its line, then a line for each of its holders.</summary>
        public void Log(RecordLog log)
        {
            log.Line(1, "quad '{0}': height {1}, {2}", Name, FormatUtils.FormatHeight(HeightM), Matrix.ToLogText());
            foreach (HolderMeasure holder in Holders)
            {
                holder.Log(log);
            }
        }
    }
}
