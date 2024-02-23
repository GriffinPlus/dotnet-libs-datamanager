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
/// Base class for exceptions that are thrown if something data manager specific goes wrongs.
/// </summary>
[Serializable]
public abstract class DataManagerException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DataManagerException"/> class.
	/// </summary>
	protected DataManagerException() { }

	/// <summary>
	/// Initializes a new instance of the <see cref="DataManagerException"/> class.
	/// </summary>
	/// <param name="message">Exception message text.</param>
	protected DataManagerException(string message) :
		base(message) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="DataManagerException"/> class.
	/// </summary>
	/// <param name="message">Exception message text.</param>
	/// <param name="ex">Exception that lead to the current exception (becomes the inner-exception of the current exception).</param>
	protected DataManagerException(string message, Exception ex) :
		base(message, ex) { }

#if !NET8_0_OR_GREATER
	/// <summary>
	/// Serialization constructor for .NET serialization support.
	/// </summary>
	/// <param name="info">The <see cref="SerializationInfo"/> containing data to initialize the object with.</param>
	/// <param name="context">The streaming context (see MSDN).</param>
	protected DataManagerException(SerializationInfo info, StreamingContext context) :
		base(info, context) { }
#endif
}
