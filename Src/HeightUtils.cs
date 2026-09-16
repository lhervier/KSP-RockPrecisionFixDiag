using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag
{
    /// <summary>
    /// Turns world positions into heights: distances from the centre of a body, in metres.
    /// </summary>
    internal static class HeightUtils
    {
        /// <summary>The distance from the centre of a body to a world position, in metres.</summary>
        public static double HeightOf(Vector3d position, CelestialBody body)
        {
            // Vector3d throughout: a gap of a few millimetres is read between heights of hundreds of
            // kilometres, and the subtraction must not lose what the positions hold. body.position is the
            // exact position of the centre of the body, kept in double precision before Unity rounds it into
            // the body's transform. The positions read from transforms are world positions, close to the
            // craft, so the floats themselves are good to a fraction of a millimetre.
            return (position - body.position).magnitude;
        }

        /// <summary>The translation of a transform's local to world matrix: where Unity draws it, in the world.</summary>
        public static Vector3d MatrixTranslation(Transform transform)
        {
            Vector3 translation = transform.localToWorldMatrix.GetColumn(3);
            return (Vector3d)translation;
        }
    }
}
