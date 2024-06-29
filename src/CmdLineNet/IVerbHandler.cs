namespace CmdLineNet
{
	using System.Collections.Generic;

	/// <summary>
	/// Handles incoming verbs and their arguments.
	/// </summary>
	/// <typeparam name="T">The return type, typically an exit or error code.</typeparam>
	public interface IVerbHandler<T>
	{
		/// <summary>
		/// Handles a verb and its arguments.
		/// </summary>
		/// <param name="verbName">The verb name.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>The value returned by handling the verb.</returns>
		T HandleVerb(string verbName, IEnumerable<string> args);
	}
}