///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Diagnostics;
using System.Threading;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Event arguments for events concerning a single <see cref="DataValue{T}"/> in the data manager.
/// </summary>
public sealed class DataValueChangedEventArgs<T> : DataManagerEventArgs
{
	#region Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="DataValueChangedEventArgs{T}"/> class
	/// (for internal use only, not synchronized).<br/>
	/// This constructor is used when notifying event recipients about subsequent changes to the data value.
	/// </summary>
	/// <param name="dataValue">Data value that has changed.</param>
	/// <param name="changedFlags">Changed flags indicating what properties have changed.</param>
	internal DataValueChangedEventArgs(DataValue<T> dataValue, DataValueChangedFlags changedFlags)
	{
		Debug.Assert(Monitor.IsEntered(dataValue.DataTreeManager.Sync), "The tree synchronization object is not locked.");

		mSnapshot = new DataValueSnapshot<T>(dataValue);
		DataValue = dataValue;
		ChangedFlags = changedFlags;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DataValueChangedEventArgs{T}"/> class copying another instance.
	/// </summary>
	/// <param name="other">Event arguments to copy.</param>
	private DataValueChangedEventArgs(DataValueChangedEventArgs<T> other)
	{
		mSnapshot = new DataValueSnapshot<T>(other.mSnapshot);
		DataValue = other.DataValue;
		ChangedFlags = other.ChangedFlags;
	}

	#endregion

	#region Properties

	#region DataValue

	/// <summary>
	/// Gets the data value that has changed.<br/>
	/// The data value will always return the latest value and properties, while the <see cref="Snapshot"/>
	/// property provides a snapshot of the data value just after the change the event notifies about.
	/// </summary>
	public DataValue<T> DataValue { get; }

	#endregion

	#region Snapshot

	private bool                 mIsSnapshotValid;
	private DataValueSnapshot<T> mSnapshot;

	/// <summary>
	/// Gets the snapshot of <see cref="DataValue"/> just after the data value has changed.
	/// </summary>
	public DataValueSnapshot<T> Snapshot
	{
		get
		{
			// abort if a copy of the internal value is already available via the snapshot...
			if (mIsSnapshotValid)
				return mSnapshot;

			// the snapshot still contains the original value instance that must not leave the data tree
			// => copy the internal value outside the lock to reduce lock contention
			T copy = DataValue.DataTreeManager.Serializer.CopySerializableValue(mSnapshot.Value);

			lock (Sync)
			{
				if (mIsSnapshotValid) return mSnapshot;
				mSnapshot.Value = copy;
				Thread.MemoryBarrier(); // ensures that mIsSnapshotValid is guaranteed to be set after mSnapshot
				mIsSnapshotValid = true;
				return mSnapshot;
			}
		}
	}

	#endregion

	#region ChangedFlags

	/// <summary>
	/// Gets the flags indicating what properties have changed.
	/// </summary>
	public DataValueChangedFlags ChangedFlags { get; }

	#endregion

	#endregion

	#region Copying Event Arguments

	/// <inheritdoc/>
	public override DataManagerEventArgs[] Dupe(int count)
	{
		var copies = new DataManagerEventArgs[count];
		for (int i = 0; i < count; i++)
		{
			copies[i] = new DataValueChangedEventArgs<T>(this);
		}

		return copies;
	}

	#endregion
}
