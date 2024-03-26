///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Delegate for the callback method needed when copying/moving data trees.
/// </summary>
/// <param name="original">The data value to copy/move (source).</param>
/// <param name="copy">The copied/moved data value (destination).</param>
public delegate void DataValueModifierDelegate(IUntypedDataValue original, IUntypedDataValue copy);
