using UnityEngine;

namespace com.github.lhervier.ksp.rockprecisionfixdiag
{
    /// <summary>
    /// Chooses which vertices of a rock to measure: a few, spread over the whole rock.
    /// </summary>
    internal static class VertexPicker
    {
        /// <summary>
        /// Picks <paramref name="count"/> vertices of a model spread as far apart from each other as possible,
        /// starting with the lowest one (smallest y), and returns their indices in the order they were picked.
        /// Returns every index, in order, when the model has no more than <paramref name="count"/> vertices,
        /// and fewer than <paramref name="count"/> indices when the model has fewer distinct positions. The
        /// same model always gives the same indices.
        /// </summary>
        public static int[] PickSpread(Vector3[] model, int count)
        {
            if (model.Length <= count)
            {
                int[] all = new int[model.Length];
                for (int i = 0; i < model.Length; i++)
                {
                    all[i] = i;
                }
                return all;
            }

            // The lowest vertex of the model first: stock stands every rock up along the y axis of its model.
            int first = 0;
            for (int i = 1; i < model.Length; i++)
            {
                if (model[i].y < model[first].y)
                {
                    first = i;
                }
            }

            // Then, again and again, the vertex furthest from all those already picked. nearest[i] holds the
            // squared distance from vertex i to the nearest picked vertex: 0 for the picked ones, so that they
            // are never picked twice. On a tie, the lowest index wins, so the choice is the same at every load.
            int[] picked = new int[count];
            picked[0] = first;
            double[] nearest = new double[model.Length];
            for (int i = 0; i < model.Length; i++)
            {
                nearest[i] = SquaredDistance(model[i], model[first]);
            }
            int pickedCount = 1;
            while (pickedCount < count)
            {
                int furthest = 0;
                for (int i = 1; i < model.Length; i++)
                {
                    if (nearest[i] > nearest[furthest])
                    {
                        furthest = i;
                    }
                }
                // Every vertex left stands on one already picked: the model has no more distinct positions.
                if (nearest[furthest] == 0.0)
                {
                    break;
                }

                picked[pickedCount++] = furthest;
                for (int i = 0; i < model.Length; i++)
                {
                    double distance = SquaredDistance(model[i], model[furthest]);
                    if (distance < nearest[i])
                    {
                        nearest[i] = distance;
                    }
                }
            }

            if (pickedCount == count)
            {
                return picked;
            }
            int[] shorter = new int[pickedCount];
            System.Array.Copy(picked, shorter, pickedCount);
            return shorter;
        }

        /// <summary>The squared distance between two points, in double precision.</summary>
        private static double SquaredDistance(Vector3 a, Vector3 b)
        {
            double x = (double)a.x - b.x;
            double y = (double)a.y - b.y;
            double z = (double)a.z - b.z;
            return x * x + y * y + z * z;
        }
    }
}
