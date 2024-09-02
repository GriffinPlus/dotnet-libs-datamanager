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
/// Tests targeting the <see cref="DataTreeManagerHost"/> class.
/// </summary>
public class DataTreeManagerHostTests
{
	/// <summary>
	/// Tests creating an instance of the <see cref="DataTreeManagerHost"/> class.
	/// </summary>
	[Fact]
	public void Construct()
	{
		var host = new DataTreeManagerHost();
		Assert.Equal(TimeSpan.FromSeconds(10), host.PeriodicCheckInterval);
		Assert.NotNull(host.SynchronizationContext);
		Assert.True(host.Thread.IsAlive);
		Assert.True(host.Thread.IsBackground);

		// note:
		// the data tree manager host thread shuts down automatically after the data tree manager host
		// has been garbage collected (tested in separate test case)
	}

	/// <summary>
	/// Tests whether the thread in the <see cref="DataTreeManagerHost"/> shuts down on its own
	/// after the <see cref="DataTreeManagerHost"/> object has been garbage collected.
	/// </summary>
	[Fact]
	public void CheckAutomaticShutdown()
	{
		TimeSpan periodicCheckInterval = TimeSpan.FromMilliseconds(100);

		// create a data tree manager host and return a weak reference to it to check,
		// whether it is collected properly later on
		WeakReference weakDataDataTreeManagerHost = CreateDataTreeManagerHost(out Thread hostThread);

		// trigger garbage collection and wait for it to release the object (max. 10000 ms)
		GC.Collect();
		for (int i = 0; weakDataDataTreeManagerHost.IsAlive && i < 100; i++)
		{
			Thread.Sleep(100);
		}

		// the host should have been collected now
		Assert.False(weakDataDataTreeManagerHost.IsAlive);

		// the thread running the processing loop in the data tree manager host should terminate automatically
		// after it has detected that its host has been collected
		Thread.Sleep(periodicCheckInterval + TimeSpan.FromMilliseconds(50));
		Assert.False(hostThread.IsAlive);

		return;

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
		WeakReference CreateDataTreeManagerHost(out Thread thread)
		{
			// create a data tree manager host
			var host = new DataTreeManagerHost
			{
				PeriodicCheckInterval = periodicCheckInterval
			};

			// get the processing thread the runs inside the data tree manager host
			thread = host.Thread;

			// return a weak reference the data tree manager host to track whether it has been collected later on
			return new WeakReference(host);
		}
	}
}
