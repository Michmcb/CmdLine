namespace CmdLineNet
{
	/// <summary>
	/// Settings for writing help.
	/// </summary>
	public sealed class HelpSettings
	{
		/// <summary>
		/// The default settings.
		/// </summary>
		public static readonly HelpSettings Default = new(" ", 1, 3, true);
		public HelpSettings(string longShortNameSeparator, int leftMargin, int rightMargin, bool alignHelpTextAcrossAllGroups)
		{
			LongShortNameSeparator = longShortNameSeparator;
			LeftMargin = leftMargin;
			RightMargin = rightMargin;
			AlignHelpTextAcrossAllGroups = alignHelpTextAcrossAllGroups;
		}
		/// <summary>
		/// The separator to use between long and short names.
		/// </summary>
		public string LongShortNameSeparator { get; }
		/// <summary>
		/// The number of spaces on the left.
		/// </summary>
		public int LeftMargin { get; }
		/// <summary>
		/// The number of spaces on the right.
		/// </summary>
		public int RightMargin { get; }
		/// <summary>
		/// If true, help text for different groups will have the same alignment as eachother.
		/// If false, help text for different groups will only be aligned with help text in the same group
		/// </summary>
		public bool AlignHelpTextAcrossAllGroups { get; }
	}
}