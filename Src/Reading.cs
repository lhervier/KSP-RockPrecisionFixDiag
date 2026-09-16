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
    /// How far one set of rocks sits from the terrain quad it was placed on: the position of the object
    /// holding the rocks, minus the position of the quad, in the world.
    /// </summary>
    internal class RockGap
    {
        /// <summary>Name of the terrain quad.</summary>
        public string QuadName;

        /// <summary>Name of the kind of scatter (the rock or tree type), as the body's terrain calls it.</summary>
        public string ScatterName;

        /// <summary>Where the holder hangs in the scene hierarchy.</summary>
        public HolderHang Hang;

        /// <summary>Distance from the craft to the origin of the quad, which is its centre, in metres.</summary>
        public double DistanceM;

        /// <summary>Part of the gap along the local vertical, in millimetres. Positive: rocks above the ground.</summary>
        public double UpMm;

        /// <summary>What is left of the gap once the vertical part is removed, in millimetres.</summary>
        public double AcrossMm;

        /// <summary>Full length of the gap, in millimetres.</summary>
        public double LengthMm;

        /// <summary>
        /// For the holder: the translation of its local to world matrix minus its transform position,
        /// along the local vertical, in millimetres. Both are where Unity says the holder is; the matrix is
        /// what the rocks are drawn with.
        /// </summary>
        public double MatrixUpMm;

        /// <summary>The same for the quad, in millimetres.</summary>
        public double QuadMatrixUpMm;

        /// <summary>
        /// The translation of the holder's local to world matrix minus the translation of the quad's, along
        /// the local vertical, in millimetres: where the rocks are drawn against where the ground is drawn.
        /// Positive: rocks drawn above the ground.
        /// </summary>
        public double DrawnUpMm;
    }

    /// <summary>
    /// Where one rock stands against the ground: the height of its lowest point above the terrain
    /// collision surface right under that point.
    /// </summary>
    internal class RockDepth
    {
        /// <summary>Name of the kind of scatter the rock belongs to.</summary>
        public string ScatterName;

        /// <summary>Rank of the rock among the rocks of its kind on its quad, from 0.</summary>
        public int Index;

        /// <summary>
        /// Height of the lowest point of the rock above the ground under it, in millimetres. Negative: that
        /// point is below the ground, which stock does on purpose to some extent.
        /// </summary>
        public double BottomMm;

        /// <summary>
        /// <see cref="BottomMm"/> minus the part of it that comes from the holder's matrix not being at the
        /// holder's transform position: what the height would be if the rock were drawn at that position.
        /// </summary>
        public double BottomLessMatrixMm;
    }

    /// <summary>
    /// One line of the table, at one moment: the gaps between the holders of the rocks and their quads,
    /// over every quad carrying rocks, and where the rocks of the nearest quad stand against the ground.
    /// Everything starts unknown, and stays so when there are no rocks to measure.
    /// </summary>
    internal class Reading
    {
        /// <summary>Body the craft is on.</summary>
        public string BodyName;

        /// <summary>Whether terrain scatter is switched on in the game settings. Without it there are no rocks.</summary>
        public bool ScatterEnabled;

        /// <summary>Every set of rocks measured, nearest to the craft first.</summary>
        public readonly List<RockGap> Gaps = new List<RockGap>();

        /// <summary>Number of distinct quads carrying rocks.</summary>
        public int QuadCount;

        /// <summary>Holders counted in <see cref="Gaps"/> that hang directly under their own quad.</summary>
        public int HoldersOnOwnQuad;

        /// <summary>Holders counted in <see cref="Gaps"/> that hang under the terrain sphere, not directly under their quad.</summary>
        public int HoldersUnderSphere;

        /// <summary>Holders counted in <see cref="Gaps"/> that hang anywhere else.</summary>
        public int HoldersElsewhere;

        /// <summary>
        /// Holders hanging under a quad given back to the pool of quads, whatever their state, or -1 when
        /// the pool could not be reached. Not counted in <see cref="Gaps"/>.
        /// </summary>
        public int HoldersOnPooledQuads = -1;

        /// <summary>Vertical gap of the rocks nearest to the craft, in millimetres.</summary>
        public double NearestUpMm = double.NaN;

        /// <summary>Whether the holder of the rocks nearest to the craft hangs directly under its own quad.</summary>
        public bool NearestOnOwnQuad;

        /// <summary>
        /// Lowest vertical gap over every set of rocks whose holder does not hang directly under its own
        /// quad, in millimetres.
        /// </summary>
        public double LowestUpMm = double.NaN;

        /// <summary>The same, highest.</summary>
        public double HighestUpMm = double.NaN;

        /// <summary>The same, longest gap, all directions included.</summary>
        public double LargestMm = double.NaN;

        /// <summary>Every rock of the nearest quad whose ground could be found, in the order stock built them.</summary>
        public readonly List<RockDepth> Rocks = new List<RockDepth>();

        /// <summary>Rocks of the nearest quad for which no terrain was found under their lowest point.</summary>
        public int RocksMissed;

        /// <summary>Mean of <see cref="RockDepth.BottomMm"/> over <see cref="Rocks"/>, in millimetres.</summary>
        public double RocksMeanBottomMm = double.NaN;

        /// <summary>Mean of <see cref="RockDepth.BottomLessMatrixMm"/> over <see cref="Rocks"/>, in millimetres.</summary>
        public double RocksMeanBottomLessMatrixMm = double.NaN;

        /// <summary><see cref="RockGap.MatrixUpMm"/> of the holder nearest to the craft, in millimetres.</summary>
        public double NearestMatrixUpMm = double.NaN;

        /// <summary><see cref="RockGap.DrawnUpMm"/> of the holder nearest to the craft, in millimetres.</summary>
        public double NearestDrawnUpMm = double.NaN;

        /// <summary>
        /// Writes the reading to KSP.log, under the given record number: a summary line, the line saying
        /// where the holders hang, one line per set of rocks, nearest to the craft first, then one line per
        /// rock of the nearest quad.
        /// </summary>
        public void Log(int number)
        {
            StringBuilder text = new StringBuilder();
            text.Append(Constants.LOG_PREFIX)
                .AppendFormat(CultureInfo.InvariantCulture,
                    "Record {0} on {1}: {2} quads with rocks, {3} sets of rocks, scatter {4}",
                    number, BodyName, QuadCount, Gaps.Count, ScatterEnabled ? "on" : "off")
                .AppendFormat(CultureInfo.InvariantCulture,
                    ", nearest {0}, rocks {1} mm, matrix {2} mm, rocks less matrix {3} mm, drawn {4} mm",
                    NearestOnOwnQuad ? "on quad" : FormatUtils.FormatSigned(NearestUpMm) + " mm",
                    FormatUtils.FormatSigned(RocksMeanBottomMm),
                    FormatUtils.FormatSigned(NearestMatrixUpMm),
                    FormatUtils.FormatSigned(RocksMeanBottomLessMatrixMm),
                    FormatUtils.FormatSigned(NearestDrawnUpMm))
                .AppendFormat(CultureInfo.InvariantCulture,
                    ", lowest {0} mm, highest {1} mm, largest {2} mm",
                    FormatUtils.FormatSigned(LowestUpMm), FormatUtils.FormatSigned(HighestUpMm),
                    FormatUtils.Format(LargestMm));
            text.AppendLine()
                .Append(Constants.LOG_PREFIX)
                .Append("  ")
                .Append(DescribeHolders());
            foreach (RockGap gap in Gaps)
            {
                text.AppendLine()
                    .Append(Constants.LOG_PREFIX)
                    .AppendFormat(CultureInfo.InvariantCulture,
                        "  quad '{0}' scatter '{1}' [{2}], centre at {3:0} m: up {4} mm, across {5} mm,"
                        + " holder matrix {6} mm, quad matrix {7} mm, drawn {8} mm",
                        gap.QuadName, gap.ScatterName, HangTag(gap.Hang), gap.DistanceM,
                        FormatUtils.FormatSigned(gap.UpMm), FormatUtils.Format(gap.AcrossMm),
                        FormatUtils.FormatSigned(gap.MatrixUpMm), FormatUtils.FormatSigned(gap.QuadMatrixUpMm),
                        FormatUtils.FormatSigned(gap.DrawnUpMm));
            }
            text.AppendLine()
                .Append(Constants.LOG_PREFIX)
                .AppendFormat(CultureInfo.InvariantCulture,
                    "  rocks of the nearest quad: {0} measured, {1} without ground under them",
                    Rocks.Count, RocksMissed);
            foreach (RockDepth rock in Rocks)
            {
                text.AppendLine()
                    .Append(Constants.LOG_PREFIX)
                    .AppendFormat(CultureInfo.InvariantCulture,
                        "  rock '{0}' #{1}: lowest point {2} mm above the ground, {3} mm less the matrix",
                        rock.ScatterName, rock.Index, FormatUtils.FormatSigned(rock.BottomMm),
                        FormatUtils.FormatSigned(rock.BottomLessMatrixMm));
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

        /// <summary>The short tag given in KSP.log to a place where a holder hangs.</summary>
        private static string HangTag(HolderHang hang)
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
    }
}
