namespace CmdLineNet
{
	/// <summary>
	/// The kind of argument that was just read.
	/// </summary>
	public enum ArgState
	{
		/// <summary>
		/// A lone value.
		/// </summary>
		Value,
		/// <summary>
		/// A long option.
		/// </summary>
		LongOption,
		/// <summary>
		/// A short option.
		/// </summary>
		ShortOption,
		/// <summary>
		/// A long switch.
		/// </summary>
		LongSwitch,
		/// <summary>
		/// A short switch.
		/// </summary>
		ShortSwitch,
		/// <summary>
		/// A single switch in a sequence of stacked switches. For example, -abc is shorthand for -a -b -c
		/// </summary>
		StackedSwitch,

		/// <summary>
		/// Some other error occurred.
		/// </summary>
		OtherError,
		/// <summary>
		/// A short option that was unrecognized.
		/// </summary>
		ShortUnrecognized,
		/// <summary>
		/// A long option that was unrecognized.
		/// </summary>
		LongUnrecognized,
		/// <summary>
		/// A short-form option was read in a collapsed switch.
		/// For example, if -o is an option, and -s is a switch, "-so" was provided.
		/// </summary>
		ShortOptionFoundInStackedSwitches,
	}
}