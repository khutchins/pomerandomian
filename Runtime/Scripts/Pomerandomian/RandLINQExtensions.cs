using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pomerandomian {
	public static class RandLINQExtensions {
		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, IRandom rand) {
			T[] arr = source.ToArray();

			for (int i = arr.Length - 1; i >= 0; i--) {
				int swap = rand.Next(i + 1);
				yield return arr[swap];
				arr[swap] = arr[i];
			}
		}

        /// <summary>
        /// Reservoir sampling returns up to num items chosen uniformly at random from 
        /// an input source of any size.
        /// </summary>
        public static IEnumerable<T> ReservoirSample<T>(this IEnumerable<T> source, IRandom rand, int num) {
            if (source == null) yield break;
            if (num <= 0) yield break;

            List<T> reservoir = new List<T>(num);
            int i = 0;

            foreach (var item in source) {
                if (i < num) {
                    reservoir.Add(item);
                } else {
                    // Randomly select int in [0, items read]. If it collides with an index in the reservoir,
                    // replace that value.
                    int j = rand.Next(i + 1);
                    if (j < num) {
                        reservoir[j] = item;
                    }
                }
                i++;
            }

            for (int r = 0; r < reservoir.Count; r++) {
                yield return reservoir[r];
            }
        }
    }
}