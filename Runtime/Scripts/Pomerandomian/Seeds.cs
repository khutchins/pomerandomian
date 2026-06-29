using System;
using System.Security.Cryptography;
using System.Text;

namespace Pomerandomian {
	/// <summary>
	/// Utilities for deriving seeds from strings.
	/// </summary>
	public static class Seeds {

		/// <summary>
		/// Hashes a string into seed material (the 16-byte MD5 of its UTF-8 bytes). This is the
		/// primitive the typed helpers are built on; use those unless you specifically need more bits.
		/// </summary>
		/// <param name="seedStr">String to hash to make seed material.</param>
		/// <returns>16 bytes of seed material.</returns>
		public static byte[] Bytes(string seedStr) {
			using (MD5 md5Hash = MD5.Create()) {
				return md5Hash.ComputeHash(Encoding.UTF8.GetBytes(seedStr));
			}
		}

		/// <summary>
		/// Converts a string to a 32-bit seed.
		/// </summary>
		/// <param name="seedStr">String to hash to make a seed.</param>
		/// <returns>int value of the passed in string.</returns>
		public static int Int(string seedStr) {
			return BitConverter.ToInt32(Bytes(seedStr), 0);
		}

		/// <summary>
		/// Converts a string to a 64-bit seed.
		/// </summary>
		/// <param name="seedStr">String to hash to make a seed.</param>
		/// <returns>ulong value of the passed in string.</returns>
		public static ulong ULong(string seedStr) {
			return BitConverter.ToUInt64(Bytes(seedStr), 0);
		}
	}
}
