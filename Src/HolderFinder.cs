using System.Collections.Generic;
using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag
{
    /// <summary>
    /// Finds the holders of rocks (PQSMod_LandClassScatterQuad) in the scene, wherever they hang.
    /// </summary>
    internal static class HolderFinder
    {
        /// <summary>
        /// Every holder of rocks attached to a quad of a terrain sphere, inactive ones included, grouped by the
        /// quad it is attached to. Each holder appears once, and each quad carries one holder per kind of
        /// scatter on it.
        /// </summary>
        public static Dictionary<PQ, List<PQSMod_LandClassScatterQuad>> Find(PQS sphere)
        {
            // A quad knows nothing of its holders: only a holder points to its quad, so the holders are searched
            // for and grouped under their quad.
            Dictionary<PQ, List<PQSMod_LandClassScatterQuad>> holders =
                new Dictionary<PQ, List<PQSMod_LandClassScatterQuad>>();
            HashSet<PQSMod_LandClassScatterQuad> seen = new HashSet<PQSMod_LandClassScatterQuad>();
            // Inactive holders are included: a holder is placed as soon as its quad is built, and only
            // shown once the quad is visible.
            // Stock hangs every holder under the sphere, in a "Scatter <name>" object.
            Collect(sphere.transform, sphere, holders, seen);
            // A mod may instead hang a holder under its own quad. The most detailed quads, the only ones
            // carrying rocks, are kept under a storage object which is not under the sphere when the scene
            // has a LocalSpace, so both places have to be searched. When the scene has none, the storage is
            // under the sphere and its holders were found above: seen keeps them from being added twice.
            if (sphere.LocalSpacePQStorage != null)
            {
                Collect(sphere.LocalSpacePQStorage.transform, sphere, holders, seen);
            }
            return holders;
        }

        /// <summary>
        /// Adds to <paramref name="holders"/>, under its quad, every holder under a transform that is attached
        /// to a quad of the sphere and not in <paramref name="seen"/> yet, and adds it to
        /// <paramref name="seen"/>.
        /// </summary>
        private static void Collect(Transform root, PQS sphere, Dictionary<PQ, List<PQSMod_LandClassScatterQuad>> holders,
            HashSet<PQSMod_LandClassScatterQuad> seen)
        {
            foreach (PQSMod_LandClassScatterQuad rocks in root.GetComponentsInChildren<PQSMod_LandClassScatterQuad>(true))
            {
                // A holder waiting in its pool has no quad. The storage of the most detailed quads is shared
                // by every body, so holders of other bodies may be found there too.
                if (rocks.quad == null || rocks.quad.sphereRoot != sphere)
                {
                    continue;
                }
                if (!seen.Add(rocks))
                {
                    continue;
                }
                if (!holders.TryGetValue(rocks.quad, out List<PQSMod_LandClassScatterQuad> quadHolders))
                {
                    quadHolders = new List<PQSMod_LandClassScatterQuad>();
                    holders.Add(rocks.quad, quadHolders);
                }
                quadHolders.Add(rocks);
            }
        }
    }
}
