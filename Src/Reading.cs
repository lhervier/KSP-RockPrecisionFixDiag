using System.Collections.Generic;

namespace com.github.lhervier.ksp.rockoffsetprobe
{
    /// <summary>
    /// How far one set of rocks sits from the terrain quad it was placed on: the position of the object
    /// holding the rocks, minus the position of the quad, in the world.
    /// </summary>
    internal class RockGap
    {
        /// <summary>Name of the terrain quad.</summary>
        public string QuadName;

        /// <summary>Name of the kind of scatter (the rock or tree type), as the body's terrain calls it.</summary>
        public string ScatterName;

        /// <summary>Distance from the craft to the origin of the quad, which is its centre, in metres.</summary>
        public double DistanceM;

        /// <summary>Part of the gap along the local vertical, in millimetres. Positive: rocks above the ground.</summary>
        public double UpMm;

        /// <summary>What is left of the gap once the vertical part is removed, in millimetres.</summary>
        public double AcrossMm;

        /// <summary>Full length of the gap, in millimetres.</summary>
        public double LengthMm;

        /// <summary>
        /// For the holder: the translation of its local to world matrix minus its transform position,
        /// along the local vertical, in millimetres. Both are where Unity says the holder is; the matrix is
        /// what the rocks are drawn with.
        /// </summary>
        public double MatrixUpMm;

        /// <summary>The same for the quad, in millimetres.</summary>
        public double QuadMatrixUpMm;
    }

    /// <summary>
    /// Where one rock stands against the ground: the height of its lowest point above the terrain
    /// collision surface right under that point.
    /// </summary>
    internal class RockDepth
    {
        /// <summary>Name of the kind of scatter the rock belongs to.</summary>
        public string ScatterName;

        /// <summary>Rank of the rock among the rocks of its kind on its quad, from 0.</summary>
        public int Index;

        /// <summary>
        /// Height of the lowest point of the rock above the ground under it, in millimetres. Negative: that
        /// point is below the ground, which stock does on purpose to some extent.
        /// </summary>
        public double BottomMm;

        /// <summary>
        /// <see cref="BottomMm"/> minus the part of it that comes from the holder's matrix not being at the
        /// holder's transform position: what the height would be if the rock were drawn at that position.
        /// </summary>
        public double BottomLessMatrixMm;
    }

    /// <summary>
    /// One line of the table, at one moment: the gaps between the holders of the rocks and their quads,
    /// over every quad carrying rocks, and where the rocks of the nearest quad stand against the ground.
    /// Everything starts unknown, and stays so when there are no rocks to measure.
    /// </summary>
    internal class Reading
    {
        /// <summary>Body the craft is on.</summary>
        public string BodyName;

        /// <summary>Whether terrain scatter is switched on in the game settings. Without it there are no rocks.</summary>
        public bool ScatterEnabled;

        /// <summary>Every set of rocks measured, nearest to the craft first.</summary>
        public readonly List<RockGap> Gaps = new List<RockGap>();

        /// <summary>Number of distinct quads carrying rocks.</summary>
        public int QuadCount;

        /// <summary>Vertical gap of the rocks nearest to the craft, in millimetres.</summary>
        public double NearestUpMm = double.NaN;

        /// <summary>Lowest vertical gap over every set of rocks, in millimetres.</summary>
        public double LowestUpMm = double.NaN;

        /// <summary>Highest vertical gap over every set of rocks, in millimetres.</summary>
        public double HighestUpMm = double.NaN;

        /// <summary>Longest gap over every set of rocks, all directions included, in millimetres.</summary>
        public double LargestMm = double.NaN;

        /// <summary>Every rock of the nearest quad whose ground could be found, in the order stock built them.</summary>
        public readonly List<RockDepth> Rocks = new List<RockDepth>();

        /// <summary>Rocks of the nearest quad for which no terrain was found under their lowest point.</summary>
        public int RocksMissed;

        /// <summary>Mean of <see cref="RockDepth.BottomMm"/> over <see cref="Rocks"/>, in millimetres.</summary>
        public double RocksMeanBottomMm = double.NaN;

        /// <summary>Mean of <see cref="RockDepth.BottomLessMatrixMm"/> over <see cref="Rocks"/>, in millimetres.</summary>
        public double RocksMeanBottomLessMatrixMm = double.NaN;

        /// <summary><see cref="RockGap.MatrixUpMm"/> of the holder nearest to the craft, in millimetres.</summary>
        public double NearestMatrixUpMm = double.NaN;
    }
}
