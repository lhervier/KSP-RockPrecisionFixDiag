using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace com.github.lhervier.ksp.rockoffsetprobe
{
    /// <summary>
    /// Rock offset recorder. Shows, live and in millimetres, how far the holders of the rocks of the
    /// terrain quads around the active vessel sit from the quads themselves, and where the rocks of the
    /// nearest quad stand against the ground. The player freezes a reading into a table whenever it suits
    /// them, and the table survives scene changes, so reloading the same save several times builds it up
    /// line by line. Each frozen reading is also written to KSP.log in full, quad by quad and rock by rock.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class RockOffsetProbeMod : MonoBehaviour
    {
        private static readonly List<Reading> READINGS = new List<Reading>();

        // The line in progress, replaced by a fresh reading at every survey, never modified in place: a
        // reading is frozen simply by keeping a reference to it.
        private Reading live;

        private float nextSurvey;

        private void Update()
        {
            // Rocks are placed when their quad is built, not at every frame, so a survey twice a second
            // follows them closely enough, without walking the whole terrain hierarchy at every frame.
            if (Time.realtimeSinceStartup < nextSurvey)
            {
                return;
            }
            nextSurvey = Time.realtimeSinceStartup + Constants.SURVEY_PERIOD;
            live = RockSurvey.Take(FlightGlobals.ActiveVessel);
        }

        /// <summary>
        /// Writes a frozen reading to KSP.log: a summary line, one line per set of rocks, nearest to the
        /// craft first, then one line per rock of the nearest quad.
        /// </summary>
        private static void LogReading(int number, Reading reading)
        {
            StringBuilder text = new StringBuilder();
            text.Append(Constants.LOG_PREFIX)
                .AppendFormat(CultureInfo.InvariantCulture,
                    "Record {0} on {1}: {2} quads with rocks, {3} sets of rocks, scatter {4}",
                    number, reading.BodyName, reading.QuadCount, reading.Gaps.Count,
                    reading.ScatterEnabled ? "on" : "off")
                .AppendFormat(CultureInfo.InvariantCulture,
                    ", nearest {0} mm, rocks {1} mm, matrix {2} mm, rocks less matrix {3} mm",
                    FormatUtils.FormatSigned(reading.NearestUpMm), FormatUtils.FormatSigned(reading.RocksMeanBottomMm),
                    FormatUtils.FormatSigned(reading.NearestMatrixUpMm),
                    FormatUtils.FormatSigned(reading.RocksMeanBottomLessMatrixMm))
                .AppendFormat(CultureInfo.InvariantCulture,
                    ", lowest {0} mm, highest {1} mm, largest {2} mm",
                    FormatUtils.FormatSigned(reading.LowestUpMm), FormatUtils.FormatSigned(reading.HighestUpMm),
                    FormatUtils.Format(reading.LargestMm));
            foreach (RockGap gap in reading.Gaps)
            {
                text.AppendLine()
                    .Append(Constants.LOG_PREFIX)
                    .AppendFormat(CultureInfo.InvariantCulture,
                        "  quad '{0}' scatter '{1}', centre at {2:0} m: up {3} mm, across {4} mm,"
                        + " holder matrix {5} mm, quad matrix {6} mm",
                        gap.QuadName, gap.ScatterName, gap.DistanceM,
                        FormatUtils.FormatSigned(gap.UpMm), FormatUtils.Format(gap.AcrossMm),
                        FormatUtils.FormatSigned(gap.MatrixUpMm), FormatUtils.FormatSigned(gap.QuadMatrixUpMm));
            }
            text.AppendLine()
                .Append(Constants.LOG_PREFIX)
                .AppendFormat(CultureInfo.InvariantCulture,
                    "  rocks of the nearest quad: {0} measured, {1} without ground under them",
                    reading.Rocks.Count, reading.RocksMissed);
            foreach (RockDepth rock in reading.Rocks)
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

        // =========================================================
        // UI
        // =========================================================

        private Rect windowRect = new Rect(
            Constants.WINDOW_X,
            Constants.WINDOW_Y,
            Constants.WINDOW_WIDTH,
            0f
        );

        private void OnGUI()
        {
            GUI.skin = HighLogic.Skin;
            windowRect = GUILayout.Window(
                Constants.WINDOW_ID,
                windowRect,
                DrawWindow,
                "Rock Offset Probe"
            );
        }

        private void DrawWindow(int id)
        {
            GUILayout.BeginVertical();

            // Header
            GUILayout.BeginHorizontal();
            DrawCells("Record #", "Quads", "Nearest (mm)", "Rocks (mm)", "Matrix (mm)", "Rocks-Matrix",
                "Lowest (mm)", "Highest (mm)");
            GUILayout.EndHorizontal();

            // Recorded lines
            int deleteIndex = -1;
            for (int i = 0; i < READINGS.Count; i++)
            {
                GUILayout.BeginHorizontal();
                DrawReading(i + 1, READINGS[i]);
                if (GUILayout.Button("Delete", GUILayout.Width(Constants.COL_BUTTON)))
                {
                    deleteIndex = i;
                }
                GUILayout.EndHorizontal();
            }
            if (deleteIndex >= 0)
            {
                READINGS.RemoveAt(deleteIndex);
            }

            // Current line
            GUILayout.BeginHorizontal();
            DrawReading(READINGS.Count + 1, live);
            if (GUILayout.Button("Record", GUILayout.Width(Constants.COL_BUTTON)))
            {
                if (live != null && live.Gaps.Count > 0)
                {
                    READINGS.Add(live);
                    LogReading(READINGS.Count, live);
                }
            }
            GUILayout.EndHorizontal();

            // What the live line refers to: the nearest quad has to be the same one from one loading to the
            // next for its column to compare anything.
            GUILayout.Space(5f);
            GUILayout.Label(DescribeLive(live));

            // Clear table button
            GUILayout.Space(10f);
            if (GUILayout.Button("Clear table"))
            {
                READINGS.Clear();
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        /// <summary>One line of explanation under the table, about the reading in progress.</summary>
        private static string DescribeLive(Reading reading)
        {
            if (reading == null)
            {
                return "No vessel, or no terrain under it.";
            }
            if (!reading.ScatterEnabled)
            {
                return "Terrain scatter is switched off in the settings: there are no rocks to measure.";
            }
            if (reading.Gaps.Count == 0)
            {
                return $"No rocks around the craft on {reading.BodyName} (yet?).";
            }
            RockGap nearest = reading.Gaps[0];
            return string.Format(CultureInfo.InvariantCulture,
                "Nearest: quad '{0}', its centre {1:0} m from the craft. {2} of its rocks measured, {3} without"
                + " ground under them.",
                nearest.QuadName, nearest.DistanceM, reading.Rocks.Count, reading.RocksMissed);
        }

        /// <summary>Draws the columns of one reading, or dashes when there is none yet.</summary>
        private static void DrawReading(int number, Reading reading)
        {
            bool none = reading == null || reading.Gaps.Count == 0;
            DrawCells(
                FormatUtils.Format(number),
                none ? "--" : FormatUtils.Format(reading.QuadCount),
                none ? "--" : FormatUtils.FormatSigned(reading.NearestUpMm),
                none ? "--" : FormatUtils.FormatSigned(reading.RocksMeanBottomMm),
                none ? "--" : FormatUtils.FormatSigned(reading.NearestMatrixUpMm),
                none ? "--" : FormatUtils.FormatSigned(reading.RocksMeanBottomLessMatrixMm),
                none ? "--" : FormatUtils.FormatSigned(reading.LowestUpMm),
                none ? "--" : FormatUtils.FormatSigned(reading.HighestUpMm)
            );
        }

        /// <summary>Draws the columns of one line. The caller owns the surrounding horizontal group, so
        /// that it can put a button at the end of the line.</summary>
        private static void DrawCells(string record, string quads, string nearest, string rocks, string matrix,
            string rocksLessMatrix, string lowest, string highest)
        {
            GUILayout.Label(record, GUILayout.Width(Constants.COL_RECORD));
            GUILayout.Label(quads, GUILayout.Width(Constants.COL_QUADS));
            GUILayout.Label(nearest, GUILayout.Width(Constants.COL_GAP));
            GUILayout.Label(rocks, GUILayout.Width(Constants.COL_GAP));
            GUILayout.Label(matrix, GUILayout.Width(Constants.COL_GAP));
            GUILayout.Label(rocksLessMatrix, GUILayout.Width(Constants.COL_GAP));
            GUILayout.Label(lowest, GUILayout.Width(Constants.COL_GAP));
            GUILayout.Label(highest, GUILayout.Width(Constants.COL_GAP));
        }
    }
}
