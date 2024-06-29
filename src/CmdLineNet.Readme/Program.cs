namespace CmdLineNet.Readme;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public sealed record class MoveArgs(string Source, string Dest, bool Overwrite) : ICmdParseable<MoveArgs.Id, MoveArgs>
{
	public enum Id { Source, Dest, Overwrite }
	private static readonly ArgsReader<Id> reader = new ArgsReaderBuilder<Id>()
		.Option(Id.Source, 's', "source", 1, 1, "The source file to move")
		.Option(Id.Dest, 'd', "dest", 1, 1, "The destination to move the file to")
		.Switch(Id.Overwrite, 'o', "overwrite", 1, 1, "Whether or not to allow overwriting the destination file")
		.Build();
	public static ArgsReader<Id> GetReader() { return reader; }
	public static ParseResult<MoveArgs> Parse(IEnumerable<RawArg<Id>> args)
	{
		string? source = null, dest = null;
		bool overwrite = false;
		foreach (RawArg<Id> a in args)
		{
			if (!a.Ok) return a.Content;
			switch (a.Id)
			{
				case Id.Source:
					source = a.Content;
					break;
				case Id.Dest:
					dest = a.Content;
					break;
				case Id.Overwrite:
					overwrite = true;
					break;
			}
		}
		if (source == null) return "Missing required option: -s|--source";
		if (dest == null) return "Missing required option: -d|--dest";
		return new MoveArgs(source, dest, overwrite);
	}
}
public sealed record class NewArgs(string Name) : ICmdParseable<NewArgs.Id, NewArgs>
{
	public enum Id { Name }
	private static readonly ArgsReader<Id> reader = new ArgsReaderBuilder<Id>()
		.Value(Id.Name, "Name", 1, 1, "The name of the new file")
		.Build();
	public static ArgsReader<Id> GetReader() { return reader; }
	public static ParseResult<NewArgs> Parse(IEnumerable<RawArg<Id>> args)
	{
		string? name = null;
		foreach (RawArg<Id> a in args)
		{
			if (!a.Ok) return a.Content;
			switch (a.Id)
			{
				case Id.Name:
					name = a.Content;
					break;
			}
		}
		if (name == null) return "Missing required value: Name";
		return new NewArgs(name);
	}
}
public static class Program
{
	const int errorBadVerb = 1;
	const int errorBadArgs = 2;
	const int errorOnNew = 3;
	const int errorOnMove = 4;
	public static int Main(string[] args)
	{
		DictionaryVerbHandler<int> verbHandler = new
		(
			allVerbs: [],
			unknownVerbHandler: DefaultDelegate.UnknownVerbHandler(returnValue: errorBadVerb)
		);

		verbHandler.AddVerb
		(
			name: "new",
			description: "Creates a new file",
			execute: DefaultDelegate.ExecuteOrErrorMessage<int, NewArgs.Id, NewArgs>(New, errorBadArgs),
			writeDetailedHelp: DefaultDelegate.WriteVerbDetailedHelp<NewArgs.Id, NewArgs>()
		);
		verbHandler.AddVerb
		(
			name: "move",
			description: "Moves a file",
			execute: DefaultDelegate.ExecuteOrErrorMessage<int, MoveArgs.Id, MoveArgs>(Move, errorBadArgs),
			writeDetailedHelp: DefaultDelegate.WriteVerbDetailedHelp<MoveArgs.Id, MoveArgs>()
		);
		verbHandler.AddHelpVerb
		(
			name: "help",
			description: "Displays general help or specific help for other verbs. Use \"help help\" for help on this verb",
			helpText: "To show general help, just type help. To show detailed help on a verb, type help <verb>.",
			returnValue: 0,
			writeGeneralHelp: DefaultDelegate.WriteVerbGeneralHelp<int>(),
			unknownVerbHelp: DefaultDelegate.UnknownVerbHelp
		);

		string? verb = args.FirstOrDefault();
		if (verb != null)
		{
			return verbHandler.HandleVerb(verb, args.Skip(1));
		}
		else
		{
			Console.WriteLine("Use \"help\" to get some help");
			return 0;
		}
	}
	private static int New(NewArgs args)
	{
		try
		{
			File.Create(args.Name);
			return 0;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
			return errorOnNew;
		}
	}
	private static int Move(MoveArgs args)
	{
		try
		{
			File.Move(args.Source, args.Dest, args.Overwrite);
			return 0;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
			return errorOnMove;
		}
	}
}
