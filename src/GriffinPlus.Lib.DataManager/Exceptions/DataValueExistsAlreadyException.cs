///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
#if !NET8_0_OR_GREATER
using System.Runtime.Serialization;
#endif

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Exception that is thrown when the user tries to add or rename a data value and the specified
/// value name does already exist at the same level.
/// </summary>
[Serializable]
public class DataValueExistsAlreadyException : DataManagerException
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DataValueExistsAlreadyException"/> class.
	/// </summary>
	public DataValueExistsAlreadyException() { }

	/// <summary>
	/// Initializes a new instance of the <see cref="DataValueExistsAlreadyException"/> class.
	/// </summary>
	/// <param name="message">Exception message text.</param>
	public DataValueExistsAlreadyException(string message) : base(message) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="DataValueExistsAlreadyException"/> class.
	/// </summary>
	/// <param name="message">Exception message text.</param>
	/// <param name="exception">Exception that lead to this exception (becomes the inner exception).</param>
	public DataValueExistsAlreadyException(string message, Exception exception) :
		base(message, exception) { }

#if !NET8_0_OR_GREATER
	/// <summary>
	/// Serialization constructor for .NET serialization support.
	/// </summary>
	/// <param name="info">The <see cref="SerializationInfo"/> containing data to initialize the object with.</param>
	/// <param name="context">The streaming context (see MSDN).</param>
	protected DataValueExistsAlreadyException(SerializationInfo info, StreamingContext context) :
		base(info, context) { }
#endif
}
