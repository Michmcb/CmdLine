namespace CmdLineNet
{
	using System.Collections.Generic;

	/// <summary>
	/// A verb.
	/// </summary>
	/// <typeparam name="T">The return type.</typeparam>
	public interface IVerb<T>
	{
		/// <summary>
		/// The name of the verb
		/// </summary>
		string Name { get; }
		/// <summary>
		/// A description of what this verb does on invoking <see cref="Execute(IEnumerable{string})"/>.
		/// </summary>
		string Description { get; }
		/// <summary>
		/// Accepts the arguments, not including the verb name, does something with them, and returns a value of type <typeparamref name="T"/>.
		/// </summary>
		/// <param name="args">The arguments (not including the verb name).</param>
		/// <returns></returns>
		T Execute(IEnumerable<string> args);
		/// <summary>
		/// Writes help for this verb.
		/// </summary>
		void WriteHelp();
	}
}