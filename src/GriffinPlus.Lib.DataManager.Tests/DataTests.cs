///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.CompilerServices;
using System.Threading;

using Xunit;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Tests targeting the <see cref="Data{T}"/> class.
/// </summary>
[Collection(nameof(NoParallelizationCollection))]
public class DataTests
{
	[Theory]
	[InlineData("1")]     // 1 value
	[InlineData("1/2")]   // 1 node + 1 value
	[InlineData("1/2/3")] // 2 nodes + 1 value
	public void CheckCleanupWhenCollected(string path)
	{
		TimeSpan periodicCheckInterval = TimeSpan.FromMilliseconds(100);

		// create a new data tree manager host to avoid changing the configuration of
		// the default data tree manager host
		var dataTreeManagerHost = new DataTreeManagerHost
		{
			PeriodicCheckInterval = periodicCheckInterval
		};

		// create the root node of a new data tree
		var root = new DataNode(
			name: "Root",
			properties: DataNodeProperties.None,
			dataTreeManagerHost: dataTreeManagerHost,
			serializer: null);

		// create a data value reference and return a weak reference to it to check,
		// whether it is collected properly later on
		WeakReference weakDataValueReference = CreateDataValueReference(path, root);

		// trigger garbage collection and wait for it to release the object
		GC.Collect();
		GC.WaitForPendingFinalizers();

		// the data value reference should have been collected now...
		Assert.False(weakDataValueReference.IsAlive);

		// let the data tree manager clean up dummy data nodes/values that belong to collected data value references
		// (this method is periodically invoked by the data tree manager host thread)
		// root.DataTreeManager.CheckPeriodically();

		// wait for the data tree manager thread to clean up
		Thread.Sleep(10 * (int)periodicCheckInterval.TotalMilliseconds);

		// check whether the underlying dummy node has been removed as expected
		lock (root.DataTreeManager.Sync)
		{
			// check whether the underlying dummy value has been removed
			IUntypedDataValueInternal dataValue = root.GetDataValueUntypedInternalUnsynced(path.AsSpan());
			Assert.Null(dataValue);

			// in general, the root node should neither have child nodes nor data values now...
			// (ensures that there are no relics left)
			Assert.Empty(root.Children.InternalBuffer);
			Assert.Empty(root.Values.InternalBuffer);
		}

		return;

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
		static WeakReference CreateDataValueReference(string path, DataNode root)
		{
			// create a data value reference
			// (should create the path to a dummy data value as the data tree is empty)
			Data<int> dataValueReference = root.GetData<int>(path, DataValueProperties.None);
			Assert.NotNull(dataValueReference);

			// check whether the underlying dummy data value exists as expected
			lock (root.DataTreeManager.Sync)
			{
				IUntypedDataValueInternal dataValue = root.GetDataValueUntypedInternalUnsynced(path.AsSpan());
				Assert.NotNull(dataValue);
				Assert.Equal(DataValuePropertiesInternal.Dummy, dataValue.PropertiesUnsynced);
			}

			// return a weak reference the data value reference to track whether it has been collected later on
			return new WeakReference(dataValueReference);
		}
	}
}
