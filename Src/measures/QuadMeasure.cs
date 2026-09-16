using System.Collections.Generic;
using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag.measures
{
    /// <summary>
    /// Measure 1: where a terrain quad carrying rocks stands, and where Unity draws it. Heights are distances
    /// from the centre of the body, in metres.
    /// </summary>
    internal class QuadMeasure
    {
        /// <summary>Name of the terrain quad.</summary>
        public string Name;

        /// <summary>Height of the quad's transform position: its origin, on the ground under its centre.</summary>
        public double HeightM;

        /// <summary>Height of the translation of the quad's local to world matrix: where the ground is drawn from.</summary>
        public double MatrixHeightM;

        /// <summary>The holders of the quad, one per kind of scatter. Filled by the survey, not by <see cref="Take"/>.</summary>
        public readonly List<HolderMeasure> Holders = new List<HolderMeasure>();

        /// <summary>Measures a terrain quad of a body.</summary>
        public static QuadMeasure Take(PQ quad, CelestialBody body)
        {
            Vector3d position = quad.transform.position;
            return new QuadMeasure
            {
                Name = quad.name,
                HeightM = HeightUtils.HeightOf(position, body),
                // Read so that a quad whose matrix left its position would show.
                MatrixHeightM = HeightUtils.HeightOf(HeightUtils.MatrixTranslation(quad.transform), body)
            };
        }
    }
}
