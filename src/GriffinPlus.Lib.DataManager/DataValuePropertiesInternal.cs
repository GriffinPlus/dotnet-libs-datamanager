///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// The <see cref="DataValueProperties"/> combined with administrative flags (for internal use only).
/// </summary>
[Flags]
enum DataValuePropertiesInternal
{
	/// <summary>
	/// None.
	/// </summary>
	None = DataValueProperties.None,

	/// <summary>
	/// The data value is included when persisting the data tree.
	/// </summary>
	Persistent = DataValueProperties.Persistent,

	/// <summary>
	/// Administrative Flag:
	/// The data value is just a placeholder for a regular data value.
	/// </summary>
	Dummy = 0x00000010,

	/// <summary>
	/// Administrative Flag:
	/// The data value has been removed from the data tree, i.e. it does not have a parent data node anymore.
	/// </summary>
	Detached = 0x00000020,

	/// <summary>
	/// All property flags that can be explicitly set by the user.
	/// </summary>
	UserProperties = Persistent,

	/// <summary>
	/// All property flags that are used for administrative purposes only and must not be explicitly set by the user.
	/// </summary>
	AdministrativeProperties = Dummy | Detached,

	/// <summary>
	/// All property flags that might occur.
	/// </summary>
	All = UserProperties | AdministrativeProperties
}
