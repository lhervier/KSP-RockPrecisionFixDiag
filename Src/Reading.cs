using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using com.github.lhervier.ksp.rockprecisionfixdiag.measures;
using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag
{
    /// <summary>
    /// One reading, at one moment: every quad carrying rocks around the craft with its holders, and the rocks
    /// of one quad against the ground. Everything starts unknown, and stays so when there are no rocks to
    /// measure.
    /// </summary>
    internal class Reading
    {
        /// <summary>Body the craft is on.</summary>
        public string BodyName;

        /// <summary>Whether terrain scatter is switched on in the game settings. Without it there are no rocks.</summary>
        public bool ScatterEnabled;

        /// <summary>Every quad carrying rocks, nearest to the craft first.</summary>
        public readonly List<QuadMeasure> Quads = new List<QuadMeasure>();

        /// <summary>The quad whose rocks were measured, or null when no quad carries rocks.</summary>
        public QuadMeasure RocksQuad;

        /// <summary>Number of holders over every quad.</summary>
        public int HolderCount;

        /// <summary>Lowest <see cref="HolderMeasure.UpMm"/> over every holder of every quad.</summary>
        public double LowestUpMm = double.NaN;

        /// <summary>The same, highest.</summary>
        public double HighestUpMm = double.NaN;

        /// <summary>Largest <see cref="HolderMeasure.AcrossMm"/> over every holder of every quad.</summary>
        public double LargestAcrossMm = double.NaN;

        /// <summary>Adds a holder to the counts and extremes of the reading.</summary>
        public void Count(HolderMeasure holder)
        {
            HolderCount++;

            // The extremes start unknown (NaN), which Math.Min and Math.Max would carry along.
            bool first = HolderCount == 1;
            LowestUpMm = first ? holder.UpMm : Math.Min(LowestUpMm, holder.UpMm);
            HighestUpMm = first ? holder.UpMm : Math.Max(HighestUpMm, holder.UpMm);
            LargestAcrossMm = first ? holder.AcrossMm : Math.Max(LargestAcrossMm, holder.AcrossMm);
        }

        /// <summary>
        /// Writes the reading to KSP.log, under the given record number: a summary line, then every quad, nearest to the craft first, each followed by its holders,
        /// and for the quad whose rocks were measured by the rocks of each holder.
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
            foreach (QuadMeasure quad in Quads)
            {
                AppendLine(text, "  ").AppendFormat(CultureInfo.InvariantCulture,
                    "quad '{0}': height {1}, matrix {2}",
                    quad.Name, FormatUtils.FormatHeight(quad.HeightM),
                    FormatUtils.FormatHeight(quad.MatrixHeightM));
                foreach (HolderMeasure holder in quad.Holders)
                {
                    AppendLine(text, "    ").AppendFormat(CultureInfo.InvariantCulture,
                        "holder '{0}': height {1}, matrix {2}, up {3} mm, across {4} mm",
                        holder.ScatterName, FormatUtils.FormatHeight(holder.HeightM),
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
                    foreach (RockMeasure rock in holder.Rocks)
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

        /// <summary>Starts a new log line in the text, with the log prefix and the given indent, and returns the text.</summary>
        private static StringBuilder AppendLine(StringBuilder text, string indent)
        {
            return text.AppendLine().Append(Constants.LOG_PREFIX).Append(indent);
        }
    }
}
