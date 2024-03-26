﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Diagnostics;
using System.Threading;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Event arguments for events concerning a single <see cref="IUntypedData"/> in the data manager.
/// </summary>
public sealed class UntypedDataChangedEventArgs : DataManagerEventArgs
{
	#region Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="UntypedDataChangedEventArgs"/> class
	/// (for internal use only, not synchronized).<br/>
	/// This constructor is used when notifying event recipients about subsequent changes to the data value reference.
	/// </summary>
	/// <param name="data">Data value reference that has changed.</param>
	/// <param name="changedFlags">Changed flags indicating what properties have changed.</param>
	internal UntypedDataChangedEventArgs(IUntypedDataInternal data, DataChangedFlags changedFlags)
	{
		Debug.Assert(Monitor.IsEntered(data.RootNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");

		mSnapshot = new UntypedDataSnapshot(data);
		mData = data;
		ChangedFlags = changedFlags;
	}

	#endregion

	#region DataValue

	private readonly IUntypedDataInternal mData;

	/// <summary>
	/// Gets the data value reference that has changed.<br/>
	/// The data value reference will always return the latest value and properties, while the <see cref="Snapshot"/>
	/// property provides a snapshot of the data value reference just after the change the event notifies about.
	/// </summary>
	public IUntypedData Data => mData;

	#endregion

	#region Snapshot

	private bool                mIsSnapshotValid;
	private UntypedDataSnapshot mSnapshot;

	/// <summary>
	/// Gets the snapshot of <see cref="Data"/> just after the data value has changed.
	/// </summary>
	public UntypedDataSnapshot Snapshot
	{
		get
		{
			// abort if a copy of the internal value is already available via the snapshot...
			if (mIsSnapshotValid)
				return mSnapshot;

			// the snapshot still contains the original value instance that must not leave the data tree
			// => copy the internal value outside the lock to reduce lock contention
			object copy = mData.RootNode.DataTreeManager.Serializer.CopySerializableValue(mSnapshot.Value);

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
	public DataChangedFlags ChangedFlags { get; }

	#endregion
}
