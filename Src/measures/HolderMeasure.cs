namespace com.github.lhervier.ksp.rockprecisionfixdiag.measures
{
    /// <summary>
    /// Measure 2: where an object holding a set of rocks stands, and where Unity draws it, against its quad.
    /// Heights are distances from the centre of the body, in metres.
    /// </summary>
    internal class HolderMeasure
    {
        /// <summary>Name of the kind of scatter (the rock or tree type), as the body's terrain calls it.</summary>
        public string ScatterName;

        /// <summary>Whether stock has built the rocks of the holder yet.</summary>
        public bool IsBuilt;

        /// <summary>Height of the holder's transform position.</summary>
        public double HeightM;

        /// <summary>The holder's matrix, the one its rocks are drawn from, against the transform position of its quad.</summary>
        public MatrixMeasure Matrix;

        /// <summary>Measures a holder of rocks attached to a quad of the given body.</summary>
        public static HolderMeasure Take(PQSMod_LandClassScatterQuad holder, CelestialBody body)
        {
            return new HolderMeasure
            {
                ScatterName = HolderFinder.ScatterNameOf(holder),
                IsBuilt = holder.isBuilt,
                HeightM = HeightUtils.HeightOf(holder.transform.position, body),
                Matrix = MatrixMeasure.Take(holder.transform, holder.quad, body)
            };
        }

        /// <summary>Adds the holder to a record, on a line of its own.</summary>
        public void Log(RecordLog log)
        {
            log.Line(2, "holder '{0}': {1}, height {2}, {3}",
                ScatterName, IsBuilt ? "built" : "not built", FormatUtils.FormatHeight(HeightM), Matrix.ToLogText());
        }
    }
}
