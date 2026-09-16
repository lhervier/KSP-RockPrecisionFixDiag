using System.Globalization;
using System.Text;
using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag
{
    /// <summary>
    /// The text of one record, built line by line by the measures, then written to KSP.log as a single
    /// entry.
    /// </summary>
    internal class RecordLog
    {
        private readonly StringBuilder text = new StringBuilder();

        /// <summary>
        /// Adds a line, indented by the given depth, formatted in the invariant culture so that two players
        /// comparing their logs read the same digits whatever their machine is set to.
        /// </summary>
        public void Line(int depth, string format, params object[] args)
        {
            if (text.Length > 0)
            {
                text.AppendLine();
            }
            text.Append(Constants.LOG_PREFIX)
                .Append(' ', 2 * depth)
                .AppendFormat(CultureInfo.InvariantCulture, format, args);
        }

        /// <summary>Writes every line added so far to KSP.log.</summary>
        public void Write()
        {
            // One entry for the whole record: one per line would stamp each with its own time and stack
            // trace, hundreds of times.
            Debug.Log(text.ToString());
        }
    }
}
