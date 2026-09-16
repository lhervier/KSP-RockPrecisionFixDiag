using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag
{
    /// <summary>
    /// Terrain scatter recorder. In flight, each press of Mod+F6 takes a reading of the rocks around the
    /// active vessel and writes it to KSP.log in full, quad by quad and rock by rock, under a record number
    /// that keeps counting across scene changes.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class RockPrecisionFixDiagMod : MonoBehaviour
    {
        private static readonly KeyBinding RECORD = new KeyBinding(Constants.RECORD_KEY);

        // Static: a reload destroys this addon, and the numbers must go on from one load to the next.
        private static int recordCount;

        private void Update()
        {
            if (GameSettings.MODIFIER_KEY.GetKey() && RECORD.GetKeyDown())
            {
                Record();
            }
        }

        /// <summary>
        /// Takes a reading of the rocks around the active vessel and writes it to KSP.log under the next record
        /// number. When there is no vessel or no terrain to read, writes only a line saying so, without using
        /// up a number.
        /// </summary>
        private static void Record()
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
