///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Diagnostics;
using System.Threading;

namespace GriffinPlus.Lib.DataManager.Viewer;

/// <summary>
/// Event arguments for events concerning a single <see cref="IUntypedViewerDataValue"/> in the data manager.
/// </summary>
public sealed class UntypedViewerDataValueChangedEventArgs : DataManagerEventArgs
{
	#region Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="UntypedViewerDataValueChangedEventArgs"/> class
	/// (for internal use only, not synchronized).<br/>
	/// This constructor is used when notifying event recipients about subsequent changes to the data value.
	/// </summary>
	/// <param name="dataValue">Data value that has changed.</param>
	/// <param name="changedFlags">Changed flags indicating what properties have changed.</param>
	internal UntypedViewerDataValueChangedEventArgs(IUntypedDataValueInternal dataValue, ViewerDataValueChangedFlags changedFlags)
	{
		Debug.Assert(Monitor.IsEntered(dataValue.DataTreeManager.Sync), "The tree synchronization object is not locked.");

		mSnapshot = new UntypedViewerDataValueSnapshot(dataValue);
		mDataValue = dataValue;
		ChangedFlags = changedFlags;
	}

	#endregion

	#region DataValue

	private readonly IUntypedDataValueInternal mDataValue;

	/// <summary>
	/// Gets the data value that has changed.<br/>
	/// The data value will always return the latest value and properties, while the <see cref="Snapshot"/>
	/// property provides a snapshot of the data value just after the change the event notifies about.
	/// </summary>
	public IUntypedViewerDataValue DataValue => mDataValue.ViewerWrapper;

	#endregion

	#region Snapshot

	private bool                           mIsSnapshotValid;
	private UntypedViewerDataValueSnapshot mSnapshot;

	/// <summary>
	/// Gets the snapshot of <see cref="DataValue"/> just after the data value has changed.
	/// </summary>
	public UntypedViewerDataValueSnapshot Snapshot
	{
		get
		{
			// abort if a copy of the internal value is already available via the snapshot...
			if (mIsSnapshotValid)
				return mSnapshot;

			// the snapshot still contains the original value instance that must not leave the data tree
			// => copy the internal value outside the lock to reduce lock contention
			object copy = mDataValue.DataTreeManager.Serializer.CopySerializableValue(mSnapshot.Value);

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
	public ViewerDataValueChangedFlags ChangedFlags { get; }

	#endregion
}
