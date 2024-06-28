namespace CmdLineNet
{
	using System;
	using System.Buffers;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Runtime.CompilerServices;

	/// <summary>
	/// Writes help to the console.
	/// </summary>
	public sealed class HelpWriter : IDisposable
	{
		private const string TwoDashes = "--";
		private readonly HelpSettings settings;
		private readonly TextWriter output;
		private IMemoryOwner<char> memOwner;
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <param name="output">The <see cref="TextWriter"/> to write help to.</param>
		/// <param name="initialBufferSize">The buffer size to use for <see cref="char"/> pools, to avoid string allocations.</param>
		public HelpWriter(HelpSettings settings, TextWriter output, int initialBufferSize = 256)
		{
			this.settings = settings;
			this.output = output;
			memOwner = MemoryPool<char>.Shared.Rent(initialBufferSize);
		}
		/// <summary>
		/// Writes help for <paramref name="groups"/>, in the order provided.
		/// </summary>
		/// <typeparam name="TId">The type of the ID.</typeparam>
		/// <param name="groups">The arguments to write.</param>
		public void WriteHelp<TId>(IReadOnlyCollection<ArgMetasGroup<TId>> groups) where TId : struct
		{
			ReadOnlySpan<char> longShortNameSeparator = settings.LongShortNameSeparator;
			int leftMargin = settings.LeftMargin;
			int valueNum = 1;
			int maxLen;
			int minRightMargin;

			switch (settings.HelpTextAlign)
			{
				case HelpTextAlign.None:
					minRightMargin = settings.RightMargin;
					maxLen = 0;
					break;
				case HelpTextAlign.AcrossAllGroups:
					maxLen = CalculateRightMargin(groups.SelectMany(x => x.Args), settings);
					minRightMargin = 0;
					break;
				default:
					minRightMargin = 0;
					maxLen = 0;
					break;
			}
			foreach (ArgMetasGroup<TId> g in groups)
			{
				if (!g.Args.Any()) continue;
				if (settings.HelpTextAlign == HelpTextAlign.WithinGroups) maxLen = CalculateRightMargin(g.Args, settings);

				output.WriteLine(g.Name);
				foreach (ArgMeta<TId> a in g.Args)
				{
					ReadOnlySpan<char> help = a.Help.AsSpan();
					int length = 0;
					switch (a.Type)
					{
						case ArgType.Switch:
						case ArgType.Option:
							if (a.Name != null)
							{
								length = a.ShortName != default
									// 4 is 3 total dashes, plus the single char for ShortName
									? Fill(leftMargin, '-', a.ShortName, longShortNameSeparator, TwoDashes, a.Name, Math.Max(minRightMargin, maxLen - (leftMargin + 4 + longShortNameSeparator.Length + a.Name.Length)), help)
									// 2 is the dashes preceding Name
									: Fill(leftMargin, TwoDashes, a.Name, Math.Max(minRightMargin, maxLen - (leftMargin + 2 + a.Name.Length)), help);
							}
							else
							{
								if (a.ShortName != default)
								{
									// 2 is the single dash plus single char for ShortName
									length = Fill(leftMargin, '-', a.ShortName, Math.Max(minRightMargin, maxLen - (leftMargin + 2)), help);
								}
							}
							break;
						case ArgType.Value:
							length = a.Name != null
								? Fill(leftMargin, a.Name, Math.Max(minRightMargin, maxLen - (leftMargin + a.Name.Length)), help)
								: Fill(leftMargin, "Value ", valueNum++.ToString("000"), Math.Max(minRightMargin, maxLen - (leftMargin + 9)), help); // 9 is length of "Value 000"
							break;
					}
					// Re-usable buffer to avoid string allocations because I'm a lunatic
					if (length > 0)
					{
						output.WriteLine(memOwner.Memory.Span[..length]);
					}
				}
				output.WriteLine();
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EnsureSize(int reqLen)
		{
			// We don't care about the old data at all
			if (memOwner.Memory.Length >= reqLen) return;
			int newLen = Math.Max(reqLen, memOwner.Memory.Length * 2);
			memOwner.Dispose();
			memOwner = MemoryPool<char>.Shared.Rent(newLen);
		}
		private int Fill(int m0, ReadOnlySpan<char> c1, int m2, ReadOnlySpan<char> c3)
		{
			int reqLen = m0 + c1.Length + m2 + c3.Length;
			EnsureSize(reqLen);
			Span<char> buf = memOwner.Memory.Span;
			c3.CopyTo(buf[(m0 + c1.Length + m2)..]);
			buf.Slice(m0 + c1.Length, m2).Fill(' ');
			c1.CopyTo(buf[m0..]);
			buf[..m0].Fill(' ');
			return reqLen;
		}
		private int Fill(int m0, ReadOnlySpan<char> c1, ReadOnlySpan<char> c2, int m3, ReadOnlySpan<char> c4)
		{
			int reqLen = m0 + c1.Length + c2.Length + m3 + c4.Length;
			EnsureSize(reqLen);
			Span<char> buf = memOwner.Memory.Span;
			c4.CopyTo(buf[(m0 + c1.Length + c2.Length + m3)..]);
			buf.Slice(m0 + c1.Length + c2.Length, m3).Fill(' ');
			c2.CopyTo(buf[(m0 + c1.Length)..]);
			c1.CopyTo(buf[m0..]);
			buf[..m0].Fill(' ');
			return reqLen;
		}
		private int Fill(int m0, char c1, char c2, int m3, ReadOnlySpan<char> c4)
		{
			int reqLen = m0 + 2 + m3 + c4.Length;
			EnsureSize(reqLen);
			Span<char> buf = memOwner.Memory.Span;
			c4.CopyTo(buf[(m0 + 2 + m3)..]);
			buf.Slice(m0 + 2, m3).Fill(' ');
			buf[m0 + 1] = c2;
			buf[m0] = c1;
			buf[..m0].Fill(' ');
			return reqLen;
		}
		private int Fill(int m0, char c1, char c2, ReadOnlySpan<char> c3, ReadOnlySpan<char> c4, ReadOnlySpan<char> c5, int m6, ReadOnlySpan<char> c7)
		{
			int reqLen = m0 + 2 + c3.Length + c4.Length + c5.Length + m6 + c7.Length;
			EnsureSize(reqLen);
			Span<char> buf = memOwner.Memory.Span;
			c7.CopyTo(buf[(m0 + 2 + c3.Length + c4.Length + c5.Length + m6)..]);
			buf.Slice(m0 + 2 + c3.Length + c4.Length + c5.Length, m6).Fill(' ');
			c5.CopyTo(buf[(m0 + 2 + c3.Length + c4.Length)..]);
			c4.CopyTo(buf[(m0 + 2 + c3.Length)..]);
			c3.CopyTo(buf[(m0 + 2)..]);
			buf[m0 + 1] = c2;
			buf[m0] = c1;
			buf[..m0].Fill(' ');
			return reqLen;
		}
		//// <summary>
		//// Calculates the correct number of spaces to use as the right margin, given all of <paramref name="argMetas"/>, assuming
		//// </summary>
		//// <typeparam name="TId">The type of the ID.</typeparam>
		//// <param name="argMetas"></param>
		//// <param name="settings"></param>
		//// <returns></returns>
		private static int CalculateRightMargin<TId>(IEnumerable<ArgMeta<TId>> argMetas, HelpSettings settings) where TId : struct
		{
			// min length is both margins, separator's length, short name's -, and long name's --
			int leftMargin = settings.LeftMargin;
			int rightMargin = settings.RightMargin;
			int baseLenShort = leftMargin + rightMargin + 1;
			int baseLenLong = leftMargin + rightMargin + 2;
			int baseLenShortLong = settings.LongShortNameSeparator.Length + leftMargin + rightMargin + 3;
			int maxLen = 0;
			foreach (ArgMeta<TId> a in argMetas)
			{
				switch (a.Type)
				{
					case ArgType.Switch:
					case ArgType.Option:
						maxLen = a.Name != null
							? a.ShortName != default
								? Math.Max(maxLen, baseLenShortLong + a.Name.Length + 1)
								: Math.Max(maxLen, baseLenLong + a.Name.Length)
							: a.ShortName != default
								? Math.Max(maxLen, baseLenShort + 1)
								: Math.Max(maxLen, leftMargin + rightMargin);
						break;
					case ArgType.Value:
						maxLen = a.Name != null
							? Math.Max(maxLen, leftMargin + rightMargin + a.Name.Length)
							: Math.Max(maxLen, leftMargin + rightMargin + 9);// Value nnn
						break;
				}
			}
			return maxLen;
		}
		/// <summary>
		/// Writes help for <paramref name="argMetas"/>, in the order of Options, Switches, and then Values.
		/// </summary>
		/// <typeparam name="TId">The type of the ID.</typeparam>
		/// <param name="argMetas">The arguments to write.</param>
		/// <param name="settings">The settings.</param>
		public static void ConsoleWriteHelp<TId>(IEnumerable<ArgMeta<TId>> argMetas, HelpSettings settings) where TId : struct
		{
			WriteHelp(argMetas, settings, Console.Out);
		}
		/// <summary>
		/// Writes help for <paramref name="argMetas"/>, in the order of Options, Switches, and then Values.
		/// </summary>
		/// <typeparam name="TId">The type of the ID.</typeparam>
		/// <param name="argMetas">The arguments to write.</param>
		/// <param name="settings">The settings.</param>
		/// <param name="output">The <see cref="TextWriter"/> to write help to.</param>
		public static void WriteHelp<TId>(IEnumerable<ArgMeta<TId>> argMetas, HelpSettings settings, TextWriter output) where TId : struct
		{
			List<ArgMeta<TId>> options = [];
			List<ArgMeta<TId>> switches = [];
			List<ArgMeta<TId>> values = [];
			foreach (var item in argMetas)
			{
				switch (item.Type)
				{
					case ArgType.Switch:
						switches.Add(item);
						break;
					case ArgType.Option:
						options.Add(item);
						break;
					case ArgType.Value:
						values.Add(item);
						break;
				}
			}
			using HelpWriter hw = new(settings, output);
			hw.WriteHelp([new ArgMetasGroup<TId>("Options:", options), new ArgMetasGroup<TId>("Switches:", switches), new ArgMetasGroup<TId>("Values:", values)]);
		}
		/// <summary>
		/// Writes help for <paramref name="groups"/>, in the order provided.
		/// </summary>
		/// <typeparam name="TId">The type of the ID.</typeparam>
		/// <param name="groups">The arguments to write.</param>
		/// <param name="settings">The settings.</param>
		public static void ConsoleWriteHelp<TId>(IReadOnlyCollection<ArgMetasGroup<TId>> groups, HelpSettings settings) where TId : struct
		{
			using HelpWriter hw = new(settings, Console.Out);
			hw.WriteHelp(groups);
		}
		/// <summary>
		/// Disposes of the buffer held.
		/// </summary>
		public void Dispose()
		{
			memOwner.Dispose();
		}
	}
}