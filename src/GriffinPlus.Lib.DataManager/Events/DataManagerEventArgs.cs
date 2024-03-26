///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Base class for arguments used by data manager events.
/// </summary>
public abstract class DataManagerEventArgs : EventArgs
{
	/// <summary>
	/// Object that can be used for protecting access to the event arguments using monitor synchronization.
	/// </summary>
	protected static readonly object Sync = new();
}
