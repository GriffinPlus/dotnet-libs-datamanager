///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Flags indicating which properties in a data node have changed.
/// </summary>
[Flags]
public enum DataNodeChangedFlags
{
	// --------------------------------------------------------------------------------------------------------------------
	// ATTENTION:
	// This enumeration uses the same encoding for the data node properties as the DataNodeProperties enumeration.
	// Therefore, you must not change the encoding of these members!
	// --------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Nothing has changed.
	/// </summary>
	None = DataNodeChangedFlagsInternal.None,

	/// <summary>
	/// The <see cref="DataNode.IsPersistent"/> property has changed.
	/// </summary>
	IsPersistent = DataNodeChangedFlagsInternal.IsPersistent,

	/// <summary>
	/// The <see cref="DataNode.Properties"/> property has changed.
	/// </summary>
	Properties = DataNodeChangedFlagsInternal.Properties,

	/// <summary>
	/// The <see cref="DataNode.Name"/> property has changed.
	/// </summary>
	Name = DataNodeChangedFlagsInternal.Name,

	/// <summary>
	/// The <see cref="DataNode.Path"/> property has changed.
	/// </summary>
	Path = DataNodeChangedFlagsInternal.Path,

	/// <summary>
	/// All flags (except <see cref="InitialUpdate"/>).
	/// </summary>
	All = DataNodeChangedFlagsInternal.AllUserFlags,

	/// <summary>
	/// This flag indicates that this is the first update to a monitored data node.
	/// </summary>
	InitialUpdate = DataNodeChangedFlagsInternal.InitialUpdate
}
