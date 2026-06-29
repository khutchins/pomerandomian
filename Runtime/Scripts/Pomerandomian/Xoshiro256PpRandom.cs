using System;

namespace Pomerandomian {
	/// <summary>
	/// A xoshiro256++ PRNG that subclasses IRandom. Its primitives (Next(int, int) and
	/// NextDouble()) are bit-identical across platforms and runtimes, so a given seed reproduces
	/// the same sequence everywhere (unlike SystemRandom).
	///
	/// Note that IRandom helpers built on floating-point weights can still vary across platforms
	/// regardless of this generator; see the remarks on the float-odds FromWithOdds overloads, and
	/// prefer integer odds when you need cross-platform reproducibility.
	///
	/// Reference: https://prng.di.unimi.it/.
	/// </summary>
	public sealed class Xoshiro256PpRandom : ISeededRandom, ISeeded<ulong> {

		private ulong _s0, _s1, _s2, _s3;
		private readonly ulong _seed64;
		private readonly object _rawSeed;

		/// <summary>
		/// The 64-bit seed this generator runs on. For a string seed this is the hash of the string;
		/// RawSeed holds the original string.
		/// </summary>
		public ulong Seed => _seed64;

		public object RawSeed => _rawSeed;

		/// <summary>
		/// xoshiro256++ RNG seeded with Environment.TickCount.
		/// </summary>
		public Xoshiro256PpRandom() : this(Environment.TickCount) { }

		/// <summary>
		/// xoshiro256++ RNG seeded with the given seed.
		/// </summary>
		public Xoshiro256PpRandom(int seed) {
			_rawSeed = seed;
			_seed64 = (ulong)seed;
			SeedState(_seed64);
		}

		/// <summary>
		/// xoshiro256++ RNG seeded with the given seed string hashed to 64 bits. RawSeed holds the
		/// faithful string.
		/// </summary>
		public Xoshiro256PpRandom(string seedStr) {
			_rawSeed = seedStr;
			_seed64 = Seeds.ULong(seedStr);
			SeedState(_seed64);
		}

		/// <summary>
		/// xoshiro256++ RNG seeded with a full 64-bit seed. This is the generator's native seed width,
		/// so a value from Seed round-trips exactly: new Xoshiro256PpRandom(rng.Seed) reproduces rng's
		/// sequence. Also used internally by ChildRandom and Split.
		/// </summary>
		public Xoshiro256PpRandom(ulong seed) {
			_rawSeed = seed;
			_seed64 = seed;
			SeedState(seed);
		}

		public int Next(int minInclusive, int maxExclusive) {
            if (minInclusive > maxExclusive) throw new ArgumentOutOfRangeException(nameof(minInclusive), "minInclusive must not be greater than maxExclusive.");
            uint range = (uint)((long)maxExclusive - minInclusive);
			if (range == 0) return minInclusive;
			return (int)(minInclusive + (long)BoundedUInt(range));
		}

		public double NextDouble() {
			// The top 53 bits give a uniformly spaced double in [0, 1). 2^53 = 9007199254740992.
			return (NextULong() >> 11) * (1.0 / 9007199254740992.0);
		}

		public long NextLong() {
			return (long)NextULong();
		}

		public IRandom ChildRandom() {
			return new Xoshiro256PpRandom(NextULong());
		}

		public ISeededRandom Split(int streamId) {
			return new Xoshiro256PpRandom(MixSeed(_seed64, streamId));
		}

		/// <summary>
		/// Advances the state and returns the next raw 64-bit value.
		/// </summary>
		private ulong NextULong() {
			unchecked {
				ulong result = Rotl(_s0 + _s3, 23) + _s0;
				ulong t = _s1 << 17;
				_s2 ^= _s0;
				_s3 ^= _s1;
				_s1 ^= _s2;
				_s0 ^= _s3;
				_s2 ^= t;
				_s3 = Rotl(_s3, 45);
				return result;
			}
		}

		/// <summary>
		/// Returns a uniform value in [0, range) using Lemire's reduction.
		/// https://lemire.me/blog/2016/06/27/a-fast-alternative-to-the-modulo-reduction/
		/// </summary>
		private uint BoundedUInt(uint range) {
			unchecked {
				ulong m = (ulong)(uint)(NextULong() >> 32) * range;
				uint low = (uint)m;
				if (low < range) {
					uint threshold = (0u - range) % range;
					while (low < threshold) {
						m = (ulong)(uint)(NextULong() >> 32) * range;
						low = (uint)m;
					}
				}
				return (uint)(m >> 32);
			}
		}

		/// <summary>
		/// Fills the 256-bit state from a single 64-bit seed using SplitMix64.
		/// </summary>
		private void SeedState(ulong seed) {
			ulong sm = seed;
			_s0 = SplitMix64(ref sm);
			_s1 = SplitMix64(ref sm);
			_s2 = SplitMix64(ref sm);
			_s3 = SplitMix64(ref sm);
			// xoshiro requires a non-zero state.
			if ((_s0 | _s1 | _s2 | _s3) == 0UL) _s0 = 1UL;
		}

		/// <summary>
		/// SplitMix64: a counter-based generator used to expand a seed into well-distributed state words.
		/// </summary>
		private static ulong SplitMix64(ref ulong state) {
			unchecked {
				state += 0x9E3779B97F4A7C15UL;
				ulong z = state;
				z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
				z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
				return z ^ (z >> 31);
			}
		}

		/// <summary>
		/// Combines the full 64-bit seed and a stream id into a new 64-bit seed by mixing the stream
		/// id in and running the result through the SplitMix64 finalizer, so adjacent or shared values
		/// (including streamId 0) do not produce correlated streams.
		/// </summary>
		private static ulong MixSeed(ulong seed, int streamId) {
			unchecked {
				ulong z = seed ^ ((uint)streamId * 0x9E3779B97F4A7C15UL);
				z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
				z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
				z ^= z >> 31;
				return z;
			}
		}

		private static ulong Rotl(ulong x, int k) {
			return (x << k) | (x >> (64 - k));
		}
	}
}
