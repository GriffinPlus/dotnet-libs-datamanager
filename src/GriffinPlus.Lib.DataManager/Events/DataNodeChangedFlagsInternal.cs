///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

using GriffinPlus.Lib.DataManager.Viewer;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Flags indicating which properties in a data node have changed (for internal use only).
/// </summary>
[Flags]
enum DataNodeChangedFlagsInternal
{
	// --------------------------------------------------------------------------------------------------------------------
	// ATTENTION:
	// This enumeration uses the same encoding for the data node properties as the DataNodeProperties enumeration.
	// Therefore, you must not change the encoding of these members!
	// --------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Nothing has changed.
	/// </summary>
	None = DataNodePropertiesInternal.None, // 0x00000000

	/// <summary>
	/// The <see cref="DataNode.IsPersistent"/> property or the <see cref="ViewerDataNode.IsPersistent"/> has changed.
	/// </summary>
	IsPersistent = DataNodePropertiesInternal.Persistent, // 0x00000001

	/// <summary>
	/// The <see cref="ViewerDataNode.IsDummy"/> has changed.
	/// </summary>
	IsDummy = DataNodePropertiesInternal.Dummy, // 0x00000100

	/// <summary>
	/// The <see cref="DataNode.Properties"/> property or the <see cref="ViewerDataNode.Properties"/> has changed.
	/// </summary>
	Properties = 0x00010000,

	/// <summary>
	/// The <see cref="DataNode.Name"/> property or the <see cref="ViewerDataNode.Name"/> has changed.
	/// </summary>
	Name = 0x00020000,

	/// <summary>
	/// The <see cref="DataNode.Path"/> property or the <see cref="ViewerDataNode.Path"/> has changed.
	/// </summary>
	Path = 0x00040000,

	/// <summary>
	/// All user flags (except <see cref="InitialUpdate"/>).
	/// </summary>
	AllUserFlags = IsPersistent | Properties | Name | Path,

	/// <summary>
	/// All viewer flags (except <see cref="InitialUpdate"/>).
	/// </summary>
	AllViewerFlags = AllUserFlags | IsDummy,

	/// <summary>
	/// This flag indicates that this is the first update to a monitored data node.
	/// </summary>
	InitialUpdate = 0x10000000
}
