using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag
{
    /// <summary>
    /// The survey of the rocks against the ground (Mod+F6): quad by quad and rock by rock, under a record number
    /// that keeps counting across scene changes.
    /// </summary>
    internal static class RockSurvey
    {
        // Static: a reload destroys the addon, and the numbers must go on from one load to the next.
        private static int recordCount;

        /// <summary>
        /// Takes a reading of the rocks around the active vessel and writes it to KSP.log under the next record
        /// number. When there is no vessel or no terrain to read, writes only a line saying so, without using
        /// up a number.
        /// </summary>
        public static void Record()
        {
            Reading reading = Reading.Take(FlightGlobals.ActiveVessel);
            if (reading == null)
            {
                Debug.Log(Constants.LOG_PREFIX + "No vessel, or no terrain under it: nothing recorded.");
                return;
            }

            recordCount++;
            reading.Log(recordCount);
        }
    }
}
