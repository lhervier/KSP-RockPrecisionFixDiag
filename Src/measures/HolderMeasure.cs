using System.Collections.Generic;
using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag.measures
{
    /// <summary>
    /// Measure 2: where an object holding a set of rocks stands, and where Unity draws it, against its quad.
    /// Heights are distances from the centre of the body, in metres.
    /// </summary>
    internal class HolderMeasure
    {
        /// <summary>Name of the kind of scatter (the rock or tree type), as the body's terrain calls it.</summary>
        public string ScatterName;

        /// <summary>Height of the holder's transform position.</summary>
        public double HeightM;

        /// <summary>
        /// Height of the translation of the holder's local to world matrix: the origin its rocks are drawn
        /// from.
        /// </summary>
        public double MatrixHeightM;

        /// <summary>
        /// The translation of the holder's matrix minus the transform position of its quad, along the
        /// vertical of the quad, in millimetres. Positive: rocks drawn above the ground.
        /// </summary>
        public double UpMm;

        /// <summary>The same gap, what is left once the vertical part is removed, in millimetres.</summary>
        public double AcrossMm;

        /// <summary>
        /// Measure 3 on every rock of the holder whose ground could be found, in the order stock built them,
        /// or null when the rocks of this holder were not measured. Filled by the survey, not by
        /// <see cref="Take"/>.
        /// </summary>
        public List<RockMeasure> Rocks;

        /// <summary>Rocks of the holder for which no terrain was found under their lowest point.</summary>
        public int RocksMissed;

        /// <summary>Measures a holder of rocks attached to a quad of the given body.</summary>
        public static HolderMeasure Take(PQSMod_LandClassScatterQuad holder, CelestialBody body)
        {
            // The gap that moves the rocks against the ground: they are drawn from the holder's matrix, and
            // the ground is placed by the quad's transform.
            Vector3d quadPosition = holder.quad.transform.position;
            Vector3d matrix = HeightUtils.MatrixTranslation(holder.transform);
            Vector3d gap = matrix - quadPosition;
            Vector3d up = (quadPosition - body.position).normalized;
            double upM = Vector3d.Dot(gap, up);
            return new HolderMeasure
            {
                ScatterName = holder.scatter != null ? holder.scatter.scatterName : "?",
                HeightM = HeightUtils.HeightOf(holder.transform.position, body),
                MatrixHeightM = HeightUtils.HeightOf(matrix, body),
                UpMm = upM * 1000.0,
                AcrossMm = (gap - up * upM).magnitude * 1000.0
            };
        }

        /// <summary>
        /// Adds the holder to a record: its line and, when <paramref name="withRocks"/> is set, the mean of its
        /// rocks above the ground and a line per rock, or a line saying they are not built yet.
        /// </summary>
        public void Log(RecordLog log, bool withRocks)
        {
            log.Line(2, "holder '{0}': height {1}, matrix {2}, up {3} mm, across {4} mm",
                ScatterName, FormatUtils.FormatHeight(HeightM), FormatUtils.FormatHeight(MatrixHeightM),
                FormatUtils.FormatSigned(UpMm), FormatUtils.Format(AcrossMm));
            if (!withRocks)
            {
                return;
            }
            if (Rocks == null)
            {
                log.Line(3, "rocks: not built yet");
                return;
            }

            double sumMm = 0.0;
            foreach (RockMeasure rock in Rocks)
            {
                sumMm += rock.AboveGroundMm;
            }
            double meanMm = Rocks.Count > 0 ? sumMm / Rocks.Count : double.NaN;
            log.Line(3,
                "rocks: {0} measured, {1} without ground under them, lowest point {2} mm above the ground on average",
                Rocks.Count, RocksMissed, FormatUtils.FormatSigned(meanMm));
            foreach (RockMeasure rock in Rocks)
            {
                rock.Log(log);
            }
        }
    }
}
