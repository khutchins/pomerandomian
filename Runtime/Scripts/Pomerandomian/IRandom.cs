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
	/// A capability for seeded RNGs.
	/// </summary>
	public interface ISeeded {
		/// <summary>
		/// The raw seed used to initialize this random. Use it to display or faithfully reconstruct the source.
		/// </summary>
		object RawSeed { get; }

		/// <summary>
		/// Deterministically derives an independent substream as a pure function of the seed and the
		/// stream id. Unlike ChildRandom, it doesn't consume from or depend on the order of the
		/// parent, so it's ideal for giving each entity (e.g. a spawn index) its own reproducible
		/// stream. If this is called on the same random with the same streamId, it will produce the
		/// same substream.
		/// </summary>
		ISeededRandom Split(int streamId);

		/// <summary>
		/// Returns a new generator of the same type seeded with this generator's seed, positioned at
		/// the start of the sequence.
		/// </summary>
		ISeededRandom WithSameSeed();
	}

	public interface ISeeded<out TSeed> : ISeeded {
		TSeed Seed { get; }
	}

	/// <summary>
	/// An interface for a RNG. Seeded randoms implement ISeeded (optionally typed).
	/// Extension functions provide additional functionality.
	/// </summary>
	public interface IRandom {

		/// <summary>
		/// Returns a child random. Useful for cases where you want to be able
		/// to change one subsystem without affecting other subsystems. Just
		/// have each subsystem use a child random and they'll be isolated from
		/// one another.
		/// </summary>
		IRandom ChildRandom();

		/// <summary>
		/// Returns a random integer number between minInclusive and maxExclusive.
		/// </summary>
		/// <param name="minInclusive">Minimum integer value, inclusive</param>
		/// <param name="maxExclusive">Maximum integer value, exclusive</param>
		/// <returns>Integer between min (inclusive) and max (exclusive).</returns>
		int Next(int minInclusive, int maxExclusive);

		/// <summary>
		///  Gets a random double number between 0.0 (inclusive) and 1.0 (exclusive)
		/// </summary>
		/// <returns>Double between 0.0 (inclusive) and 1.0 (exclusive)</returns>
		double NextDouble();

		/// <summary>
		/// Returns a random 64-bit integer spanning the full range of long. Ranged helpers are
		/// provided as extension methods.
		/// </summary>
		long NextLong();
	}

	/// <summary>
	/// A seeded random you can both generate from and derive substreams of: the combination of
	/// IRandom and ISeeded. Use this as a parameter or field type when code needs to both pull
	/// values and Split, instead of taking an IRandom and casting to ISeeded.
	/// </summary>
	public interface ISeededRandom : IRandom, ISeeded { }
}
