using System;
using System.Collections.Generic;

namespace Pomerandomian {
	internal static class WeightQuantization {
		public const long Target = 1L << 30;

		public static void Validate(float weight, string paramName) {
			if (float.IsNaN(weight) || float.IsInfinity(weight) || weight < 0f) {
				throw new ArgumentException("Weights must be finite and non-negative.", paramName);
			}
		}

		public static double Scale(float max, string paramName) {
			if (max <= 0f) throw new ArgumentException("At least one weight must be positive.", paramName);
			return Target / (double)max;
		}

		public static long Quantize(float weight, double scale) {
			return (long)(weight * scale);
		}
	}

	/// <summary>
	/// A precomputed weighted distribution over a set of items.
	///
	/// Float weights are quantized to integers at construction, so Pick is deterministic across
	/// platforms and runtimes (unlike summing floats at pick time). This can lose precision
	/// across broadly differently sized float values, but at that point random picking is going
	/// to be very biased towards the bigger number anyway.
	/// </summary>
	public sealed class WeightedOdds<T> {
		private readonly T[] _items;
		private readonly long[] _cumulative;
		private readonly long _total;

		/// <summary>Sum of all (quantized) weights. A Pick draws a value in [0, Total).</summary>
		public long Total => _total;

		public int Count => _items.Length;

		private WeightedOdds(T[] items, long[] cumulative, long total) {
			_items = items;
			_cumulative = cumulative;
			_total = total;
		}

		/// <summary>
		/// Builds a table from integer weights. Weights must be non-negative and not all zero, and the
		/// same length as items.
		/// </summary>
		public static WeightedOdds<T> FromWeights(IReadOnlyList<T> items, IReadOnlyList<int> weights) {
			ValidateLengths(items, weights);
			T[] copy = new T[items.Count];
			long[] cumulative = new long[items.Count];
			long total = 0;
			for (int i = 0; i < items.Count; i++) {
				int w = weights[i];
				if (w < 0) throw new ArgumentException("Weights must be non-negative.", nameof(weights));
				copy[i] = items[i];
				total += w;
				cumulative[i] = total;
			}
			return Build(copy, cumulative, total);
		}

		/// <summary>
		/// Builds a table from floating-point weights, quantizing them to integers. Weights must be
		/// finite (no NaN/Infinity) and non-negative, not all zero, and the same length as items.
		///
		/// A positive weight too small to register against the largest (more than ~10^9x smaller)
		/// quantizes to zero and becomes unpickable. That's faithful by default — such a weight's true
		/// selection probability is negligible, and flooring it to one unit would over-represent it by
		/// many orders of magnitude. Pass keepTinyWeightsSelectable: true to instead floor those
		/// weights at one unit so they keep a (tiny, slightly inflated) chance.
		/// </summary>
		public static WeightedOdds<T> FromWeights(IReadOnlyList<T> items, IReadOnlyList<float> weights, bool keepTinyWeightsSelectable = false) {
			ValidateLengths(items, weights);

			float max = 0f;
			for (int i = 0; i < weights.Count; i++) {
				WeightQuantization.Validate(weights[i], nameof(weights));
				if (weights[i] > max) max = weights[i];
			}
			double scale = WeightQuantization.Scale(max, nameof(weights));

			T[] copy = new T[items.Count];
			long[] cumulative = new long[items.Count];
			long total = 0;
			for (int i = 0; i < items.Count; i++) {
				long q = WeightQuantization.Quantize(weights[i], scale);
				if (keepTinyWeightsSelectable && q == 0 && weights[i] > 0f) q = 1;
				copy[i] = items[i];
				total += q;
				cumulative[i] = total;
			}
			return Build(copy, cumulative, total);
		}

		/// <summary>
		/// Builds a table from ObjectOdds.
		/// </summary>
		public static WeightedOdds<T> From(IReadOnlyList<ObjectOdds<T>> odds) {
			if (odds == null) throw new ArgumentNullException(nameof(odds));
			T[] items = new T[odds.Count];
			int[] weights = new int[odds.Count];
			for (int i = 0; i < odds.Count; i++) {
				items[i] = odds[i].Object;
				weights[i] = odds[i].Odds;
			}
			return FromWeights(items, weights);
		}

		/// <summary>
		/// Builds a table from ObjectOddsFloat.
		/// The float weights are quantized as in the float FromWeights overload.
		/// </summary>
		public static WeightedOdds<T> From(IReadOnlyList<ObjectOddsFloat<T>> odds, bool keepTinyWeightsSelectable = false) {
			if (odds == null) throw new ArgumentNullException(nameof(odds));
			T[] items = new T[odds.Count];
			float[] weights = new float[odds.Count];
			for (int i = 0; i < odds.Count; i++) {
				items[i] = odds[i].Object;
				weights[i] = odds[i].Odds;
			}
			return FromWeights(items, weights, keepTinyWeightsSelectable);
		}

		/// <summary>
		/// Picks an item, with probability proportional to its weight. O(log n).
		/// </summary>
		public T Pick(IRandom random) {
			long r = random.NextLong(0, _total);
			// Binary search for the first cumulative entry strictly greater than r.
			int lo = 0;
			int hi = _cumulative.Length - 1;
			while (lo < hi) {
				int mid = (lo + hi) >> 1;
				if (r < _cumulative[mid]) hi = mid;
				else lo = mid + 1;
			}
			return _items[lo];
		}

		private static WeightedOdds<T> Build(T[] items, long[] cumulative, long total) {
			if (total <= 0) throw new ArgumentException("At least one weight must be positive.");
			return new WeightedOdds<T>(items, cumulative, total);
		}

		private static void ValidateLengths<TWeight>(IReadOnlyList<T> items, IReadOnlyList<TWeight> weights) {
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (weights == null) throw new ArgumentNullException(nameof(weights));
			if (items.Count == 0) throw new ArgumentException("Items must not be empty.", nameof(items));
			if (items.Count != weights.Count) throw new ArgumentException("Items and weights must be the same length.");
		}
	}
}
