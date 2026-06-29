using System;
using System.Collections.Generic;
using System.Linq;

namespace Pomerandomian {
	/// <summary>
	/// Convenience helpers on IRandom for picking items from collections.
	/// </summary>
	public static class RandomPickExtensions {

		/// <summary>
		/// Returns a random item from the given list.
		/// </summary>
		[Obsolete("Use From() instead.")]
		public static T FromList<T>(this IRandom random, IList<T> list) {
			return list[random.Next(0, list.Count)];
		}

		/// <summary>
		/// Returns a random item from the given array.
		/// </summary>
		[Obsolete("Use From() instead.")]
		public static T FromArray<T>(this IRandom random, T[] array) {
			return array[random.Next(0, array.Length)];
		}

		/// <summary>
		/// Returns a random item from the given list.
		/// </summary>
		public static T From<T>(this IRandom random, IReadOnlyList<T> list) {
			return list[random.Next(0, list.Count)];
		}

		/// <summary>
		/// Returns a random item from a given array, using the provided weighted odds.
		///
		/// array and odds must be of the same length, or an exception will be thrown.
		/// </summary>
		/// <param name="array">Parameter array</param>
		/// <param name="odds">Odds array</param>
		/// <returns>A random item from array, using odds.</returns>
		[Obsolete("Use FromWithOdds instead.")]
		public static T FromArrayWithOdds<T>(this IRandom random, T[] array, int[] odds) {
			return random.FromWithOdds<T>(array, odds);
		}

		/// <summary>
		/// Returns a random item from a given list, using the provided weighted odds.
		///
		/// list and odds must be of the same length, or an exception will be thrown.
		/// </summary>
		/// <param name="list">Parameter list</param>
		/// <param name="odds">Odds list</param>
		/// <returns>A random item from list, using odds.</returns>
		public static T FromWithOdds<T>(this IRandom random, IReadOnlyList<T> list, IReadOnlyList<int> odds) {
			if (list == null) throw new ArgumentNullException(nameof(list));
			if (odds == null) throw new ArgumentNullException(nameof(odds));
			if (list.Count == 0) throw new ArgumentException("Items must not be empty.", nameof(list));
			if (list.Count != odds.Count) throw new ArgumentException("Items and weights must be the same length.");
			var allOdds = odds.Sum();
			if (allOdds < 1) return list[0];
			var num = random.Next(0, allOdds);

			int sum = 0;

			for (int i = 0; i < list.Count; i++) {
				sum += odds[i];
				if (num < sum) return list[i];
			}
			return list[list.Count - 1];
		}

		/// <summary>
		/// Returns a random item from a given list, using the provided weighted odds.
		///
		/// list and odds must be of the same length, or an exception will be thrown.
		/// </summary>
		/// <param name="list">Parameter list</param>
		/// <param name="odds">Odds list</param>
		/// <returns>A random item from list, using odds.</returns>
		/// <remarks>
		/// The float weights are quantized to integers (as in <see cref="WeightedOdds{T}"/>) and the
		/// pick is made against the integer total, so the result is deterministic across platforms and
		/// runtimes. A weight more than ~10^9x smaller than the largest quantizes to zero and becomes
		/// unpickable; build a <see cref="WeightedOdds{T}"/> if you need to keep such weights selectable
		/// or to reuse the distribution across many picks.
		/// </remarks>
		public static T FromWithOdds<T>(this IRandom random, IReadOnlyList<T> list, IReadOnlyList<float> odds) {
			if (list == null) throw new ArgumentNullException(nameof(list));
			if (odds == null) throw new ArgumentNullException(nameof(odds));
			if (list.Count == 0) throw new ArgumentException("Items must not be empty.", nameof(list));
			if (list.Count != odds.Count) throw new ArgumentException("Items and weights must be the same length.");

			float max = 0f;
			for (int i = 0; i < odds.Count; i++) {
				WeightQuantization.Validate(odds[i], nameof(odds));
				if (odds[i] > max) max = odds[i];
			}
			if (max <= 0f) return list[0];
			double scale = WeightQuantization.Scale(max, nameof(odds));

			long total = 0;
			for (int i = 0; i < odds.Count; i++) total += WeightQuantization.Quantize(odds[i], scale);

			long num = random.NextLong(0, total);
			long sum = 0;
			for (int i = 0; i < list.Count; i++) {
				sum += WeightQuantization.Quantize(odds[i], scale);
				if (num < sum) return list[i];
			}
			return list[list.Count - 1];
		}

