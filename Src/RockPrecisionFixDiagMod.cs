using System.Collections.Generic;
using System.Globalization;
using com.github.lhervier.ksp.rockprecisionfixdiag.measures;
using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag
{
    /// <summary>
    /// Terrain scatter recorder. Shows, live, the height of the terrain quad nearest to the active vessel,
    /// and in millimetres against it the heights of its holders of rocks and where their
    /// rocks stand against the ground, with the extremes over every quad around. The player freezes a
    /// reading into a table whenever it suits them, and the table survives scene changes, so reloading the
    /// same save several times builds it up reading by reading. Each frozen reading is also written to
    /// KSP.log in full, quad by quad and rock by rock.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class RockPrecisionFixDiagMod : MonoBehaviour
    {
        private static readonly List<Reading> READINGS = new List<Reading>();

        // The line in progress, replaced by a fresh reading at every survey, never modified in place: a
        // reading is frozen simply by keeping a reference to it.
        private Reading live;

        private float nextSurvey;

        private void Update()
        {
            // What is measured moves on its own schedule: rock meshes are built frames after their quad, the
            // matrices follow every floating origin shift, and the nearest quad follows the craft. A survey twice
            // a second follows all of that closely enough, without walking the terrain hierarchy at every frame.
            if (Time.realtimeSinceStartup < nextSurvey)
            {
                return;
            }
            nextSurvey = Time.realtimeSinceStartup + Constants.SURVEY_PERIOD;
            live = RocksSurvey.Take(FlightGlobals.ActiveVessel);
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
                "Rock Precision Fix Diag"
            );
        }

        private void DrawWindow(int id)
        {
            GUILayout.BeginVertical();

            // Header
            GUILayout.BeginHorizontal();
            DrawCells("Record #", "Quad, holders, rocks", "Height (m, mm)", "Matrix (mm)", "Up (mm)",
                "Across (mm)");
            GUILayout.EndHorizontal();

            // Recorded readings
            int deleteIndex = -1;
            for (int i = 0; i < READINGS.Count; i++)
            {
                if (DrawReading(i + 1, READINGS[i], "Delete"))
                {
                    deleteIndex = i;
                }
                GUILayout.Space(4f);
            }
            if (deleteIndex >= 0)
            {
                READINGS.RemoveAt(deleteIndex);
            }

            // Current reading
            if (DrawReading(READINGS.Count + 1, live, "Record") && live != null && live.Quads.Count > 0)
            {
                READINGS.Add(live);
                live.Log(READINGS.Count);
            }

            // What the live reading refers to.
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
            if (reading.Quads.Count == 0)
            {
                return $"No rocks around the craft on {reading.BodyName} (yet?).";
            }
            return string.Format(CultureInfo.InvariantCulture,
                "Height of the nearest quad: from the centre of {0}, in metres. Everything else: in millimetres,"
                + " against that height.",
                reading.BodyName);
        }

        /// <summary>
        /// Draws one reading as a block of lines: the quad whose rocks were measured, each holder of that quad followed by its
        /// rocks, then the extremes over every quad. Draws a line of dashes when there is no reading yet.
        /// Returns whether the button at the end of the first line was pressed.
        /// </summary>
        private static bool DrawReading(int number, Reading reading, string button)
        {
            QuadMeasure quad = reading != null ? reading.RocksQuad : null;

            GUILayout.BeginHorizontal();
            if (quad == null)
            {
                DrawCells(FormatUtils.Format(number), "--", "--", "--", "--", "--");
            }
            else
            {
                DrawCells(
                    FormatUtils.Format(number),
                    quad.Name,
                    FormatUtils.FormatHeight(quad.HeightM),
                    FormatUtils.FormatSigned(MmAbove(quad.MatrixHeightM, quad)),
                    "",
                    "");
            }
            bool pressed = GUILayout.Button(button, GUILayout.Width(Constants.COL_BUTTON));
            GUILayout.EndHorizontal();
            if (quad == null)
            {
                return pressed;
            }

            foreach (HolderMeasure holder in quad.Holders)
            {
                GUILayout.BeginHorizontal();
                DrawCells(
                    "",
                    "  " + holder.ScatterName,
                    FormatUtils.FormatSigned(MmAbove(holder.HeightM, quad)),
                    FormatUtils.FormatSigned(MmAbove(holder.MatrixHeightM, quad)),
                    FormatUtils.FormatSigned(holder.UpMm),
                    FormatUtils.Format(holder.AcrossMm));
                GUILayout.EndHorizontal();

                // The rocks of the holder, under it: the mean height of their lowest point above the ground
                // under each of them.
                GUILayout.BeginHorizontal();
                DrawCells(
                    "",
                    holder.RocksMeasured
                        ? string.Format(CultureInfo.InvariantCulture, "    {0} rocks, {1} without ground",
                            holder.Rocks.Count, holder.RocksMissed)
                        : "    rocks not read (not built yet?)",
                    FormatUtils.FormatSigned(holder.RocksMeanMm),
                    "",
                    "",
                    "");
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            DrawCells(
                "",
                string.Format(CultureInfo.InvariantCulture, "All {0} quads, {1} holders", reading.Quads.Count,
                    reading.HolderCount),
                "",
                "",
                FormatUtils.FormatSigned(reading.LowestUpMm) + " to " + FormatUtils.FormatSigned(reading.HighestUpMm),
                FormatUtils.Format(reading.LargestAcrossMm));
            GUILayout.EndHorizontal();
            return pressed;
        }

        /// <summary>How far a height is above the height of a quad, in millimetres.</summary>
        private static double MmAbove(double heightM, QuadMeasure quad)
        {
            return (heightM - quad.HeightM) * 1000.0;
        }

        /// <summary>Draws the columns of one line. The caller owns the surrounding horizontal group, so
        /// that it can put a button at the end of the line.</summary>
        private static void DrawCells(string record, string name, string height, string matrix, string up,
            string across)
        {
            GUILayout.Label(record, GUILayout.Width(Constants.COL_RECORD));
            GUILayout.Label(name, GUILayout.Width(Constants.COL_NAME));
            GUILayout.Label(height, GUILayout.Width(Constants.COL_HEIGHT));
            GUILayout.Label(matrix, GUILayout.Width(Constants.COL_GAP));
            GUILayout.Label(up, GUILayout.Width(Constants.COL_UP));
            GUILayout.Label(across, GUILayout.Width(Constants.COL_GAP));
        }
    }
}
