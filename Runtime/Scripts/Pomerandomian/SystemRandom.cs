using System;

namespace Pomerandomian {
	/// <summary>
	/// Wrapper of System.Random that subclasses IRandom. Note that this could potentially change across .NET versions and across platforms, depending on MS's whims.
	/// 
	/// From the docs here https://docs.microsoft.com/en-us/dotnet/api/system.random?redirectedfrom=MSDN&view=net-5.0: The implementation of the random number generator in the Random class isn't guaranteed to remain the same across major versions of the .NET Framework. As a result, you shouldn't assume that the same seed will result in the same pseudo-random sequence in different versions of the .NET Framework.
	/// </summary>
	public sealed class SystemRandom : ISeededRandom, ISeeded<int> {

		private System.Random _random;
		private int _seed;
		private object _rawSeed;
		private readonly byte[] _longBuffer = new byte[8];

		public int Seed => _seed;

		public object RawSeed => _rawSeed;

		/// <summary>
		/// System.Random RNG seeded with Environment.TickCount.
		/// </summary>
		public SystemRandom() {
			_rawSeed = _seed = Environment.TickCount;
			_random = new System.Random(_seed);
		}

		/// <summary>
		/// System.Random RNG seeded with the given seed.
		/// </summary>
		public SystemRandom(int seed) {
			_rawSeed = _seed = seed;
			_random = new System.Random(_seed);
		}

		/// <summary>
		/// System.Random RNG seeded with the given seed string hashed to get an int.
		/// </summary>
		public SystemRandom(string seedStr) {
			_rawSeed = seedStr;
			_seed = Seeds.Int(seedStr);
			_random = new System.Random(_seed);
		}

		public int Next(int minInclusive, int maxExclusive) {
			return _random.Next(minInclusive, maxExclusive);
		}

		public double NextDouble() {
			return _random.NextDouble();
		}

		public long NextLong() {
			_random.NextBytes(_longBuffer);
			return BitConverter.ToInt64(_longBuffer, 0);
		}

		public IRandom ChildRandom() {
			return new SystemRandom(Next(0, int.MaxValue));
		}

		public IRandom Split(int streamId) {
			return new SystemRandom(unchecked(_seed + streamId));
		}
	}
}