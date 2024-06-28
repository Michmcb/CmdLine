namespace CmdLineNet
{
	using System.Collections;
	using System.Collections.Generic;
	/// <summary>
	/// Enumerates an enumerator of <see cref="IdCount{TId}"/>, duplicating the value as many times as <see cref="IdCount{TId}.Max"/>.
	/// This allows you to see what the expected sequence of values is.
	/// </summary>
	/// <typeparam name="TId">The type of the ID given to each Argument.</typeparam>
	public sealed class DuplicatingValuesEnumerator<TId> : IEnumerator<TId> where TId : struct
	{
		private int max;
		private int num;
		private readonly IEnumerator<IdCount<TId>> values;
		/// <summary>
		/// Creates a new instance wrapping <paramref name="values"/>. Disposes <paramref name="values"/> when this is disposed.
		/// </summary>
		/// <param name="values">The enumerator to wrap.</param>
		public DuplicatingValuesEnumerator(IEnumerator<IdCount<TId>> values)
		{
			this.values = values;
		}
		/// <inheritdoc/>
		public TId Current { get; private set; }
		/// <inheritdoc/>
		object IEnumerator.Current => Current;
		/// <inheritdoc/>
		public bool MoveNext()
		{
			if (num >= max)
			{
				if (values.MoveNext())
				{
					Current = values.Current.Id;
					num = 1;
					max = values.Current.Max;
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
			max = 0;
			Current = default;
		}
		/// <inheritdoc/>
		public void Dispose() { values.Dispose(); }
	}
}