using System.Collections.Generic;
using UnityEngine;
using com.github.lhervier.ksp.rockprecisionfixdiag.measures;

namespace com.github.lhervier.ksp.rockprecisionfixdiag
{
    /// <summary>
    /// One reading, at one moment: every quad carrying rocks around the craft with its holders, and the rocks
    /// of the quad nearest to the craft against the ground.
    /// </summary>
    internal class Reading
    {
        /// <summary>Body the craft is on.</summary>
        public string BodyName;

        /// <summary>Whether terrain scatter is switched on in the game settings. Without it there are no rocks.</summary>
        public bool ScatterEnabled;

        /// <summary>Every quad carrying rocks, in no set order.</summary>
        public readonly List<QuadMeasure> Quads = new List<QuadMeasure>();

        /// <summary>
        /// Every rock of the nearest quad, holder after holder in the order of <see cref="QuadMeasure.Holders"/>,
        /// in the order stock built them within a holder.
        /// </summary>
        public readonly List<RockMeasure> Rocks = new List<RockMeasure>();

        /// <summary>
        /// Takes a reading of the rocks on the body the vessel is on, or returns null when there is no
        /// vessel or no terrain to read. The reading holds three measures:
        /// - <see cref="QuadMeasure"/>, on every terrain quad carrying rocks;
        /// - <see cref="HolderMeasure"/>, on every holder of rocks of those quads;
        /// - <see cref="RockMeasure"/>, on a few vertices of every rock of the quad nearest to the craft, and
        ///   <see cref="ColliderMeasure"/> on those of them that carry a collider.
        /// </summary>
        public static Reading Take(Vessel vessel)
        {
            if (vessel == null || vessel.mainBody == null || vessel.mainBody.pqsController == null)
            {
                return null;
            }

            // The colliders a mod can give the rocks are read where the physics engine holds them, and the game
            // does not hand it every move of a transform as it happens.
            Physics.SyncTransforms();

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

            // Every quad and every holder are measured.
            PQ nearest = null;
            double nearestDistance = double.MaxValue;
            foreach (KeyValuePair<PQ, List<PQSMod_LandClassScatterQuad>> quadHolders in holders)
            {
                QuadMeasure quadMeasure = QuadMeasure.Take(quadHolders.Key, quadHolders.Value, body);
                reading.Quads.Add(quadMeasure);

                double distance = ((Vector3d)quadHolders.Key.transform.position - craft).magnitude;
                if (distance < nearestDistance)
                {
                    nearest = quadHolders.Key;
                    nearestDistance = distance;
                }
            }

            // The rocks are measured on one quad only, the nearest to the craft. That is enough to compare
            // with the heights of its holders, and one line per rock of every quad around the craft would bury
            // the log in tens of thousands of lines at each reading.
            if (nearest != null)
            {
                foreach (PQSMod_LandClassScatterQuad holder in holders[nearest])
                {
                    reading.Rocks.AddRange(RockMeasure.TakeAll(holder, body));
                }
            }
            return reading;
        }

        /// <summary>
        /// Writes the reading to KSP.log, under the given record number: an opening line, every quad with its
        /// holders, the rocks of the nearest quad, then a closing line counting what was written.
        /// </summary>
        public void Log(int number)
        {
            RecordLog log = new RecordLog();
            log.Line(0, "Record {0} on {1}: scatter {2}. Heights are distances from the centre of {1}, in metres",
                number, BodyName, ScatterEnabled ? "on" : "off");
            foreach (QuadMeasure quad in Quads)
            {
                quad.Log(log);
            }
            foreach (RockMeasure rock in Rocks)
            {
                rock.Log(log);
            }

            // Last, so that it is what stays in sight of whoever follows the log as it grows: the counts tell
            // whether the scene has settled since the load.
            if (Quads.Count == 0)
            {
                log.Line(0, "End of record {0}: no quad with rocks", number);
            }
            else
            {
                int holderCount = 0;
                int unbuiltHolderCount = 0;
                foreach (QuadMeasure quad in Quads)
                {
                    holderCount += quad.Holders.Count;
                    foreach (HolderMeasure holder in quad.Holders)
                    {
                        if (!holder.IsBuilt)
                        {
                            unbuiltHolderCount++;
                        }
                    }
                }
                int pointCount = 0;
                int noGroundPointCount = 0;
                int colliderCount = 0;
                foreach (RockMeasure rock in Rocks)
                {
                    pointCount += rock.Points.Count;
                    if (rock.Collider != null)
                    {
                        colliderCount++;
                    }
                    foreach (PointMeasure point in rock.Points)
                    {
                        if (double.IsNaN(point.GroundM))
                        {
                            noGroundPointCount++;
                        }
                    }
                }
                // The rocks carry the name of their quad: with none measured, there is no name to give.
                if (Rocks.Count == 0)
                {
                    log.Line(0, "End of record {0}: {1} quads with rocks, {2} holders ({3} not built yet);"
                        + " nearest quad: no rocks measured",
                        number, Quads.Count, holderCount, unbuiltHolderCount);
                }
                else
                {
                    log.Line(0, "End of record {0}: {1} quads with rocks, {2} holders ({3} not built yet);"
                        + " nearest quad '{4}': {5} rocks, {6} vertices measured, {7} without ground under them,"
                        + " {8} colliders measured",
                        number, Quads.Count, holderCount, unbuiltHolderCount, Rocks[0].QuadName, Rocks.Count,
                        pointCount, noGroundPointCount, colliderCount);
                }
            }
            log.Write();
        }
    }
}
