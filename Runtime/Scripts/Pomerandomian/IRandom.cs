using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Pomerandomian {
	[System.Serializable]
	public struct ObjectOdds<T> {
		public T Object;
		public int Odds;
	}

	[System.Serializable]
    public struct ObjectOddsFloat<T> {
        public T Object;
        public float Odds;
    }

	/// <summary>
	/// An interface for a RNG that implements several helper functions.
	/// </summary>
	public abstract class IRandom {

		/// <summary>
		/// Returns a child random. Useful for cases where you want to be able
		/// to change one subsystem without affecting other subsystems. Just
		/// have each subsystem use a child random and they'll be isolated from
		/// one another.
		/// </summary>
		abstract public IRandom ChildRandom();

		/// <summary>
		/// Deterministically derive an independent child random from the parent.
		/// One way this can be done is by seeding the RNG with the parent seed
		/// modified by this split id.
		/// </summary>
		abstract public IRandom Split(int streamId);

        /// <summary>
        /// Returns a random integer number between minInclusive and maxExclusive.
        /// </summary>
        /// <param name="minInclusive">Minimum integer value, inclusive</param>
        /// <param name="maxExclusive">Maximum integer value, exclusive</param>
        /// <returns>Integer between min (inclusive) and max (exclusive).</returns>
        abstract public int Next(int minInclusive, int maxExclusive);

		/// <summary>
		///  Gets a random double number between 0.0 (inclusive) and 1.0 (exclusive)
		/// </summary>
		/// <returns>Double between 0.0 (inclusive) and 1.0 (exclusive)</returns>
		abstract public double NextDouble();

		/// <summary>
		/// Returns the integer seed used to initialize this random. Useful for being able
		/// to preserve state without having to remember which seed you passed in.
		/// </summary>
		/// <returns></returns>
		abstract public int Seed { get; }

		/// <summary>
		/// The raw seed used to initialize this random. It can be integer, string, or 
		/// another type based on what the implementation supports.
		/// </summary>
		abstract public object RawSeed { get; }

		/// <summary>
		/// Hashes a string into seed material (the 16-byte MD5 of its UTF-8 bytes). This is the
		/// primitive the typed StringToSeed helpers are built on; use those unless you specifically
		/// need more bits than they expose. MD5 + UTF-8 are byte-exact across platforms, so the
		/// result is deterministic everywhere. Standard MD5 security caveats apply: if you're doing
		/// anything security-sensitive, don't use this.
		/// </summary>
		/// <param name="seedStr">String to hash to make seed material.</param>
		/// <returns>16 bytes of seed material.</returns>
		public static byte[] StringToSeedBytes(string seedStr) {
			using (MD5 md5Hash = MD5.Create()) {
				return md5Hash.ComputeHash(Encoding.UTF8.GetBytes(seedStr));
			}
		}

		/// <summary>
		/// Converts a string to an integer seed. Because this is an integer and uses
		/// an MD5 hash, it's possible for players to engineer hash collisions against
		/// a known seed.
		/// </summary>
		/// <param name="seedStr">String to hash to make a seed.</param>
		/// <returns>int value of the passed in string.</returns>
		public static int StringToSeed(string seedStr) {
			return BitConverter.ToInt32(StringToSeedBytes(seedStr), 0);
		}

		/// <summary>
		/// Converts a string to a 64-bit seed, using more of the hash than StringToSeed. Useful for
		/// implementations that can seed from a full 64-bit value (e.g. Xoshiro256PpRandom). The same
		/// MD5 collision caveats apply.
		/// </summary>
		/// <param name="seedStr">String to hash to make a seed.</param>
		/// <returns>ulong value of the passed in string.</returns>
		public static ulong StringToSeed64(string seedStr) {
			return BitConverter.ToUInt64(StringToSeedBytes(seedStr), 0);
		}

		/// <summary>
		/// Returns a random bool value.
		/// </summary>
		public bool NextBool() {
			return WithOdds(1, 2);
		}

		/// <summary>
		/// Returns a random bool value with the given odds.
		/// 
		/// Ex: WithOdds(3, 5) has a 3/5ths chance of returning true.
		/// </summary>
		/// <param name="chance">Chance that true will be returned</param>
		/// <param name="outOf">Total numbers</param>
		/// <returns>Whether or not the chance was met.</returns>
		public bool WithOdds(int chance, int outOf) {
			return Next(0, outOf) < chance;
		}

		/// <summary>
		/// Returns a random bool value with the given percent chance.
		/// </summary>
		/// <param name="chance">% chance that true will be returned.</param>
		public bool WithPercentChance(double chance) {
			return NextDouble() < chance;
		}

        /// <summary>
        /// Returns a random item from the given list.
        /// </summary>
        [Obsolete("Use From() instead.")]
        public T FromList<T>(IList<T> list) {
			return list[Next(0, list.Count)];
		}

        /// <summary>
        /// Returns a random item from the given array.
        /// </summary>
        [Obsolete("Use From() instead.")]
        public T FromArray<T>(T[] array) {
			return array[Next(0, array.Length)];
		}

		public T From<T>(IReadOnlyList<T> list) {
			return list[Next(0, list.Count)];
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
        public T FromArrayWithOdds<T>(T[] array, int[] odds) {
			return FromWithOdds<T>(array, odds);
		}

        /// <summary>
        /// Returns a random item from a given list, using the provided weighted odds.
        /// 
        /// array and odds must be of the same length, or an exception will be thrown.
        /// </summary>
        /// <param name="array">Parameter array</param>
        /// <param name="odds">Odds array</param>
        /// <returns>A random item from array, using odds.</returns>
        public T FromWithOdds<T>(IReadOnlyList<T> list, IReadOnlyList<int> odds) {
            if (list == null || list.Count == 0) return default;
            if (list.Count != odds.Count) throw new ArgumentException("Array lengths do not match");
            var allOdds = odds.Sum();
            if (allOdds < 1) return list[0];
            var num = Next(0, allOdds);

            int sum = 0;

            for (int i = 0; i < list.Count; i++) {
                sum += odds[i];
                if (num < sum) return list[i];
            }
            return list[list.Count - 1];
        }

        /// <summary>
        /// Returns a random item from a given array, using the provided weighted odds.
        /// 
        /// array and odds must be of the same length, or an exception will be thrown.
        /// </summary>
        /// <param name="array">Parameter array</param>
        /// <param name="odds">Odds array</param>
        /// <returns>A random item from array, using odds.</returns>
        /// <remarks>
        /// This overload sums and compares floating-point weights, which is not guaranteed to be
        /// bit-identical across platforms or runtimes, even when the underlying generator is
        /// deterministic. Use the integer-odds overload if you need cross-platform reproducibility
        /// (e.g. shared seeds or replays).
        /// </remarks>
        public T FromWithOdds<T>(IReadOnlyList<T> list, IReadOnlyList<float> odds) {
            if (list == null || list.Count == 0) return default;
            if (list.Count != odds.Count) throw new ArgumentException("Array lengths do not match");
            var allOdds = odds.Sum();
            if (allOdds <= 0) return list[0];
            var num = Next(0, allOdds);

            float sum = 0;

            for (int i = 0; i < list.Count; i++) {
                sum += odds[i];
                if (num < sum) return list[i];
            }
            return list[list.Count - 1];
        }

		/// <summary>
		/// Returns a random item from a given array, using the provided weighted odds.
		/// ObjectOdds can be useful in the inspector.
		/// </summary>
		/// <param name="objectOdds">Struct that binds objects and odds.</param>
		/// <returns>A random item from array, using odds.</returns>
		[Obsolete("Use FromWithOdds instead.")]
        public T FromArrayWithOdds<T>(ObjectOdds<T>[] objectOdds) {
			return FromWithOdds(objectOdds);
		}

        /// <summary>
        /// Returns a random item from a given list, using the provided weighted odds.
        /// ObjectOdds can be useful in the inspector.
        /// </summary>
        /// <param name="objectOdds">Struct that binds objects and odds.</param>
        /// <returns>A random item from array, using odds.</returns>
        public T FromWithOdds<T>(IReadOnlyList<ObjectOdds<T>> objectOdds) {
            if (objectOdds == null || objectOdds.Count == 0) return default;
            int allOdds = objectOdds.Sum(x => x.Odds);
            if (allOdds < 1) return objectOdds[0].Object;

            int num = Next(0, allOdds);
            int sum = 0;

            for (int i = 0; i < objectOdds.Count; i++) {
                sum += objectOdds[i].Odds;
                if (num < sum) return objectOdds[i].Object;
            }
            return objectOdds[objectOdds.Count - 1].Object;

        }

        /// <summary>
        /// Returns a random item from a given array, using the provided weighted odds.
        /// ObjectOddsFloat can be useful in the inspector.
        /// </summary>
        /// <param name="objectOdds">Struct that binds objects and odds.</param>
        /// <returns>A random item from array, using odds.</returns>
        /// <remarks>
        /// This overload sums and compares floating-point weights, which is not guaranteed to be
        /// bit-identical across platforms or runtimes, even when the underlying generator is
        /// deterministic. Use an integer-odds overload if you need cross-platform reproducibility
        /// (e.g. shared seeds or replays).
        /// </remarks>
        public T FromWithOdds<T>(IReadOnlyList<ObjectOddsFloat<T>> objectOdds) {
            if (objectOdds == null || objectOdds.Count == 0) return default;
            float allOdds = objectOdds.Sum(x => x.Odds);
            if (allOdds <= 0) return objectOdds[0].Object;

            float num = Next(0, allOdds);
            float sum = 0;

            for (int i = 0; i < objectOdds.Count; i++) {
                sum += objectOdds[i].Odds;
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
        /// <param name="list">Enumerable </param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public IEnumerable<T> PickFromList<T>(IEnumerable<T> list, int amount) {
			List<T> selected = new List<T>();
			int total = list.Count();

			int idx = 0;
			foreach (T obj in list) {
				if (WithOdds(amount - selected.Count, total - idx)) {
					selected.Add(obj);
				}
				idx++;
				if (selected.Count >= amount) break;
			}
			return selected;
		}

		/// <summary>
		/// Returns a random enum value.
		/// </summary>
		public T FromEnum<T>() {
			Array values = Enum.GetValues(typeof(T));
			return (T)values.GetValue(Next(values.Length));
		}

		/// <summary>
		/// Returns a random integer between 0 (inclusive) and maxExclusive (exclusive).
		/// </summary>
		/// <returns></returns>
		public int Next(int maxExclusive) {
			return Next(0, maxExclusive);
		}

		/// <summary>
		/// Returns a double between min (inclusive) and max (exclusive).
		/// </summary>
		public double NextDouble(double min, double max) {
			// Split the multiply-add into separate roundings so it's more likely to be deterministic.
			double scaled = NextDouble() * (max - min);
			return scaled + min;
		}

		/// <summary>
		/// Returns a float between min (inclusive) and max (exclusive).
		/// </summary>
		public float Next(float min, float max) {
			// Split the multiply-add into separate roundings so it's more likely to be deterministic.
			double scaled = NextDouble() * ((double)max - min);
			double shifted = scaled + min;
			return (float)shifted;
		}

		/// <summary>
		/// Rolls the given dice configuration and returns the result.
		/// </summary>
		public int Roll(Dice dice) {
			return dice.Roll(this);
		}
	}
}