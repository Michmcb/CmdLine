namespace CmdLineNet
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// A verb.
	/// </summary>
	/// <typeparam name="TReturn">The return type.</typeparam>
	public sealed class Verb<TReturn> : IVerb<TReturn>
	{
		private readonly VerbHandler<TReturn> execute;
		private readonly Action writeDetailedHelp;
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public Verb(string name, string description, VerbHandler<TReturn> execute, Action writeDetailedHelp)
		{
			Name = name;
			Description = description;
			this.execute = execute;
			this.writeDetailedHelp = writeDetailedHelp;
		}
		/// <inheritdoc/>
		public string Name { get; }
		/// <inheritdoc/>
		public string Description { get; }
		/// <inheritdoc/>
		public TReturn Execute(IEnumerable<string> args)
		{
			return execute(Name, args);
		}
		/// <inheritdoc/>
		public void WriteHelp()
		{
			 writeDetailedHelp();
		}
	}
}