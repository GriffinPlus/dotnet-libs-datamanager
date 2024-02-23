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
/// Exception that is thrown when the user tries to access the value of a dummy <see cref="DataValue{T}"/>,
/// i.e. a data value that does not actually exist in the data tree.
/// </summary>
[Serializable]
public class DataValueDoesNotExistException : DataManagerException
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DataValueDoesNotExistException"/> class.
	/// </summary>
	public DataValueDoesNotExistException() { }

	/// <summary>
	/// Initializes a new instance of the <see cref="DataValueDoesNotExistException"/> class.
	/// </summary>
	/// <param name="message">Exception message text.</param>
	public DataValueDoesNotExistException(string message) : base(message) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="DataValueDoesNotExistException"/> class.
	/// </summary>
	/// <param name="message">Exception message text.</param>
	/// <param name="exception">Exception that lead to this exception (becomes the inner exception).</param>
	public DataValueDoesNotExistException(string message, Exception exception) :
		base(message, exception) { }

#if !NET8_0_OR_GREATER
	/// <summary>
	/// Serialization constructor for .NET serialization support.
	/// </summary>
	/// <param name="info">The <see cref="SerializationInfo"/> containing data to initialize the object with.</param>
	/// <param name="context">The streaming context (see MSDN).</param>
	protected DataValueDoesNotExistException(SerializationInfo info, StreamingContext context) :
		base(info, context) { }
#endif
}
