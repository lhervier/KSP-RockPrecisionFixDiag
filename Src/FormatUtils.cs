using System.Globalization;

namespace com.github.lhervier.ksp.rockprecisionfixdiag
{
    /// <summary>
    /// Turns what has been measured into what the log shows. Invariant culture throughout, so that two
    /// players comparing their logs read the same digits whatever their machine is set to.
    /// </summary>
    internal static class FormatUtils
    {
        /// <summary>
        /// A distance in millimetres, to the thousandth, or "--" when there is nothing to show.
        /// </summary>
        public static string Format(double mm)
        {
            return double.IsNaN(mm) ? "--" : mm.ToString("N3", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// The same, with the sign always shown: the sign is what tells rocks floating above the ground
        /// from rocks sunk into it.
        /// </summary>
        public static string FormatSigned(double mm)
        {
            return double.IsNaN(mm) ? "--" : mm.ToString("+0.000;-0.000;0.000", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// A height from the centre of the body, in metres, to the micrometre: the same resolution as the
        /// distances in millimetres, so that subtracting two heights gives back the digits of a gap.
        /// </summary>
        public static string FormatHeight(double m)
        {
            return double.IsNaN(m) ? "--" : m.ToString("0.000000", CultureInfo.InvariantCulture) + " m";
        }
    }
}
