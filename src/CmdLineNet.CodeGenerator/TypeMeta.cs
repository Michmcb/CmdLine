namespace CmdLineNet.CodeGenerator;

using System;

public sealed record class TypeMeta(string? ParseMethod, Type? Type, bool ParseMethodReturnsErrorMessage, bool IntegralType);
