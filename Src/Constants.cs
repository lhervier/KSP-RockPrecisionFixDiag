using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag
{
    /// <summary>
    /// The key that records a reading, and the fixed values the survey of the rocks relies on.
    /// </summary>
    internal static class Constants
    {
        /// <summary>Tag in front of every line this mod writes to KSP.log.</summary>
        public const string LOG_PREFIX = "[RockPrecisionFixDiag] ";

        /// <summary>Key that records a reading, pressed along with the modifier key of the game (Alt).</summary>
        public const KeyCode RECORD_KEY = KeyCode.F6;

        /// <summary>
        /// How far above a vertex of a rock the ray looking for the ground starts, in metres. The
        /// ground is a surface without thickness that a ray only hits from above, so the ray must start
        /// above it even when the rock is sunk into it.
        /// </summary>
        public const float RAY_START_HEIGHT = 100f;

        /// <summary>How many vertices of each rock are measured against the ground.</summary>
        public const int POINTS_PER_ROCK = 10;

        /// <summary>Layer of the terrain colliders.</summary>
        public const int TERRAIN_LAYER = 15;

        /// <summary>
        /// Vertices per rock when a kind of scatter has no mesh of its own: stock then uses two back to back
        /// squares, four vertices each.
        /// </summary>
        public const int FALLBACK_ROCK_VERTICES = 8;
    }
}
