///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
#if !NET8_0_OR_GREATER
using System.Runtime.Serialization;
#endif

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Exception that is thrown when the user tries to assign an object of an incompatible type to a data value.
/// </summary>
[Serializable]
public class DataTypeMismatchException : DataManagerException
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DataTypeMismatchException"/> class.
	/// </summary>
	public DataTypeMismatchException() { }

	/// <summary>
	/// Initializes a new instance of the <see cref="DataTypeMismatchException"/> class.
	/// </summary>
	/// <param name="message">Exception message text.</param>
	public DataTypeMismatchException(string message) : base(message) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="DataTypeMismatchException"/> class.
	/// </summary>
	/// <param name="message">Exception message text.</param>
	/// <param name="exception">Exception that lead to this exception (becomes the inner exception).</param>
	public DataTypeMismatchException(string message, Exception exception) :
		base(message, exception) { }

#if !NET8_0_OR_GREATER
	/// <summary>
	/// Serialization constructor for .NET serialization support.
	/// </summary>
	/// <param name="info">The <see cref="SerializationInfo"/> containing data to initialize the object with.</param>
	/// <param name="context">The streaming context (see MSDN).</param>
	protected DataTypeMismatchException(SerializationInfo info, StreamingContext context) :
		base(info, context) { }
#endif
}
