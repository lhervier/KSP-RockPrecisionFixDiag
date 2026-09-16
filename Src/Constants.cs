namespace com.github.lhervier.ksp.rockprecisionfixdiag
{
    /// <summary>
    /// Fixed sizes, identifiers and timings of the flight window, and the fixed values the survey of the rocks
    /// relies on.
    /// </summary>
    internal static class Constants
    {
        /// <summary>Identifier of the flight window. Any value no other window in the game uses.</summary>
        public const int WINDOW_ID = 0x524F5001;

        /// <summary>Tag in front of every line this mod writes to KSP.log.</summary>
        public const string LOG_PREFIX = "[RockPrecisionFixDiag] ";

        /// <summary>Seconds between two surveys of the rocks, in real time.</summary>
        public const float SURVEY_PERIOD = 0.5f;

        /// <summary>
        /// How far above the lowest point of a rock the ray looking for the ground starts, in metres. The
        /// ground is a surface without thickness that a ray only hits from above, so the ray must start
        /// above it even when the rock is sunk into it.
        /// </summary>
        public const float RAY_START_HEIGHT = 100f;

        /// <summary>Layer of the terrain colliders.</summary>
        public const int TERRAIN_LAYER = 15;

        /// <summary>
        /// Vertices per rock when a kind of scatter has no mesh of its own: stock then uses two back to back
        /// squares, four vertices each.
        /// </summary>
        public const int FALLBACK_ROCK_VERTICES = 8;

        // Where the window shows up before the player drags it, and how wide it is. The height is left to
        // the layout, which grows it as records pile up.
        public const float WINDOW_X = 60f;
        public const float WINDOW_Y = 60f;
        public const float WINDOW_WIDTH = 1060f;

        // Column widths, in pixels. Fixed rather than laid out by content: the numbers only speak once
        // aligned as a column, and the skin font is not monospaced.
        public const float COL_RECORD = 70f;
        public const float COL_NAME = 230f;
        public const float COL_WHERE = 90f;
        public const float COL_HEIGHT = 150f;
        public const float COL_GAP = 100f;
        public const float COL_UP = 160f;
        public const float COL_BUTTON = 90f;
    }
}
