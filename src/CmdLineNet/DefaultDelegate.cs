namespace CmdLineNet;

using System;

/// <summary>
/// Methods for default delegates.
/// </summary>
public static class DefaultDelegate
{
	/// <summary>
	/// Returns a default delegate that prints <see cref="IVerb{TReturn}.Name"/> concatenated with <see cref="IVerb{TReturn}.Description"/> to the console, delimited with a colon and space.
	/// </summary>
	/// <typeparam name="TReturn">The return type.</typeparam>
	/// <returns>The delegate.</returns>
	public static VerbHelp<TReturn> VerbHelp<TReturn>()
	{
		return (verb) => Console.WriteLine(string.Concat(verb.Name, ": ", verb.Description));
	}
	/// <summary>
	/// Returns a default delegate that prints "Unrecognized verb: " to the console, followed by the verb name.
	/// Returns <paramref name="returnValue"/>.
	/// </summary>
	/// <param name="returnValue">The value to return.</param>
	/// <returns>Always returns <paramref name="returnValue"/>.</returns>
	public static UnknownVerbHandler<TReturn> UnknownVerbHandler<TReturn>(TReturn returnValue) where TReturn : struct
	{
		return (verbName, _) =>
		{
			Console.WriteLine("Unrecognized verb: " + verbName);
			return returnValue;
		};
	}
	/// <summary>
	/// Returns a default delegate that prints "Unrecognized verb: " to the console, followed by the verb name.
	/// </summary>
	public static UnknownVerbHelp UnknownVerbHelp => (verbName) => Console.WriteLine("Unrecognized verb: " + verbName);
	/// <summary>
	/// Returns a default delegate that invokes <paramref name="success"/> on success.
	/// On failure, prints the error message to the console, and returns <paramref name="failReturn"/>.
	/// </summary>
	/// <typeparam name="TReturn">The return type.</typeparam>
	/// <typeparam name="TId">The type of the ID.</typeparam>
	/// <typeparam name="TSelf">The parsed object type.</typeparam>
	/// <param name="success">The delegate to invoke on success.</param>
	/// <param name="failReturn">The value to return on failure.</param>
	/// <returns>The delegate.</returns>
	public static VerbHandler<TReturn> ExecuteOrErrorMessage<TReturn, TId, TSelf>(Func<TSelf, TReturn> success, TReturn failReturn) where TSelf : ICmdParseable<TId, TSelf> where TId : struct
	{
		return (_, args) =>
		{
			if (TSelf.Parse(TSelf.GetReader().Read(args)).Ok(out TSelf parsed, out var errMsg))
			{
				return success(parsed);
			}
			else
			{
				Console.WriteLine(errMsg);
				return failReturn;
			}
		};
	}
	/// <summary>
	/// Returns a default delegate that invokes <paramref name="success"/> on success.
	/// On failure, prints the error message formatted using <paramref name="failFormat"/> to the console, and returns <paramref name="failFormat"/>.
	/// </summary>
	/// <typeparam name="TReturn">The return type.</typeparam>
	/// <typeparam name="TId">The type of the ID.</typeparam>
	/// <typeparam name="TSelf">The parsed object type.</typeparam>
	/// <param name="success">The delegate to invoke on success.</param>
	/// <param name="failFormat">The format string. Use {0} as the format parameter for the error message.</param>
	/// <param name="failReturn">The value to return on failure.</param>
	/// <returns>The delegate.</returns>
	public static VerbHandler<TReturn> ExecuteOrFormatErrorMessage<TReturn, TId, TSelf>(Func<TSelf, TReturn> success, string failFormat, TReturn failReturn) where TSelf : ICmdParseable<TId, TSelf> where TId : struct
	{
		return (_, args) =>
		{
			if (TSelf.Parse(TSelf.GetReader().Read(args)).Ok(out TSelf parsed, out var errMsg))
			{
				return success(parsed);
			}
			else
			{
				Console.WriteLine(string.Format(failFormat, errMsg));
				return failReturn;
			}
		};
	}
	/// <summary>
	/// Returns a default delegate that invokes <paramref name="success"/> on success.
	/// On failure, invokes <paramref name="failure"/>.
	/// </summary>
	/// <typeparam name="TReturn">The return type.</typeparam>
	/// <typeparam name="TId">The type of the ID.</typeparam>
	/// <typeparam name="TSelf">The parsed object type.</typeparam>
	/// <param name="success">The delegate to invoke on success.</param>
	/// <param name="failure">The delegate to invoke on failure.</param>
	/// <returns></returns>
	public static VerbHandler<TReturn> ExecuteOrError<TReturn, TId, TSelf>(Func<TSelf, TReturn> success, Func<string, TReturn> failure) where TSelf : ICmdParseable<TId, TSelf> where TId : struct
	{
		return (_, args) =>
		{
			if (TSelf.Parse(TSelf.GetReader().Read(args)).Ok(out TSelf parsed, out var errMsg))
			{
				return success(parsed);
			}
			else
			{
				return failure(errMsg);
			}
		};
	}
}
