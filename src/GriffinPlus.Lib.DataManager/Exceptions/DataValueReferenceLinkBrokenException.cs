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
/// Exception that is thrown when the user tries to access a data value reference (see <see cref="Data{T}"/>
/// and <see cref="IUntypedData"/>) that has lost the link to its <see cref="DataValue{T}"/> instance.
/// </summary>
[Serializable]
public class DataValueReferenceBrokenException : DataManagerException
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DataValueReferenceBrokenException"/> class.<br/>
	/// The <see cref="Exception.Message"/> is set to:
	/// 'The link between the data value reference and its data tree is broken.'
	/// </summary>
	public DataValueReferenceBrokenException() :
		base("The link between the data value reference and its data tree is broken.") { }

	/// <summary>
	/// Initializes a new instance of the <see cref="DataValueReferenceBrokenException"/> class.
	/// </summary>
	/// <param name="message">Exception message text.</param>
	public DataValueReferenceBrokenException(string message) : base(message) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="DataValueReferenceBrokenException"/> class.
	/// </summary>
	/// <param name="message">Exception message text.</param>
	/// <param name="exception">Exception that lead to this exception (becomes the inner exception).</param>
	public DataValueReferenceBrokenException(string message, Exception exception) :
		base(message, exception) { }

#if !NET8_0_OR_GREATER
	/// <summary>
	/// Serialization constructor for .NET serialization support.
	/// </summary>
	/// <param name="info">The <see cref="SerializationInfo"/> containing data to initialize the object with.</param>
	/// <param name="context">The streaming context (see MSDN).</param>
	protected DataValueReferenceBrokenException(SerializationInfo info, StreamingContext context) :
		base(info, context) { }
#endif
}
