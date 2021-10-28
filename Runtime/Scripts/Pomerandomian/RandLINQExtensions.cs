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
	}
}