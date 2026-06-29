using System;

namespace Pomerandomian {
	/// <summary>
	/// Convenience helpers on IRandom for scalars, bools, enums, and dice.
	/// </summary>
	public static class RandomExtensions {

		/// <summary>
		/// Returns a random bool value.
		/// </summary>
		public static bool NextBool(this IRandom random) {
			return random.WithOdds(1, 2);
		}

		/// <summary>
		/// Returns a random bool value with the given odds.
		///
		/// Ex: WithOdds(3, 5) has a 3/5ths chance of returning true.
		/// </summary>
		/// <param name="chance">Chance that true will be returned</param>
		/// <param name="outOf">Total numbers</param>
		/// <returns>Whether or not the chance was met.</returns>
		public static bool WithOdds(this IRandom random, int chance, int outOf) {
			return random.Next(0, outOf) < chance;
		}

		/// <summary>
		/// Returns a random bool value with the given percent chance.
		/// </summary>
		/// <param name="chance">% chance that true will be returned.</param>
		public static bool WithPercentChance(this IRandom random, double chance) {
			return random.NextDouble() < chance;
		}

		/// <summary>
		/// Returns a random enum value.
		/// </summary>
		public static T FromEnum<T>(this IRandom random) {
			Array values = Enum.GetValues(typeof(T));
			return (T)values.GetValue(random.Next(values.Length));
		}

		/// <summary>
		/// Returns a random integer between 0 (inclusive) and maxExclusive (exclusive).
		/// </summary>
		public static int Next(this IRandom random, int maxExclusive) {
			return random.Next(0, maxExclusive);
		}

		/// <summary>
		/// Returns a random long between 0 (inclusive) and maxExclusive (exclusive).
		/// </summary>
		public static long NextLong(this IRandom random, long maxExclusive) {
			return random.NextLong(0, maxExclusive);
		}

		/// <summary>
		/// Returns a random long between minInclusive (inclusive) and maxExclusive (exclusive).
		/// </summary>
		public static long NextLong(this IRandom random, long minInclusive, long maxExclusive) {
			if (minInclusive > maxExclusive) throw new ArgumentOutOfRangeException(nameof(minInclusive), "minInclusive must not be greater than maxExclusive.");
			unchecked {
				ulong range = (ulong)maxExclusive - (ulong)minInclusive;
				if (range == 0) return minInclusive;
				// Rejection sampling.
				ulong rejectBelow = (0UL - range) % range;
				ulong r;
				do {
					r = (ulong)random.NextLong();
				} while (r < rejectBelow);
				return minInclusive + (long)(r % range);
			}
		}

		/// <summary>
		/// Returns a double between min (inclusive) and max (exclusive).
		/// </summary>
		public static double NextDouble(this IRandom random, double min, double max) {
			if (min > max) throw new ArgumentOutOfRangeException(nameof(min), "min must not be greater than max.");
			if (min == max) return min;
			// Split the multiply-add into separate roundings so it's more likely to be deterministic.
			double scaled = random.NextDouble() * (max - min);
			double result = scaled + min;
			return result < max ? result : NextDown(max);
		}

		/// <summary>
		/// Returns a float between min (inclusive) and max (exclusive).
		/// </summary>
		public static float Next(this IRandom random, float min, float max) {
			if (min > max) throw new ArgumentOutOfRangeException(nameof(min), "min must not be greater than max.");
			if (min == max) return min;
			// Split the multiply-add into separate roundings so it's more likely to be deterministic.
			double scaled = random.NextDouble() * ((double)max - min);
			float result = (float)(scaled + min);
			return result < max ? result : NextDown(max);
		}

		/// <summary>
		/// Largest double strictly below x.
		/// </summary>
		private static double NextDown(double x) {
			long bits = BitConverter.DoubleToInt64Bits(x);
			if (x > 0.0) bits -= 1;
			else if (x < 0.0) bits += 1;
			else bits = unchecked((long)0x8000000000000001L); // -double.Epsilon
			return BitConverter.Int64BitsToDouble(bits);
		}

		/// <summary>
		/// Largest float strictly below x.
		/// </summary>
		private static float NextDown(float x) {
			int bits = BitConverter.ToInt32(BitConverter.GetBytes(x), 0);
			if (x > 0f) bits -= 1;
			else if (x < 0f) bits += 1;
			else bits = unchecked((int)0x80000001); // -float.Epsilon
			return BitConverter.ToSingle(BitConverter.GetBytes(bits), 0);
		}

		/// <summary>
		/// Rolls the given dice configuration and returns the result.
		/// </summary>
		public static int Roll(this IRandom random, Dice dice) {
			return dice.Roll(random);
		}
	}
}
