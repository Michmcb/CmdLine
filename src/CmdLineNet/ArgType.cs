namespace CmdLineNet
{
	/// <summary>
	/// The type of an argument which is preceded by a long or short option.
	/// </summary>
	public enum ArgType
	{
		/// <summary>
		/// A switch, whole presence indicates yes or no.
		/// Their short form may be stacked together with other short-form switches, as shorthand.
		/// Switches may also be specified multiple times, for various purposes. For example, levels of verbosity.
		/// </summary>
		Switch,
		/// <summary>
		/// An option, which is followed by a text value.
		/// </summary>
		Option,
		/// <summary>
		/// A lone value, which is not preceded by an option.
		/// </summary>
		Value,
	}
}