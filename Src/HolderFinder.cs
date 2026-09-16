using System.Collections.Generic;
using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag
{
    /// <summary>
    /// Finds the holders of rocks (PQSMod_LandClassScatterQuad) in the scene, wherever they hang. The lists
    /// it returns are reused from one call to the next.
    /// </summary>
    internal static class HolderFinder
    {
        // Reused from one call to the next: the terrain hierarchy holds hundreds of holders.
        private static readonly List<PQSMod_LandClassScatterQuad> FOUND = new List<PQSMod_LandClassScatterQuad>();
        private static readonly List<PQSMod_LandClassScatterQuad> HOLDERS = new List<PQSMod_LandClassScatterQuad>();
        private static readonly HashSet<PQSMod_LandClassScatterQuad> SEEN = new HashSet<PQSMod_LandClassScatterQuad>();

        /// <summary>
        /// Every holder of rocks attached to a quad of a terrain sphere, each once, inactive ones included.
        /// The list returned is only valid until the next call to Find.
        /// </summary>
        public static List<PQSMod_LandClassScatterQuad> Find(PQS sphere)
        {
            HOLDERS.Clear();
            SEEN.Clear();
            // Inactive holders are included: a holder is placed as soon as its quad is built, and only
            // shown once the quad is visible.
            // Stock hangs every holder under the sphere, in a "Scatter <name>" object.
            Collect(sphere.transform, sphere);
            // A mod may instead hang a holder under its own quad. The most detailed quads, the only ones
            // carrying rocks, are kept under a storage object which is not under the sphere when the scene
            // has a LocalSpace, so both places have to be searched. When the scene has none, the storage is
            // under the sphere and its holders were found above: SEEN keeps them from being added twice.
            if (sphere.LocalSpacePQStorage != null)
            {
                Collect(sphere.LocalSpacePQStorage.transform, sphere);
            }
            return HOLDERS;
        }

        /// <summary>
        /// Adds to the holders found every holder under a transform, attached to a quad of the sphere, and
        /// not found yet.
        /// </summary>
        private static void Collect(Transform root, PQS sphere)
        {
            FOUND.Clear();
            root.GetComponentsInChildren(true, FOUND);
            foreach (PQSMod_LandClassScatterQuad rocks in FOUND)
            {
                // A holder waiting in its pool has no quad. The storage of the most detailed quads is shared
                // by every body, so holders of other bodies may be found there too.
                if (rocks.quad == null || rocks.quad.sphereRoot != sphere)
                {
                    continue;
                }
                if (SEEN.Add(rocks))
                {
                    HOLDERS.Add(rocks);
                }
            }
        }
    }
}