		/// <summary>
		/// Returns a random item from a given array, using the provided weighted odds.
		/// ObjectOdds can be useful in the inspector.
		/// </summary>
		[Obsolete("Use FromWithOdds instead.")]
		public static T FromArrayWithOdds<T>(this IRandom random, ObjectOdds<T>[] objectOdds) {
			return random.FromWithOdds(objectOdds);
		}

		/// <summary>
		/// Returns a random item from a given list, using the provided weighted odds.
		/// ObjectOdds can be useful in the inspector.
		/// </summary>
		/// <param name="objectOdds">Struct that binds objects and odds.</param>
		/// <returns>A random item from list, using odds.</returns>
		public static T FromWithOdds<T>(this IRandom random, IReadOnlyList<ObjectOdds<T>> objectOdds) {
			if (objectOdds == null) throw new ArgumentNullException(nameof(objectOdds));
			if (objectOdds.Count == 0) throw new ArgumentException("Items must not be empty.", nameof(objectOdds));
			int allOdds = objectOdds.Sum(x => x.Odds);
			if (allOdds < 1) return objectOdds[0].Object;

			int num = random.Next(0, allOdds);
			int sum = 0;

			for (int i = 0; i < objectOdds.Count; i++) {
				sum += objectOdds[i].Odds;
				if (num < sum) return objectOdds[i].Object;
			}
			return objectOdds[objectOdds.Count - 1].Object;
		}

		/// <summary>
		/// Returns a random item from a given list, using the provided weighted odds.
		/// ObjectOddsFloat can be useful in the inspector.
		/// </summary>
		/// <param name="objectOdds">Struct that binds objects and odds.</param>
		/// <returns>A random item from list, using odds.</returns>
		/// <remarks>
		/// The float weights are quantized to integers (as in <see cref="WeightedOdds{T}"/>) and the
		/// pick is made against the integer total, so the result is deterministic across platforms and
		/// runtimes. A weight more than ~10^9x smaller than the largest quantizes to zero and becomes
		/// unpickable; build a <see cref="WeightedOdds{T}"/> if you need to keep such weights selectable
		/// or to reuse the distribution across many picks.
		/// </remarks>
		public static T FromWithOdds<T>(this IRandom random, IReadOnlyList<ObjectOddsFloat<T>> objectOdds) {
			if (objectOdds == null) throw new ArgumentNullException(nameof(objectOdds));
			if (objectOdds.Count == 0) throw new ArgumentException("Items must not be empty.", nameof(objectOdds));

			float max = 0f;
			for (int i = 0; i < objectOdds.Count; i++) {
				WeightQuantization.Validate(objectOdds[i].Odds, nameof(objectOdds));
				if (objectOdds[i].Odds > max) max = objectOdds[i].Odds;
			}
			if (max <= 0f) return objectOdds[0].Object;
			double scale = WeightQuantization.Scale(max, nameof(objectOdds));

			long total = 0;
			for (int i = 0; i < objectOdds.Count; i++) total += WeightQuantization.Quantize(objectOdds[i].Odds, scale);

			long num = random.NextLong(0, total);
			long sum = 0;
			for (int i = 0; i < objectOdds.Count; i++) {
				sum += WeightQuantization.Quantize(objectOdds[i].Odds, scale);
				if (num < sum) return objectOdds[i].Object;
			}
			return objectOdds[objectOdds.Count - 1].Object;
		}

		/// <summary>
		/// Returns N distinct items from the provided enumerable.
		/// If amount is greater than the enumerable length, it will return everything in the list.
		/// If you use an unbounded enumerable, this will hang.
		///
		/// It will not repeat items (unless the item is in the enumerable multiple times).
		/// </summary>
		public static IEnumerable<T> PickFromList<T>(this IRandom random, IEnumerable<T> list, int amount) {
			IReadOnlyList<T> items = list as IReadOnlyList<T> ?? new List<T>(list);
			int total = items.Count;

			List<T> selected = new List<T>();
			for (int idx = 0; idx < total; idx++) {
				if (random.WithOdds(amount - selected.Count, total - idx)) {
					selected.Add(items[idx]);
				}
				if (selected.Count >= amount) break;
			}
			return selected;
		}
	}
}
