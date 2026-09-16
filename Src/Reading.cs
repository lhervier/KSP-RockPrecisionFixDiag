using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag
{
    /// <summary>Where, in the scene hierarchy, the object holding a set of rocks hangs.</summary>
    internal enum HolderHang
    {
        /// <summary>Directly under the terrain quad it holds the rocks of.</summary>
        OwnQuad,

        /// <summary>Somewhere under the terrain sphere of the body, other than directly under its quad.</summary>
        Sphere,

        /// <summary>Anywhere else.</summary>
        Elsewhere
    }

    /// <summary>
    /// Where one rock stands against the ground: the height of its lowest point and the height of the
    /// terrain collision surface right under that point. Heights are distances from the centre of the
    /// body, in metres.
    /// </summary>
    internal class RockReading
    {
        /// <summary>Rank of the rock among the rocks of its holder, from 0, in the order stock built them.</summary>
        public int Index;

        /// <summary>Height of the terrain collision surface under the lowest point of the rock.</summary>
        public double GroundM;

        /// <summary>Height of the lowest vertex of the rock, as it is drawn.</summary>
        public double LowestM;

        /// <summary>
        /// Height of the lowest point of the rock above the ground under it, in millimetres. Negative: that
        /// point is below the ground, which stock does on purpose to some extent.
        /// </summary>
        public double AboveGroundMm => (LowestM - GroundM) * 1000.0;
    }

    /// <summary>
    /// One object holding a set of rocks, measured against its quad. Heights are distances from the centre
    /// of the body, in metres.
    /// </summary>
    internal class HolderReading
    {
        /// <summary>Name of the kind of scatter (the rock or tree type), as the body's terrain calls it.</summary>
        public string ScatterName;

        /// <summary>Where the holder hangs in the scene hierarchy.</summary>
        public HolderHang Hang;

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
        /// Every rock of the holder whose ground could be found, in the order stock built them. Empty when
        /// the rocks of the holder were not measured.
        /// </summary>
        public readonly List<RockReading> Rocks = new List<RockReading>();

        /// <summary>Rocks of the holder for which no terrain was found under their lowest point.</summary>
        public int RocksMissed;

        /// <summary>Whether the rocks of this holder were read at all.</summary>
        public bool RocksMeasured;

        /// <summary>
        /// Mean of <see cref="RockReading.AboveGroundMm"/> over <see cref="Rocks"/>, in millimetres, or NaN
        /// when there is none.
        /// </summary>
        public double RocksMeanMm = double.NaN;
    }

    /// <summary>
    /// One terrain quad carrying rocks, and its holders. Heights are distances from the centre of the body,
    /// in metres.
    /// </summary>
    internal class QuadReading
    {
        /// <summary>Name of the terrain quad.</summary>
        public string Name;

        /// <summary>Distance from the craft to the origin of the quad, in metres.</summary>
        public double DistanceM;

        /// <summary>Height of the quad's transform position: its origin, on the ground under its centre.</summary>
        public double HeightM;

        /// <summary>Height of the translation of the quad's local to world matrix: where the ground is drawn from.</summary>
        public double MatrixHeightM;

        /// <summary>The holders of the quad, one per kind of scatter, by name of scatter kind.</summary>
        public readonly List<HolderReading> Holders = new List<HolderReading>();
    }

    /// <summary>
    /// One reading, at one moment: every quad carrying rocks around the craft with its holders, and the rocks
    /// of the nearest quad against the ground. Everything starts unknown, and stays so when there are no
    /// rocks to measure.
    /// </summary>
    internal class Reading
    {
        /// <summary>Body the craft is on.</summary>
        public string BodyName;

        /// <summary>Whether terrain scatter is switched on in the game settings. Without it there are no rocks.</summary>
        public bool ScatterEnabled;

        /// <summary>Every quad carrying rocks, nearest to the craft first. The rocks are only read on the first one.</summary>
        public readonly List<QuadReading> Quads = new List<QuadReading>();

        /// <summary>The quad nearest to the craft, or null when there is none.</summary>
        public QuadReading Nearest => Quads.Count > 0 ? Quads[0] : null;

        /// <summary>Number of holders over every quad.</summary>
        public int HolderCount;

        /// <summary>Holders that hang directly under their own quad.</summary>
        public int HoldersOnOwnQuad;

        /// <summary>Holders that hang under the terrain sphere, not directly under their quad.</summary>
        public int HoldersUnderSphere;

        /// <summary>Holders that hang anywhere else.</summary>
        public int HoldersElsewhere;

        /// <summary>
        /// Holders hanging under a quad given back to the pool of quads, whatever their state, or -1 when
        /// the pool could not be reached. Not counted anywhere else.
        /// </summary>
        public int HoldersOnPooledQuads = -1;

        /// <summary>Lowest <see cref="HolderReading.UpMm"/> over every holder of every quad.</summary>
        public double LowestUpMm = double.NaN;

        /// <summary>The same, highest.</summary>
        public double HighestUpMm = double.NaN;

        /// <summary>Largest <see cref="HolderReading.AcrossMm"/> over every holder of every quad.</summary>
        public double LargestAcrossMm = double.NaN;

        /// <summary>
        /// Writes the reading to KSP.log, under the given record number: a summary line, the line saying
        /// where the holders hang, then every quad, nearest to the craft first, each followed by its holders,
        /// and for the nearest quad by the rocks of each holder.
        /// </summary>
        public void Log(int number)
        {
            StringBuilder text = new StringBuilder();
            text.Append(Constants.LOG_PREFIX)
                .AppendFormat(CultureInfo.InvariantCulture,
                    "Record {0} on {1}: scatter {2}, {3} quads with rocks, {4} holders,"
                    + " up from {5} to {6} mm, largest across {7} mm."
                    + " Heights are distances from the centre of {1}, in metres",
                    number, BodyName, ScatterEnabled ? "on" : "off", Quads.Count, HolderCount,
                    FormatUtils.FormatSigned(LowestUpMm), FormatUtils.FormatSigned(HighestUpMm),
                    FormatUtils.Format(LargestAcrossMm));
            AppendLine(text, "  ").Append(DescribeHolders());
            foreach (QuadReading quad in Quads)
            {
                AppendLine(text, "  ").AppendFormat(CultureInfo.InvariantCulture,
                    "quad '{0}', {1:0} m from the craft: height {2}, matrix {3}",
                    quad.Name, quad.DistanceM, FormatUtils.FormatHeight(quad.HeightM),
                    FormatUtils.FormatHeight(quad.MatrixHeightM));
                foreach (HolderReading holder in quad.Holders)
                {
                    AppendLine(text, "    ").AppendFormat(CultureInfo.InvariantCulture,
                        "holder '{0}' [{1}]: height {2}, matrix {3}, up {4} mm, across {5} mm",
                        holder.ScatterName, HangTag(holder.Hang), FormatUtils.FormatHeight(holder.HeightM),
                        FormatUtils.FormatHeight(holder.MatrixHeightM), FormatUtils.FormatSigned(holder.UpMm),
                        FormatUtils.Format(holder.AcrossMm));
                    if (!holder.RocksMeasured)
                    {
                        continue;
                    }
                    AppendLine(text, "      ").AppendFormat(CultureInfo.InvariantCulture,
                        "rocks: {0} measured, {1} without ground under them, lowest point {2} mm above the"
                        + " ground on average",
                        holder.Rocks.Count, holder.RocksMissed, FormatUtils.FormatSigned(holder.RocksMeanMm));
                    foreach (RockReading rock in holder.Rocks)
                    {
                        AppendLine(text, "        ").AppendFormat(CultureInfo.InvariantCulture,
                            "rock #{0}: ground {1}, lowest point {2}, {3} mm above the ground",
                            rock.Index, FormatUtils.FormatHeight(rock.GroundM),
                            FormatUtils.FormatHeight(rock.LowestM), FormatUtils.FormatSigned(rock.AboveGroundMm));
                    }
                }
            }
            Debug.Log(text.ToString());
        }

        /// <summary>
        /// One line saying where the holders of the reading hang in the scene hierarchy. The part about the
        /// pool of quads is left out when the pool could not be reached.
        /// </summary>
        public string DescribeHolders()
        {
            string text = string.Format(CultureInfo.InvariantCulture,
                "Holders: {0} under the terrain sphere, {1} under their own quad, {2} elsewhere",
                HoldersUnderSphere, HoldersOnOwnQuad, HoldersElsewhere);
            if (HoldersOnPooledQuads >= 0)
            {
                text += string.Format(CultureInfo.InvariantCulture, ", {0} on pooled quads",
                    HoldersOnPooledQuads);
            }
            return text;
        }

        /// <summary>The short tag given to a place where a holder hangs, in KSP.log and in the window.</summary>
        public static string HangTag(HolderHang hang)
        {
            switch (hang)
            {
                case HolderHang.OwnQuad:
                    return "own quad";
                case HolderHang.Sphere:
                    return "sphere";
                default:
                    return "elsewhere";
            }
        }

        /// <summary>Starts a new log line in the text, with the log prefix and the given indent, and returns the text.</summary>
        private static StringBuilder AppendLine(StringBuilder text, string indent)
        {
            return text.AppendLine().Append(Constants.LOG_PREFIX).Append(indent);
        }
    }
}
