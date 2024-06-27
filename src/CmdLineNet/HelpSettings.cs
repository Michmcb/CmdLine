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
		public static readonly HelpSettings Default = new(" ", 1, 3, HelpTextAlign.AcrossAllGroups);
		public HelpSettings(string longShortNameSeparator, int leftMargin, int rightMargin, HelpTextAlign helpTextAlign)
		{
			LongShortNameSeparator = longShortNameSeparator;
			LeftMargin = leftMargin;
			RightMargin = rightMargin;
			HelpTextAlign = helpTextAlign;
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
		/// How to align help text.
		/// </summary>
		public HelpTextAlign HelpTextAlign { get; }
	}
}