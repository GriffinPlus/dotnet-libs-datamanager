///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Manages <see cref="Data{T}"/> objects that reference data values in the data tree.
/// </summary>
public class DataTreeManager
{
	private readonly Dictionary<string, List<WeakReference<IUntypedDataInternal>>> mDataValueReferenceMap;
	private readonly WeakReference<DataNode>                                       mRootNode;

	/// <summary>
	/// Initializes a new instance of the <see cref="DataTreeManager"/> class
	/// (reuses the synchronization object of the data tree and the data tree serializer of the specified tree).
	/// </summary>
	/// <param name="manager">Data tree manager to base upon.</param>
	/// <param name="root">Root node of the data tree to manage.</param>
	internal DataTreeManager(DataTreeManager manager, DataNode root)
	{
		Host = manager.Host;
		Serializer = manager.Serializer;
		Sync = manager.Sync;

		// store a weak reference to the root node to check whether it is alive
		// in order to shut the thread down when the node is garbage collected
		mRootNode = new WeakReference<DataNode>(root);

		// init the table mapping data value paths to Data<T> objects referencing addressed DataValue<T> objects
		mDataValueReferenceMap = [];
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DataTreeManager"/> class.
	/// </summary>
	/// <param name="host">Data tree manager host the data tree manager will belong to.</param>
	/// <param name="root">Root node of the data tree to manage.</param>
	/// <param name="serializer">Serializer to use for serializing, deserializing and copying data in the data tree.</param>
	internal DataTreeManager(DataTreeManagerHost host, DataNode root, IDataManagerSerializer serializer)
	{
		Host = host;
		Serializer = serializer;
		Sync = new object();

		// store a weak reference to the root node to check whether it is alive
		// in order to shut the thread down when the node is garbage collected
		mRootNode = new WeakReference<DataNode>(root);

		// init the table mapping data value paths to Data<T> objects referencing addressed DataValue<T> objects
		mDataValueReferenceMap = [];
	}

	/// <summary>
	/// Gets the synchronization object that is used for synchronizing access to the data tree
	/// (used in conjunction with the <see cref="Monitor"/> class or lock statements).
	/// </summary>
	internal object Sync { get; }

	/// <summary>
	/// Gets the serializer the associated data tree uses to persist the data tree or copy objects.
	/// </summary>
	public IDataManagerSerializer Serializer { get; }

	/// <summary>
	/// Gets the root node of the managed data tree.
	/// </summary>
	/// <value>
	/// The root node of the managed data tree;<br/>
	/// <c>null</c> if the root node has been garbage collected.
	/// </value>
	public DataNode RootNode => mRootNode.TryGetTarget(out DataNode node) ? node : null;

	/// <summary>
	/// Gets the <see cref="Host"/> that hosts the current instance.
	/// </summary>
	public DataTreeManagerHost Host { get; }

	#region Managing Data<T> Object Monitoring

	/// <summary>
	/// Registers a data value reference for monitoring (for internal use only, not synchronized).<br/>
	/// This method registers the data value reference only, it does _not_ update the specified data value reference.
	/// </summary>
	/// <param name="data">Data value reference to register for monitoring.</param>
	internal void RegisterDataValueReferenceUnsynced(IUntypedDataInternal data)
	{
		Debug.Assert(Monitor.IsEntered(Sync), "The tree synchronization object is not locked.");

		string path = data.Path;
		if (!mDataValueReferenceMap.TryGetValue(path, out List<WeakReference<IUntypedDataInternal>> weakReferences))
		{
			weakReferences = [];
			mDataValueReferenceMap.Add(path, weakReferences);
		}

		weakReferences.Add(new WeakReference<IUntypedDataInternal>(data));
	}

	/// <summary>
	/// Registers data value references (see <see cref="Data{T}"/>) for monitoring (for internal use only, not synchronized).<br/>
	/// This method registers the specified data value references and binds the references to the corresponding data value.
	/// </summary>
	/// <param name="references">Data objects to monitor.</param>
	internal void RegisterDataValueReferencesUnsynced(IUntypedDataInternal[] references)
	{
		Debug.Assert(Monitor.IsEntered(Sync), "The tree synchronization object is not locked.");

		foreach (IUntypedDataInternal reference in references)
		{
			RegisterDataValueReferenceUnsynced(reference);
			reference.UpdateDataValueReference(); // creates the path, if necessary
		}
	}

	/// <summary>
	/// Unregisters a data value reference from monitoring (for internal use only, not synchronized).
	/// </summary>
	/// <param name="reference">Data value reference to unregister from monitoring.</param>
	internal void UnregisterDataValueReferenceUnsynced(IUntypedDataInternal reference)
	{
		Debug.Assert(Monitor.IsEntered(Sync), "The tree synchronization object is not locked.");

		// abort if the path of the data value reference is not registered
		string path = reference.Path;
		if (!mDataValueReferenceMap.TryGetValue(path, out List<WeakReference<IUntypedDataInternal>> weakReferences))
			return;

		// remove the data value reference from the collection of registered data value references
		int index = 0;
		foreach (WeakReference<IUntypedDataInternal> weakReference in weakReferences)
		{
			if (weakReference.TryGetTarget(out IUntypedDataInternal otherReference) && ReferenceEquals(reference, otherReference))
			{
				weakReferences.RemoveAt(index);
				RemoveReferencedDataValueIfPossibleUnsynced(reference, weakReferences);
				break;
			}
			index++;
		}
	}

	/// <summary>
	/// Unregisters data value references from monitoring the specified regular data value (for internal use only, not synchronized).
	/// </summary>
	/// <param name="value">Data value to unregister references from.</param>
	/// <param name="suppressChangedEvent">
	/// <c>true</c> to suppress raising the <see cref="Data{T}.Changed"/> event and the <see cref="Data{T}.ChangedAsync"/> event;<br/>
	/// otherwise <c>false</c>.
	/// </param>
	/// <returns>Unregistered data value references.</returns>
	internal IUntypedDataInternal[] UnregisterDataValueReferencesUnsynced(IUntypedDataValueInternal value, bool suppressChangedEvent)
	{
		Debug.Assert(Monitor.IsEntered(Sync), "The tree synchronization object is not locked.");

		// abort, if the specified data value is not monitored
		if (!mDataValueReferenceMap.TryGetValue(value.PathUnsynced, out List<WeakReference<IUntypedDataInternal>> dataValueReferences) || dataValueReferences.Count == 0)
			return [];

		// remove references attached to the specified data value
		var result = new List<IUntypedDataInternal>();
		for (int i = dataValueReferences.Count - 1; i >= 0; i--)
		{
			WeakReference<IUntypedDataInternal> weakReference = dataValueReferences[i];
			if (!weakReference.TryGetTarget(out IUntypedDataInternal data)) continue;
			if (!ReferenceEquals(data.DataValueUnsynced, value)) continue;
			result.Add(data);
			dataValueReferences.RemoveAt(i);
			RemoveReferencedDataValueIfPossibleUnsynced(data, dataValueReferences);
			data.InvalidateDataValueReference(suppressChangedEvent);
		}

		return [.. result];
	}

	/// <summary>
	/// Unregisters data value references below a certain node from monitoring
	/// (for internal use only, not synchronized).
	/// </summary>
	/// <param name="node">Node below which data value references should be unregistered.</param>
	/// <param name="suppressChangedEvent">
	/// <c>true</c> to suppress raising the <see cref="Data{T}.Changed"/> event and the <see cref="Data{T}.ChangedAsync"/> event;<br/>
	/// otherwise <c>false</c>.
	/// </param>
	/// <returns>Unregistered data value references.</returns>
	internal IUntypedDataInternal[] UnregisterDataValueReferencesBelowNodeUnsynced(DataNode node, bool suppressChangedEvent)
	{
		Debug.Assert(Monitor.IsEntered(Sync), "The tree synchronization object is not locked.");

		// append a path separator to the specified node path
		ReadOnlySpan<char> nodePath = node.PathUnsynced.AsSpan();
		Span<char> searchPath = stackalloc char[nodePath.Length + 1];
		nodePath.CopyTo(searchPath);
		searchPath[nodePath.Length] = PathHelpers.SeparatorChar;

		// unregister all data value references starting with the constructed path
		var result = new List<IUntypedDataInternal>();
		foreach (KeyValuePair<string, List<WeakReference<IUntypedDataInternal>>> kvp in mDataValueReferenceMap)
		{
			(string path, List<WeakReference<IUntypedDataInternal>> dataValueReferences) = (kvp.Key, kvp.Value);
			if (!path.AsSpan().StartsWith(searchPath)) continue;
			for (int i = dataValueReferences.Count - 1; i >= 0; i--)
			{
				WeakReference<IUntypedDataInternal> weakReference = dataValueReferences[i];
				if (!weakReference.TryGetTarget(out IUntypedDataInternal data)) continue;
				result.Add(data);
				dataValueReferences.RemoveAt(i);
				RemoveReferencedDataValueIfPossibleUnsynced(data, dataValueReferences);
				data.InvalidateDataValueReference(suppressChangedEvent);
			}
		}

		return [.. result];
	}

	/// <summary>
	/// Invalidates all data value references addressing the data value at the specified path
	/// (for internal use only, not synchronized).
	/// </summary>
	/// <param name="path">Path of the data value to invalidate references to.</param>
	/// <param name="suppressChangedEvent">
	/// <c>true</c> to suppress raising the <see cref="Data{T}.Changed"/> event and the <see cref="Data{T}.ChangedAsync"/> event;<br/>
	/// otherwise <c>false</c>.
	/// </param>
	internal void InvalidateReferencesForDataValueUnsynced(string path, bool suppressChangedEvent)
	{
		Debug.Assert(Monitor.IsEntered(Sync), "The tree synchronization object is not locked.");

		// abort, if the specified data value is not monitored
		if (!mDataValueReferenceMap.TryGetValue(path, out List<WeakReference<IUntypedDataInternal>> dataValueReferences) || dataValueReferences.Count == 0)
			return;

		IUntypedDataValueInternal idvi = null;
		for (int i = dataValueReferences.Count - 1; i >= 0; i--)
		{
			WeakReference<IUntypedDataInternal> weakReference = dataValueReferences[i];
			if (!weakReference.TryGetTarget(out IUntypedDataInternal data)) continue;
			IUntypedDataValueInternal x = data.InvalidateDataValueReference(suppressChangedEvent);
			Debug.Assert(idvi == null || idvi == x);
			dataValueReferences.RemoveAt(i);
		}
	}

	#endregion

	#region Things to Do Periodically

	/// <summary>
	/// Checks for dead <see cref="Data{T}"/> objects and removes the data values they reference, if appropriate.
	/// </summary>
	/// <returns>
	/// <c>true</c> if the root node is still alive;<br/>
	/// <c>false</c> if the root node has been collected meanwhile.
	/// </returns>
	internal bool CheckPeriodically()
	{
		// is called by the data tree manager host
		// => locking the tree sync is necessary...

		lock (Sync)
		{
			List<string> pathsToRemove = null;
			foreach (KeyValuePair<string, List<WeakReference<IUntypedDataInternal>>> kvp in mDataValueReferenceMap)
			{
				(string path, List<WeakReference<IUntypedDataInternal>> dataValueReferences) = (kvp.Key, kvp.Value);

				// check whether the data value at the current path is still referenced
				bool isReferenced = false;
				for (int i = dataValueReferences.Count - 1; i >= 0; i--)
				{
					WeakReference<IUntypedDataInternal> weakReference = dataValueReferences[i];

					if (weakReference.TryGetTarget(out IUntypedDataInternal _))
					{
						// Data<T> object is still alive
						// => keep the referenced data value
						isReferenced = true;
					}
					else
					{
						// Data<T> object was reclaimed
						// => remove Data<T> object from the set of monitored objects
						dataValueReferences.RemoveAt(i);
						if (dataValueReferences.Count != 0) continue;
						pathsToRemove ??= [];
						pathsToRemove.Add(path);
					}
				}

				// abort, if the data value at the current path is still alive
				if (isReferenced) continue;

				// data value is not referenced by Data<T> objects anymore
				// => check whether we can clean up a little bit

				// get access to the root node, if still possible (might have been reclaimed meanwhile)
				if (!mRootNode.TryGetTarget(out DataNode root))
					return false;

				// remove the referenced data value from the tree, if it is a dummy value
				IUntypedDataValueInternal dataValue = root.GetDataValueUntypedUnsynced(path.AsSpan().TrimStart(PathHelpers.SeparatorChar));
				if (dataValue == null) continue;
				if (!dataValue.IsDummyUnsynced) continue;
				DataNode node = dataValue.ParentNodeUnsynced;
				dataValue.RemoveUnsynced();
				node.CleanupDummiesUnsynced();
			}

			if (pathsToRemove == null)
				return true;

			foreach (string pathToRemove in pathsToRemove)
			{
				mDataValueReferenceMap.Remove(pathToRemove);
			}
		}

		return true;
	}

	/// <summary>
	/// Removes the data value referenced by a <see cref="Data{T}"/> object,
	/// if the data value is dummy and not referenced by other <see cref="Data{T}"/> objects.
	/// </summary>
	/// <param name="data">IData object referencing the data value.</param>
	/// <param name="weakReferences">List of <see cref="IUntypedData"/> objects referencing the data value.</param>
	private void RemoveReferencedDataValueIfPossibleUnsynced(
		IUntypedData                              data,
		List<WeakReference<IUntypedDataInternal>> weakReferences)
	{
		Debug.Assert(Monitor.IsEntered(Sync), "The tree synchronization object is not locked.");

		// abort, if the list contains references that are still alive
		if (weakReferences.Exists(weakReference => weakReference.TryGetTarget(out IUntypedDataInternal _)))
			return;

		// data value is not referenced by Data<T> objects anymore
		// => check whether we can clean up a little bit

		// remove the referenced data value from the tree, if it is a dummy value
		if (!mRootNode.TryGetTarget(out DataNode root)) return;
		IUntypedDataValueInternal dataValue = root.GetDataValueUntypedUnsynced(data.Path.AsSpan().TrimStart(PathHelpers.SeparatorChar));
		if (dataValue == null) return;
		if (!dataValue.IsDummyUnsynced) return;
		DataNode node = dataValue.ParentNode;
		dataValue.RemoveUnsynced();
		node.CleanupDummiesUnsynced();
	}

	#endregion
}
