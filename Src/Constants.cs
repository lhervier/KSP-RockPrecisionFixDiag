namespace com.github.lhervier.ksp.rockprecisionfixdiag
{
    /// <summary>Fixed sizes, identifiers and timings of the flight window.</summary>
    internal static class Constants
    {
        /// <summary>Identifier of the flight window. Any value no other window in the game uses.</summary>
        public const int WINDOW_ID = 0x524F5001;

        /// <summary>Tag in front of every line this mod writes to KSP.log.</summary>
        public const string LOG_PREFIX = "[RockPrecisionFixDiag] ";

        /// <summary>Seconds between two surveys of the rocks, in real time.</summary>
        public const float SURVEY_PERIOD = 0.5f;

        // Where the window shows up before the player drags it, and how wide it is. The height is left to
        // the layout, which grows it as records pile up.
        public const float WINDOW_X = 60f;
        public const float WINDOW_Y = 60f;
        public const float WINDOW_WIDTH = 1060f;

        // Column widths, in pixels. Fixed rather than laid out by content: the numbers only speak once
        // aligned as a column, and the skin font is not monospaced.
        public const float COL_RECORD = 70f;
        public const float COL_QUADS = 70f;
        public const float COL_GAP = 110f;
        public const float COL_BUTTON = 90f;
    }
}
