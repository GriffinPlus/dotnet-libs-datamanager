///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Delegate for the internal <see cref="DataValue{T}.ChangedInternal"/> event.
/// </summary>
/// <typeparam name="T">Type of the value.</typeparam>
/// <param name="dataValue">Data value that has changed.</param>
/// <param name="changedFlags">Changed flags indicating what has changed.</param>
delegate void DataValueChangedInternalDelegate<T>(DataValue<T> dataValue, DataValueChangedFlagsInternal changedFlags);
