using System.Collections.Generic;
using com.github.lhervier.ksp.rockprecisionfixdiag.measures;

namespace com.github.lhervier.ksp.rockprecisionfixdiag
{
    /// <summary>
    /// One reading, at one moment: every quad carrying rocks around the craft with its holders, and the rocks
    /// of one quad against the ground.
    /// </summary>
    internal class Reading
    {
        /// <summary>Body the craft is on.</summary>
        public string BodyName;

        /// <summary>Whether terrain scatter is switched on in the game settings. Without it there are no rocks.</summary>
        public bool ScatterEnabled;

        /// <summary>Every quad carrying rocks, in no set order.</summary>
        public readonly List<QuadMeasure> Quads = new List<QuadMeasure>();

        /// <summary>The quad whose rocks were measured, or null when no quad carries rocks.</summary>
        public QuadMeasure RocksQuad;

        /// <summary>
        /// Takes a reading of the rocks on the body the vessel is on, or returns null when there is no
        /// vessel or no terrain to read. The reading holds three measures:
        /// - <see cref="QuadMeasure"/>, on every terrain quad carrying rocks;
        /// - <see cref="HolderMeasure"/>, on every holder of rocks of those quads;
        /// - <see cref="RockMeasure"/>, on every rock of the quad nearest to the craft.
        /// </summary>
        public static Reading Take(Vessel vessel)
        {
            if (vessel == null || vessel.mainBody == null || vessel.mainBody.pqsController == null)
            {
                return null;
            }

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

            // The holders are what can be found in the scene, grouped under the quads carrying rocks.
            Dictionary<PQ, List<PQSMod_LandClassScatterQuad>> holders = HolderFinder.Find(sphere);

            // Measure 3 covers only the rocks of one quad, the nearest to the craft. That is enough to compare
            // with the heights of its holders, and one line per rock of every quad around the craft would bury
            // the log in tens of thousands of lines at each reading.
            PQ nearest = null;
            double nearestDistance = double.MaxValue;
            foreach (PQ quad in holders.Keys)
            {
                double distance = ((Vector3d)quad.transform.position - craft).magnitude;
                if (distance < nearestDistance)
                {
                    nearest = quad;
                    nearestDistance = distance;
                }
            }

            // Measures 1 and 2 on every quad and every holder, and measure 3 on the holders of the nearest quad.
            foreach (KeyValuePair<PQ, List<PQSMod_LandClassScatterQuad>> quadHolders in holders)
            {
                QuadMeasure quadMeasure = QuadMeasure.Take(quadHolders.Key, body);
                reading.Quads.Add(quadMeasure);
                bool isNearest = quadHolders.Key == nearest;
                if (isNearest)
                {
                    reading.RocksQuad = quadMeasure;
                }

                foreach (PQSMod_LandClassScatterQuad holder in quadHolders.Value)
                {
                    HolderMeasure holderMeasure = HolderMeasure.Take(holder, body);
                    if (isNearest)
                    {
                        holderMeasure.Rocks = RockMeasure.TakeAll(holder, body, out holderMeasure.RocksMissed);
                    }
                    quadMeasure.Holders.Add(holderMeasure);
                }

                // The holders of one quad come in no set order: the name of the kind of scatter orders them, so
                // that the lines keep their order from one reading to the next.
                quadMeasure.Holders.Sort((a, b) => string.CompareOrdinal(a.ScatterName, b.ScatterName));
            }
            return reading;
        }

        /// <summary>
        /// Writes the reading to KSP.log, under the given record number: an opening line, every quad with its
        /// holders and, for the quad whose rocks were measured, their rocks, then
        /// a closing line counting what was written.
        /// </summary>
        public void Log(int number)
        {
            RecordLog log = new RecordLog();
            log.Line(0, "Record {0} on {1}: scatter {2}. Heights are distances from the centre of {1}, in metres",
                number, BodyName, ScatterEnabled ? "on" : "off");
            foreach (QuadMeasure quad in Quads)
            {
                quad.Log(log, quad == RocksQuad);
            }

            // Last, so that it is what stays in sight of whoever follows the log as it grows: the counts tell
            // whether the scene has settled since the load.
            if (RocksQuad == null)
            {
                log.Line(0, "End of record {0}: no quad with rocks", number);
            }
            else
            {
                int holderCount = 0;
                foreach (QuadMeasure quad in Quads)
                {
                    holderCount += quad.Holders.Count;
                }
                int rockCount = 0;
                int missedRockCount = 0;
                int unbuiltHolderCount = 0;
                foreach (HolderMeasure holder in RocksQuad.Holders)
                {
                    if (holder.Rocks == null)
                    {
                        unbuiltHolderCount++;
                        continue;
                    }
                    rockCount += holder.Rocks.Count;
                    missedRockCount += holder.RocksMissed;
                }
                log.Line(0,
                    "End of record {0}: {1} quads with rocks, {2} holders; nearest quad '{3}': {4} rocks measured,"
                    + " {5} without ground under them, {6} holders not built yet",
                    number, Quads.Count, holderCount, RocksQuad.Name, rockCount, missedRockCount,
                    unbuiltHolderCount);
            }
            log.Write();
        }
    }
}
