using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag.measures
{
    /// <summary>
    /// Where Unity draws an object, from the translation of its local to world matrix, against the transform
    /// position of the terrain quad it belongs to: the position the ground is placed by.
    /// </summary>
    internal class MatrixMeasure
    {
        /// <summary>Height of the translation of the matrix, from the centre of the body, in metres.</summary>
        public double HeightM;

        /// <summary>
        /// The translation of the matrix minus the transform position of the quad, along the vertical of the
        /// quad, in millimetres. Positive: drawn above the quad.
        /// </summary>
        public double UpMm;

        /// <summary>The same gap, what is left once the vertical part is removed, in millimetres.</summary>
        public double AcrossMm;

        /// <summary>Measures the matrix of a transform against a quad of the given body.</summary>
        public static MatrixMeasure Take(Transform transform, PQ quad, CelestialBody body)
        {
            Vector3d quadPosition = quad.transform.position;
            Vector3d matrix = HeightUtils.MatrixTranslation(transform);
            Vector3d gap = matrix - quadPosition;
            Vector3d up = (quadPosition - body.position).normalized;
            double upM = Vector3d.Dot(gap, up);
            return new MatrixMeasure
            {
                HeightM = HeightUtils.HeightOf(matrix, body),
                UpMm = upM * 1000.0,
                AcrossMm = (gap - up * upM).magnitude * 1000.0
            };
        }

        /// <summary>The measure as it reads in a line of a record.</summary>
        public string ToLogText()
        {
            return "matrix " + FormatUtils.FormatHeight(HeightM)
                + ", up " + FormatUtils.FormatSigned(UpMm) + " mm"
                + ", across " + FormatUtils.Format(AcrossMm) + " mm";
        }
    }
}
