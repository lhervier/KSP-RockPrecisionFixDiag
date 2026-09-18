using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag
{
    /// <summary>
    /// Terrain scatter recorder. Each press of a key takes a reading and writes it to KSP.log in full: Mod+F6
    /// the rocks around the active vessel against the ground (<see cref="RockSurvey"/>), in flight only;
    /// Mod+Shift+F6 every holder of rocks against the pool it belongs to (<see cref="HolderSurvey"/>), in any
    /// scene, so that the pools can be read after leaving a body as well.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class RockPrecisionFixDiagMod : MonoBehaviour
    {
        private static readonly KeyBinding RECORD = new KeyBinding(Constants.RECORD_KEY);
        private static readonly KeyBinding LEFT_SHIFT = new KeyBinding(KeyCode.LeftShift);
        private static readonly KeyBinding RIGHT_SHIFT = new KeyBinding(KeyCode.RightShift);

        private void Update()
        {
            if (!GameSettings.MODIFIER_KEY.GetKey() || !RECORD.GetKeyDown())
            {
                return;
            }
            if (LEFT_SHIFT.GetKey() || RIGHT_SHIFT.GetKey())
            {
                HolderSurvey.Record();
            }
            else
            {
                RockSurvey.Record();
            }
        }
    }
}
