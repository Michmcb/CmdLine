namespace CmdLineNet
{
	using System.Collections.Generic;

	/// <summary>
	/// A group of ArgMeta, with a name.
	/// </summary>
	/// <param name="Name">The name of the group.</param>
	/// <param name="Args">All arguments.</param>
	public sealed record class ArgMetasGroup<TId>(string Name, IEnumerable<ArgMeta<TId>> Args) where TId : struct;
}