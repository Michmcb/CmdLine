namespace CmdLineNet
{
	using System;
	using System.Runtime.CompilerServices;

	/// <summary>
	/// Extensions to add verbs to a <see cref="DictionaryVerbHandler{TReturn}"/>.
	/// </summary>
	public static class DictionaryVerbHandlerExtensions
	{
		/// <summary>
		/// Adds a <see cref="Verb{TReturn}"/>, keyed by name.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddVerb<TReturn>(this DictionaryVerbHandler<TReturn> d, string name, string description, VerbHandler<TReturn> execute, Action writeDetailedHelp)
		{
			d.AllVerbs.Add(name, new Verb<TReturn>(name, description, execute, writeDetailedHelp));
		}
		/// <summary>
		/// Adds a <see cref="HelpVerb{T}"/>, keyed by name.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddHelpVerb<TReturn>(this DictionaryVerbHandler<TReturn> d, string name, string description, string helpText, TReturn returnValue, VerbHelp<TReturn> writeGeneralHelp, UnknownVerbHelp unknownVerbHelp)
		{
			d.AllVerbs.Add(name, new HelpVerb<TReturn>(name, description, helpText, d.AllVerbs, returnValue, writeGeneralHelp, unknownVerbHelp));
		}
	}
}