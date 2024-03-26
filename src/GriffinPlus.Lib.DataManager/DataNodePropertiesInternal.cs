///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// The <see cref="DataNodeProperties"/> combined with administrative flags (for internal use only).
/// </summary>
[Flags]
enum DataNodePropertiesInternal
{
	/// <summary>
	/// None.
	/// </summary>
	None = 0x00000000,

	//
	// user properties (mask: 0x000000FF)
	//

	/// <summary>
	/// The data node is serialized when persisting.
	/// </summary>
	Persistent = 0x00000001,

	/// <summary>
	/// All property flags that can be explicitly set by the user.
	/// </summary>
	UserProperties = Persistent,

	//
	// administrative properties (mask: 0x0000FF00)
	//

	/// <summary>
	/// Administrative Flag:
	/// The data node is just a placeholder for a regular data node.
	/// </summary>
	Dummy = 0x00000100,

	/// <summary>
	/// All property flags that are used for administrative purposes only and must not be explicitly set by the user.
	/// </summary>
	AdministrativeProperties = Dummy,

	//
	// other masks
	//

	/// <summary>
	/// All property flags that might occur.
	/// </summary>
	All = UserProperties | AdministrativeProperties
}
