using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag
{
    /// <summary>
    /// Terrain scatter recorder. Shows, live and in millimetres, how far the holders of the rocks of the
    /// terrain quads around the active vessel sit from the quads themselves, where they hang, and where the
    /// rocks of the nearest quad stand against the ground. The player freezes a reading into a table
    /// whenever it suits them, and the table survives scene changes, so reloading the same save several
    /// times builds it up line by line. Each frozen reading is also written to KSP.log in full, quad by
    /// quad and rock by rock.
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
            live = RockSurvey.Take(FlightGlobals.ActiveVessel);
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
            DrawCells("Record #", "Quads", "Nearest (mm)", "Rocks (mm)", "Matrix (mm)", "Rocks-Matrix",
                "Drawn (mm)", "Lowest (mm)", "Highest (mm)");
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
                    live.Log(READINGS.Count);
                }
            }
            GUILayout.EndHorizontal();

            // What the live line refers to: the nearest quad has to be the same one from one loading to the
            // next for its column to compare anything.
            GUILayout.Space(5f);
            GUILayout.Label(DescribeLive(live));
            if (live != null)
            {
                GUILayout.Label(live.DescribeHolders());
            }

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
                // A holder hanging from its own quad has no gap to it by construction: say so rather than
                // show a zero that would look measured.
                none ? "--" : reading.NearestOnOwnQuad ? "on quad" : FormatUtils.FormatSigned(reading.NearestUpMm),
                none ? "--" : FormatUtils.FormatSigned(reading.RocksMeanBottomMm),
                none ? "--" : FormatUtils.FormatSigned(reading.NearestMatrixUpMm),
                none ? "--" : FormatUtils.FormatSigned(reading.RocksMeanBottomLessMatrixMm),
                none ? "--" : FormatUtils.FormatSigned(reading.NearestDrawnUpMm),
                none ? "--" : FormatUtils.FormatSigned(reading.LowestUpMm),
                none ? "--" : FormatUtils.FormatSigned(reading.HighestUpMm)
            );
        }

        /// <summary>Draws the columns of one line. The caller owns the surrounding horizontal group, so
        /// that it can put a button at the end of the line.</summary>
        private static void DrawCells(string record, string quads, string nearest, string rocks, string matrix,
            string rocksLessMatrix, string drawn, string lowest, string highest)
        {
            GUILayout.Label(record, GUILayout.Width(Constants.COL_RECORD));
            GUILayout.Label(quads, GUILayout.Width(Constants.COL_QUADS));
            GUILayout.Label(nearest, GUILayout.Width(Constants.COL_GAP));
            GUILayout.Label(rocks, GUILayout.Width(Constants.COL_GAP));
            GUILayout.Label(matrix, GUILayout.Width(Constants.COL_GAP));
            GUILayout.Label(rocksLessMatrix, GUILayout.Width(Constants.COL_GAP));
            GUILayout.Label(drawn, GUILayout.Width(Constants.COL_GAP));
            GUILayout.Label(lowest, GUILayout.Width(Constants.COL_GAP));
            GUILayout.Label(highest, GUILayout.Width(Constants.COL_GAP));
        }
    }
}
