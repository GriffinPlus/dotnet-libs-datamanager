///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Specifies node properties.
/// </summary>
[Flags]
public enum DataNodeProperties
{
	/// <summary>
	/// No flags at all.
	/// </summary>
	None = DataValuePropertiesInternal.None,

	/// <summary>
	/// The data node is serialized when persisting.
	/// </summary>
	Persistent = DataNodePropertiesInternal.Persistent,

	/// <summary>
	/// All public property flags.
	/// </summary>
	All = DataNodePropertiesInternal.UserProperties
}
