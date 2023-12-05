namespace CmdLineNet
{
	using System.Collections;
	using System.Collections.Generic;
	/// <summary>
	/// Enumerates an enumerator of <see cref="ArgCount{TId}"/>, duplicating the value as many times as <see cref="ArgCount{TId}.Max"/>.
	/// This allows you to see what the expected sequence of values is.
	/// </summary>
	/// <typeparam name="TId">The type of the ID given to each Argument.</typeparam>
	public sealed class DuplicatingValuesEnumerator<TId> : IEnumerator<ArgCount<TId>> where TId : struct
	{
		private int num;
		private readonly IEnumerator<ArgCount<TId>> values;
		/// <summary>
		/// Creates a new instance wrapping <paramref name="values"/>. Disposes <paramref name="values"/> when this is disposed.
		/// </summary>
		/// <param name="values">The enumerator to wrap.</param>
		public DuplicatingValuesEnumerator(IEnumerator<ArgCount<TId>> values)
		{
			this.values = values;
		}
		/// <inheritdoc/>
		public ArgCount<TId> Current { get; private set; }
		/// <inheritdoc/>
		object IEnumerator.Current => Current;
		/// <inheritdoc/>
		public bool MoveNext()
		{
			if (num >= Current.Max)
			{
				if (values.MoveNext())
				{
					Current = values.Current;
					num = 1;
					return true;
				}
				else
				{
					num = int.MaxValue;
					return false;
				}
			}
			else
			{
				num++;
				return true;
			}
		}
		/// <inheritdoc/>
		public void Reset()
		{
			values.Reset();
			num = 0;
			Current = default;
		}
		/// <inheritdoc/>
		public void Dispose() { values.Dispose(); }
	}
}