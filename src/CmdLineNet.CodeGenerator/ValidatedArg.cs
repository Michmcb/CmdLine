namespace CmdLineNet.CodeGenerator;

public sealed record class ValidatedArg
(
	string Name,
	ArgType ArgType,
	string TypeName,
	TypeMeta? TypeMeta,
	bool Required,
	bool CtorTypeNullable,
	bool LocalTypeNullable,
	string? LongName,
	char? ShortName,
	string? FriendlyName,
	int Min,
	int Max,
	string InitialValue,
	string? Help
);