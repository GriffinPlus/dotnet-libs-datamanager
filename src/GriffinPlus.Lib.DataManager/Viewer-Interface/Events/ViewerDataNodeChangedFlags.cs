///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.DataManager.Viewer;

/// <summary>
/// Flags indicating which properties in <see cref="ViewerDataNode"/> have changed.
/// </summary>
[Flags]
public enum ViewerDataNodeChangedFlags
{
	// --------------------------------------------------------------------------------------------------------------------
	// ATTENTION:
	// This enumeration uses the same encoding for the data node properties as the DataNodeProperties enumeration.
	// Therefore, you must not change the encoding of these members!
	// --------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Nothing has changed.
	/// </summary>
	None = 0x00000000,

	/// <summary>
	/// The <see cref="ViewerDataNode.IsPersistent"/> property has changed.
	/// </summary>
	IsPersistent = DataNodeChangedFlagsInternal.IsPersistent,

	/// <summary>
	/// The <see cref="ViewerDataNode.IsDummy"/> property has changed.
	/// </summary>
	IsDummy = DataNodeChangedFlagsInternal.IsDummy,

	/// <summary>
	/// The <see cref="ViewerDataNode.Properties"/> property has changed.
	/// </summary>
	Properties = DataNodeChangedFlagsInternal.Properties,

	/// <summary>
	/// The <see cref="ViewerDataNode.Name"/> property has changed.
	/// </summary>
	Name = DataNodeChangedFlagsInternal.Name,

	/// <summary>
	/// The <see cref="ViewerDataNode.Path"/> property has changed.
	/// </summary>
	Path = DataNodeChangedFlagsInternal.Path,

	/// <summary>
	/// All flags (except <see cref="InitialUpdate"/>).
	/// </summary>
	All = IsPersistent | IsDummy | Properties | Name | Path,

	/// <summary>
	/// This flag indicates that this is the first update to a monitored data node.
	/// </summary>
	InitialUpdate = 0x00010000
}
