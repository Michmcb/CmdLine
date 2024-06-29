namespace CmdLineNet
{
	/// <summary>
	/// Settings for writing help.
	/// </summary>
	public sealed class HelpWriterSettings
	{
		/// <summary>
		/// The default settings. They are: <see cref="LongShortNameSeparator"/> is a space, <see cref="LeftMargin"/> is 1, <see cref="RightMargin"/> is 3, and <see cref="HelpTextAlign"/> is <see cref="HelpTextAlign.AcrossAllGroups"/>.
		/// </summary>
		public static HelpWriterSettings Default { get; } = new(" ", 1, 3, HelpTextAlign.AcrossAllGroups);
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public HelpWriterSettings(string longShortNameSeparator, int leftMargin, int rightMargin, HelpTextAlign helpTextAlign)
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