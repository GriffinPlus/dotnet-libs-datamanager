///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Properties of a data value.
/// </summary>
[Flags]
public enum DataValueProperties
{
	/// <summary>
	/// None.
	/// </summary>
	None = DataValuePropertiesInternal.None,

	/// <summary>
	/// The data value is included when persisting the data tree.
	/// </summary>
	Persistent = DataValuePropertiesInternal.Persistent,

	/// <summary>
	/// All public property flags.
	/// </summary>
	All = DataValuePropertiesInternal.UserProperties
}
