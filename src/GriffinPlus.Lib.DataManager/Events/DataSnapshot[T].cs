///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Threading;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// A snapshot of a <see cref="Data{T}"/>.
/// </summary>
public struct DataSnapshot<T>
{
	private readonly Data<T> mData;

	#region Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="DataSnapshot{T}"/> class assuming nothing has changed
	/// (for internal use only, not synchronized).<br/>
	/// This constructor is used for the initial event notification informing the event recipient about the initial
	/// state of the data value reference.
	/// </summary>
	/// <param name="data">Changed data value.</param>
	internal DataSnapshot(Data<T> data)
	{
		Debug.Assert(Monitor.IsEntered(data.RootNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");

		mData = data;

		Value = data.ValueUnsynced; // internal value instance
		Timestamp = data.TimestampUnsynced;
		IsHealthy = data.DataValueUnsynced != null;
		mProperties = data.PropertiesUnsynced;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DataSnapshot{T}"/> class copying another instance.
	/// </summary>
	/// <param name="other">Event arguments to copy.</param>
	internal DataSnapshot(DataSnapshot<T> other)
	{
		mData = other.mData;
		Value = other.Value; // internal value instance or copied value instance of the other event arguments
		Timestamp = other.Timestamp;
		IsHealthy = other.IsHealthy;
		mProperties = other.mProperties;
	}

	#endregion

	#region Name

	/// <summary>
	/// Gets the snapshotted value of the <see cref="Data{T}.Name"/> property.
	/// </summary>
	public readonly string Name => mData.Name; // the name is immutable

	#endregion

	#region Path

	/// <summary>
	/// Gets the snapshotted value of the <see cref="Data{T}.Path"/> property.
	/// </summary>
	public readonly string Path => mData.Path; // the path is immutable

	#endregion

	#region Value

	/// <summary>
	/// Gets the snapshotted value of the <see cref="Data{T}.Value"/> property.
	/// </summary>
	public T Value { get; internal set; }

	#endregion

	#region IsHealthy

	/// <summary>
	/// Gets the snapshotted value of the <see cref="Data{T}.IsHealthy"/> property.
	/// </summary>
	public bool IsHealthy { get; internal set; }

	#endregion

	#region Timestamp

	/// <summary>
	/// Gets the snapshotted value of the <see cref="Data{T}.Timestamp"/> property.
	/// </summary>
	public DateTime Timestamp { get; }

	#endregion

	#region Properties (incl. Boolean Accessors)

	private readonly DataValuePropertiesInternal mProperties;

	/// <summary>
	/// Gets the snapshotted value of the <see cref="Data{T}.Properties"/> property.
	/// </summary>
	public readonly DataValueProperties Properties => (DataValueProperties)(mProperties & DataValuePropertiesInternal.UserProperties);

	/// <summary>
	/// Gets the snapshotted value of the <see cref="Data{T}.IsPersistent"/> property.
	/// </summary>
	public readonly bool IsPersistent => (mProperties & DataValuePropertiesInternal.Persistent) != 0;

	#endregion
}
