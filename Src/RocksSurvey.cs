using System.Collections.Generic;
using com.github.lhervier.ksp.rockprecisionfixdiag.measures;

namespace com.github.lhervier.ksp.rockprecisionfixdiag
{
    /// <summary>
    /// Takes the three measures of the rocks around the craft:
    /// - <see cref="QuadMeasure"/>, on every terrain quad carrying rocks;
    /// - <see cref="HolderMeasure"/>, on every holder of rocks of those quads;
    /// - <see cref="RockMeasure"/>, on every rock of the quad nearest to the craft.
    /// </summary>
    internal static class RocksSurvey
    {
        // Reused from one survey to the next: hundreds of quads carry rocks around the craft.
        private static readonly Dictionary<PQ, QuadMeasure> QUADS_MEASURES = new Dictionary<PQ, QuadMeasure>();
        private static readonly List<PQ> QUADS = new List<PQ>();
        private static readonly Dictionary<PQ, double> QUAD_DISTANCES = new Dictionary<PQ, double>();
        private static readonly Dictionary<PQSMod_LandClassScatterQuad, HolderMeasure> HOLDERS_MEASURES =
            new Dictionary<PQSMod_LandClassScatterQuad, HolderMeasure>();

        /// <summary>
        /// Takes a reading of the rocks on the body the vessel is on, or returns null when there is no
        /// vessel or no terrain to read.
        /// </summary>
        public static Reading Take(Vessel vessel)
        {
            if (vessel == null || vessel.mainBody == null || vessel.mainBody.pqsController == null)
            {
                return null;
            }
            
            // Locally cached arrays cleanup
            QUADS.Clear();
            QUADS_MEASURES.Clear();
            QUAD_DISTANCES.Clear();
            HOLDERS_MEASURES.Clear();
            
            // Usefull variables
            CelestialBody body = vessel.mainBody;
            Vector3d craft = vessel.vesselTransform.position;
            PQS sphere = body.pqsController;
            
            // Prepare the reading
            Reading reading = new Reading
            {
                BodyName = body.bodyName,
                ScatterEnabled = PQS.Global_AllowScatter
            };
            
            // Measures 1 and 2: every quad carrying rocks, once, and every holder, under its quad. The holders
            // are what can be found in the scene; their quads are reached through them.
            List<PQSMod_LandClassScatterQuad> holders = HolderFinder.Find(sphere);
            foreach (PQSMod_LandClassScatterQuad holder in holders)
            {
                QuadMeasure quadMeasure;
                if (!QUADS_MEASURES.TryGetValue(holder.quad, out quadMeasure))
                {
                    quadMeasure = QuadMeasure.Take(holder.quad, body);
                    QUADS_MEASURES.Add(holder.quad, quadMeasure);
                    QUAD_DISTANCES.Add(holder.quad, ((Vector3d)holder.quad.transform.position - craft).magnitude);
                    QUADS.Add(holder.quad);
                }
                
                HolderMeasure holderMeasure = HolderMeasure.Take(holder, body);
                quadMeasure.Holders.Add(holderMeasure);
                HOLDERS_MEASURES.Add(holder, holderMeasure);
                reading.Count(holderMeasure);
            }

            if (QUADS.Count == 0)
            {
                return reading;
            }

            // The nearest quad first: it is the one whose rocks are measured. The holders of one quad come in
            // no set order: the name of the kind of scatter orders them, so that the lines keep their order
            // from one reading to the next.
            QUADS.Sort((a, b) => QUAD_DISTANCES[a].CompareTo(QUAD_DISTANCES[b]));
            foreach (PQ quad in QUADS)
            {
                QuadMeasure quadMeasure = QUADS_MEASURES[quad];
                quadMeasure.Holders.Sort((a, b) => string.CompareOrdinal(a.ScatterName, b.ScatterName));
                reading.Quads.Add(quadMeasure);
            }

            // Measure 3: only the rocks of one quad. That is enough to compare with the heights of its holders,
            // and reading every rock mesh around the craft twice a second would cost far more.
            PQ nearest = QUADS[0];
            reading.RocksQuad = QUADS_MEASURES[nearest];
            foreach (PQSMod_LandClassScatterQuad holder in holders)
            {
                if (holder.quad == nearest)
                {
                    HolderMeasure measure = HOLDERS_MEASURES[holder];
                    measure.Rocks = RockMeasure.TakeAll(holder, body, out measure.RocksMissed);
                }
            }
            return reading;
        }
    }
}
