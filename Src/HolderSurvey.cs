using System.Collections.Generic;
using com.github.lhervier.ksp.rockprecisionfixdiag.measures;
using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag
{
    /// <summary>
    /// The survey of the holders of rocks (Mod+Shift+F6): every pool of holders of the scene, and every holder
    /// that belongs to none, under a record number that keeps counting across scene changes.
    /// </summary>
    internal static class HolderSurvey
    {
        // Static: a reload destroys the addon, and the numbers must go on from one load to the next.
        private static int recordCount;

        /// <summary>Takes a reading of the holders and writes it to KSP.log under the next record number.</summary>
        public static void Record()
        {
            if (!PoolMeasure.Readable)
            {
                Debug.LogError(Constants.LOG_PREFIX + "The scatter pools of this version of KSP cannot be read:"
                    + " nothing recorded.");
                return;
            }
            recordCount++;
            int number = recordCount;

            // The pools, body by body and scatter by scatter. Those stock has not created, or holding nothing,
            // are left out: every body has some, and most are empty at any time.
            List<PoolMeasure> pools = new List<PoolMeasure>();
            HashSet<PQSMod_LandClassScatterQuad> members = new HashSet<PQSMod_LandClassScatterQuad>();
            foreach (PQSLandControl control in Resources.FindObjectsOfTypeAll<PQSLandControl>())
            {
                if (!control.gameObject.scene.IsValid() || control.sphere == null || control.scatters == null)
                {
                    continue;
                }
                foreach (PQSLandControl.LandClassScatter scatter in control.scatters)
                {
                    PoolMeasure pool = scatter != null ? PoolMeasure.Take(scatter, control.sphere, members) : null;
                    if (pool != null && (pool.InUse + pool.InUseCounter + pool.Free + pool.FreeCounter) > 0)
                    {
                        pools.Add(pool);
                    }
                }
            }

            // Every holder of the scene, inactive ones included, must belong to a pool.
            List<PQSMod_LandClassScatterQuad> strays = new List<PQSMod_LandClassScatterQuad>();
            foreach (PQSMod_LandClassScatterQuad holder in Resources.FindObjectsOfTypeAll<PQSMod_LandClassScatterQuad>())
            {
                // Prefabs and other assets belong to no scene.
                if (holder.gameObject.scene.IsValid() && !members.Contains(holder))
                {
                    strays.Add(holder);
                }
            }

            RecordLog log = new RecordLog();
            log.Line(0, "Holder record {0}: every pool of holders of rocks, one per body and kind of scatter", number);
            int inUse = 0;
            int free = 0;
            int irregular = strays.Count;
            foreach (PoolMeasure pool in pools)
            {
                pool.Log(log);
                inUse += pool.InUse;
                free += pool.Free;
                irregular += pool.Irregular;
            }
            foreach (PQSMod_LandClassScatterQuad holder in strays)
            {
                Transform parent = holder.transform.parent;
                log.Line(1, "holder '{0}' in no pool, under '{1}'", HolderFinder.ScatterNameOf(holder),
                    parent != null ? parent.name : "nothing");
            }
            // Last, so that it is what stays in sight of whoever follows the log as it grows.
            log.Line(0, "End of holder record {0}: {1} pools, {2} holders in use, {3} free, {4} in no pool;"
                + " {5} broken rules", number, pools.Count, inUse, free, strays.Count, irregular);
            log.Write();
        }
    }
}
