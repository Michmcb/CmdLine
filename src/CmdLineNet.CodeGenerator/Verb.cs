namespace CmdLineNet.CodeGenerator;

using System.Collections.Generic;

public sealed class Verb
{
	public Verb(string className, List<ValidatedArg> args)
	{
		ClassName = className;
		Args = args;
	}
	public string ClassName { get; set; }
	public List<ValidatedArg> Args { get; }
}
