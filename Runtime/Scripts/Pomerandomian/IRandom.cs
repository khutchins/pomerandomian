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
		/// Converts a string to an integer seed. Because this is an integer and uses 
		/// an MD5 hash, it's possible for players to engineer hash collisions against
		/// a known seed.
		/// </summary>
		/// <param name="seedStr">String to hash to make a seed.</param>
		/// <returns>int value of the passed in string.</returns>
		public static int StringToSeed(string seedStr) {
			using (MD5 md5Hash = MD5.Create()) {
				byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(seedStr));
				return BitConverter.ToInt32(data, 0);
			}
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
		public T FromList<T>(IList<T> list) {
			return list[Next(0, list.Count)];
		}

		/// <summary>
		/// Returns a random item from the given array.
		/// </summary>
		public T FromArray<T>(T[] array) {
			return array[Next(0, array.Length)];
		}

		/// <summary>
		/// Returns a random item from a given array, using the provided weighted odds.
		/// 
		/// array and odds must be of the same length, or an exception will be thrown.
		/// </summary>
		/// <param name="array">Parameter array</param>
		/// <param name="odds">Odds array</param>
		/// <returns>A random item from array, using odds.</returns>
		public T FromArrayWithOdds<T>(T[] array, int[] odds) {
			if (array.Length != odds.Length) throw new ArgumentException("Array lengths do not match");
			if (array == null || array.Length == 0) return default;
			int allOdds = odds.Sum();
			if (allOdds < 1) return array[0];
			int num = Next(0, allOdds);

			int sum = 0;

			for (int i = 0; i < array.Length; i++) {
				sum += odds[i];
				if (num < sum) return array[i];
			}
			return array[array.Length - 1];
		}

		/// <summary>
		/// Returns a random item from a given array, using the provided weighted odds.
		/// ObjectOdds can be useful in the inspector.
		/// </summary>
		/// <param name="objectOdds">Struct that binds objects and odds.</param>
		/// <returns>A random item from array, using odds.</returns>
		public T FromArrayWithOdds<T>(ObjectOdds<T>[] objectOdds) {
			if (objectOdds == null || objectOdds.Length == 0) return default;
			int allOdds = objectOdds.Select(x => x.Odds).Sum();
			if (allOdds < 1) return objectOdds[0].Object;

			int num = Next(0, allOdds);
			int sum = 0;

			for (int i = 0; i < objectOdds.Length; i++) {
				sum += objectOdds[i].Odds;
				if (num < sum) return objectOdds[i].Object;
			}
			return objectOdds[objectOdds.Length - 1].Object;
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
			return NextDouble() * (max - min) + min;
		}

		/// <summary>
		/// Returns a float between min (inclusive) and max (exclusive).
		/// </summary>
		public float Next(float min, float max) {
			return (float)(NextDouble() * (max - min) + min);
		}
	}
}