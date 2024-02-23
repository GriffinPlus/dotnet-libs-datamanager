///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Threading;

namespace GriffinPlus.Lib.DataManager.Viewer;

/// <summary>
/// A snapshot of a <see cref="IUntypedViewerDataValue"/>.
/// </summary>
public struct UntypedViewerDataValueSnapshot
{
	#region Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="UntypedViewerDataValueSnapshot"/> class assuming nothing has changed
	/// (for internal use only, not synchronized).<br/>
	/// This constructor is used for the initial event notification informing the event recipient about the initial
	/// state of the data value.
	/// </summary>
	/// <param name="dataValue">Changed data value.</param>
	internal UntypedViewerDataValueSnapshot(IUntypedDataValueInternal dataValue)
	{
		Debug.Assert(Monitor.IsEntered(dataValue.DataTreeManager.Sync), "The tree synchronization object is not locked.");

		Value = dataValue.ValueUnsynced; // internal value instance
		Timestamp = dataValue.TimestampUnsynced;
		mProperties = dataValue.PropertiesUnsynced;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="UntypedViewerDataValueSnapshot"/> class copying another instance.
	/// </summary>
	/// <param name="other">Event arguments to copy.</param>
	internal UntypedViewerDataValueSnapshot(UntypedViewerDataValueSnapshot other)
	{
		Value = other.Value; // internal value instance or copied value instance of the other event arguments
		Timestamp = other.Timestamp;
		mProperties = other.mProperties;
	}

	#endregion

	#region Value

	/// <summary>
	/// Gets the snapshotted value of the <see cref="IUntypedDataValue.Value"/> property.
	/// </summary>
	public object Value { get; internal set; }

	#endregion

	#region Timestamp

	/// <summary>
	/// Gets the snapshotted value of the <see cref="IUntypedDataValue.Timestamp"/> property.
	/// </summary>
	public DateTime Timestamp { get; }

	#endregion

	#region Properties (incl. Boolean Accessors)

	private readonly DataValuePropertiesInternal mProperties;

	/// <summary>
	/// Gets the snapshotted value of the <see cref="IUntypedDataValue.Properties"/> property.
	/// </summary>
	public readonly DataValueProperties Properties => (DataValueProperties)(mProperties & DataValuePropertiesInternal.UserProperties);

	/// <summary>
	/// Gets the snapshotted value of the <see cref="IUntypedDataValue.IsPersistent"/> property.
	/// </summary>
	public readonly bool IsPersistent => (mProperties & DataValuePropertiesInternal.Persistent) != 0;

	/// <summary>
	/// Gets the snapshotted value of the <see cref="IUntypedViewerDataValue.IsDummy"/> property.
	/// </summary>
	public readonly bool IsDummy => (mProperties & DataValuePropertiesInternal.Dummy) != 0;

	/// <summary>
	/// Gets the snapshotted value of the <see cref="IUntypedDataValue.IsDetached"/> property.
	/// </summary>
	public readonly bool IsDetached => (mProperties & DataValuePropertiesInternal.Detached) != 0;

	#endregion
}
