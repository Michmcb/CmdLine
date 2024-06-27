namespace CmdLineNet
{
	using System.Collections.Generic;

	/// <summary>
	/// A delegate that handles a known verb.
	/// </summary>
	/// <typeparam name="TReturn">The return type.</typeparam>
	/// <param name="verbName">The name of the verb that is being invoked.</param>
	/// <param name="args">The remaining arguments.</param>
	/// <returns>The return value.</returns>
	public delegate TReturn VerbHandler<out TReturn>(string verbName, IEnumerable<string> args);
	/// <summary>
	/// A delegate that handles an unknown verb.
	/// </summary>
	/// <typeparam name="TReturn">The return type.</typeparam>
	/// <param name="verbName">The name of the unknown verb that was provided.</param>
	/// <param name="args">The remaining arguments.</param>
	/// <returns>The return value.</returns>
	public delegate TReturn UnknownVerbHandler<out TReturn>(string verbName, IEnumerable<string> args);
	/// <summary>
	/// A delegate that provides help when help is requested for an unknown verb.
	/// </summary>
	/// <param name="verbName">The name of the unknown verb for which help was requested.</param>
	public delegate void UnknownVerbHelp(string verbName);
	/// <summary>
	/// A delegate that provides help on a specific verb.
	/// </summary>
	/// <typeparam name="TReturn">The return type.</typeparam>
	/// <param name="verb">The verb on which to provide help.</param>
	public delegate void VerbHelp<TReturn>(IVerb<TReturn> verb);
	/// <summary>
	/// A delegate executed when a verb encounters an error.
	/// </summary>
	/// <typeparam name="TReturn">The return type.</typeparam>
	/// <param name="verbName">The name of the verb that was invoked.</param>
	/// <param name="errMsg">The error message</param>
	/// <returns>The return value.</returns>
	public delegate TReturn VerbError<out TReturn>(string verbName, string errMsg);
}
