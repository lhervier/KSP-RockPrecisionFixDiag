using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag.measures
{
    /// <summary>
    /// The pool of holders of one kind of scatter on one body: stock takes a holder from it for each quad that
    /// gets rocks of that kind, and puts it back when the quad is destroyed. Each count is of holders that
    /// break a rule stock keeps, except the sizes of the two lists and the pool's own counters.
    /// </summary>
    internal class PoolMeasure
    {
        // Private fields of the stock scatter. Read at every reading: Kopernicus replaces the container at
        // runtime with an object of its own.
        private const BindingFlags PRIVATE = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo CONTAINER = Field("scatterParent");
        private static readonly FieldInfo IN_USE = Field("cacheAssigned");
        private static readonly FieldInfo IN_USE_COUNTER = Field("cacheAssignedCount");
        private static readonly FieldInfo FREE = Field("cacheUnassigned");
        private static readonly FieldInfo FREE_COUNTER = Field("cacheUnassignedCount");

        /// <summary>Body whose terrain the pool belongs to.</summary>
        public string BodyName;

        /// <summary>Name of the kind of scatter (the rock or tree type), as the body's terrain calls it.</summary>
        public string ScatterName;

        /// <summary>Holders in the list of those in use, and the pool's own count of them.</summary>
        public int InUse;
        public int InUseCounter;

        /// <summary>Holders in use that no longer exist.</summary>
        public int InUseDestroyed;

        /// <summary>Holders in use that have no quad.</summary>
        public int InUseWithoutQuad;

        /// <summary>
        /// Holders in use whose quad is not active: stock clears that flag when it hands the quad back to the
        /// cache of quads of the terrain, to be reused elsewhere.
        /// </summary>
        public int InUseOnInactiveQuad;

        /// <summary>Holders in use whose quad belongs to the terrain of another body.</summary>
        public int InUseOnOtherBody;

        /// <summary>Holders in the stack of free ones, and the pool's own count of them.</summary>
        public int Free;
        public int FreeCounter;

        /// <summary>Free holders that no longer exist.</summary>
        public int FreeDestroyed;

        /// <summary>
        /// Free holders that do not hang from the container of the pool, where stock keeps them. Their position
        /// in it is not a rule: stock hands a holder back without resetting it.
        /// </summary>
        public int FreeOutsideContainer;

        /// <summary>Free holders that still have a quad.</summary>
        public int FreeWithQuad;

        /// <summary>Holders found both in use and free.</summary>
        public int InBoth;

        /// <summary>Whether the fields of the stock scatter this measure reads were found in this version of KSP.</summary>
        public static bool Readable => CONTAINER != null && IN_USE != null && IN_USE_COUNTER != null
            && FREE != null && FREE_COUNTER != null;

        /// <summary>
        /// Every rule broken by the pool: the holders counted as breaking one, plus each of the pool's counters
        /// that disagrees with its list.
        /// </summary>
        public int Irregular => InUseDestroyed + InUseWithoutQuad + InUseOnInactiveQuad + InUseOnOtherBody
            + FreeDestroyed + FreeOutsideContainer + FreeWithQuad + InBoth
            + (InUse != InUseCounter ? 1 : 0) + (Free != FreeCounter ? 1 : 0);

        /// <summary>
        /// Measures the pool of a kind of scatter of the given terrain sphere, and adds every holder it holds to
        /// <paramref name="members"/>. Returns null when stock has not created that pool (<see cref="Readable"/>
        /// must be true).
        /// </summary>
        public static PoolMeasure Take(PQSLandControl.LandClassScatter scatter, PQS sphere,
            HashSet<PQSMod_LandClassScatterQuad> members)
        {
            List<PQSMod_LandClassScatterQuad> inUse = (List<PQSMod_LandClassScatterQuad>)IN_USE.GetValue(scatter);
            Stack<PQSMod_LandClassScatterQuad> free = (Stack<PQSMod_LandClassScatterQuad>)FREE.GetValue(scatter);
            if (inUse == null || free == null)
            {
                return null;
            }
            GameObject container = (GameObject)CONTAINER.GetValue(scatter);

            PoolMeasure pool = new PoolMeasure
            {
                BodyName = sphere.name,
                ScatterName = scatter.scatterName,
                InUse = inUse.Count,
                InUseCounter = (int)IN_USE_COUNTER.GetValue(scatter),
                Free = free.Count,
                FreeCounter = (int)FREE_COUNTER.GetValue(scatter)
            };

            HashSet<PQSMod_LandClassScatterQuad> inUseSet = new HashSet<PQSMod_LandClassScatterQuad>();
            foreach (PQSMod_LandClassScatterQuad holder in inUse)
            {
                // Unity's == also tells a destroyed object from a live one.
                if (holder == null)
                {
                    pool.InUseDestroyed++;
                    continue;
                }
                inUseSet.Add(holder);
                members.Add(holder);
                if (holder.quad == null)
                {
                    pool.InUseWithoutQuad++;
                }
                else if (!holder.quad.isActive)
                {
                    pool.InUseOnInactiveQuad++;
                }
                else if (holder.quad.sphereRoot != sphere)
                {
                    pool.InUseOnOtherBody++;
                }
            }

            foreach (PQSMod_LandClassScatterQuad holder in free)
            {
                if (holder == null)
                {
                    pool.FreeDestroyed++;
                    continue;
                }
                if (inUseSet.Contains(holder))
                {
                    pool.InBoth++;
                }
                members.Add(holder);
                if (container == null || holder.transform.parent != container.transform)
                {
                    pool.FreeOutsideContainer++;
                }
                if (holder.quad != null)
                {
                    pool.FreeWithQuad++;
                }
            }
            return pool;
        }

        /// <summary>Adds the pool to a record, on a line of its own.</summary>
        public void Log(RecordLog log)
        {
            log.Line(1, "{0} '{1}': in use {2} (counter {3}): {4} destroyed, {5} without a quad, {6} on an inactive quad,"
                + " {7} on a quad of another body; free {8} (counter {9}): {10} destroyed, {11} outside the pool's"
                + " container, {12} still with a quad; {13} both in use and free",
                BodyName, ScatterName, InUse, InUseCounter, InUseDestroyed, InUseWithoutQuad, InUseOnInactiveQuad,
                InUseOnOtherBody, Free, FreeCounter, FreeDestroyed, FreeOutsideContainer, FreeWithQuad, InBoth);
        }

        private static FieldInfo Field(string name)
        {
            return typeof(PQSLandControl.LandClassScatter).GetField(name, PRIVATE);
        }
    }
}
