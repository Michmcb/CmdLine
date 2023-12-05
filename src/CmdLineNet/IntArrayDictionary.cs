namespace CmdLineNet
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;

	/// <summary>
	/// A dictionary that wraps an array. Accessing keys in this so-called dictionary is just like indexing into an array.
	/// </summary>
	/// <typeparam name="TValue">The values stored in the array.</typeparam>
	public sealed class IntArrayDictionary<TValue> : IReadOnlyDictionary<int, TValue>
	{
		private readonly TValue[] values;
		public IntArrayDictionary(TValue[] values)
		{
			this.values = values;
		}
		/// <inheritdoc/>
		public TValue this[int key] => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException("Key was not found: " + key.ToString());
		/// <inheritdoc/>
		public IEnumerable<int> Keys => Enumerable.Range(0, values.Length);
		/// <inheritdoc/>
		public IEnumerable<TValue> Values => values;
		/// <inheritdoc/>
		public int Count => values.Length;
		/// <inheritdoc/>
		public bool ContainsKey(int key)
		{
			return key >= 0 && key < values.Length;
		}
		/// <inheritdoc/>
		public IEnumerator<KeyValuePair<int, TValue>> GetEnumerator()
		{
			for (int i = 0; i < values.Length; ++i)
			{
				yield return new KeyValuePair<int, TValue>(i, values[i]);
			}
		}
		/// <inheritdoc/>
		public bool TryGetValue(int key, [MaybeNullWhen(false)] out TValue value)
		{
			if (ContainsKey(key))
			{
				value = values[key];
				return true;
			}
			else
			{
				value = default;
				return false;
			}
		}
		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}