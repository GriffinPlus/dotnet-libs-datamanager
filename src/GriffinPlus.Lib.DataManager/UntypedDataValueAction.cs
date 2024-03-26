///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Delegate for the callback method needed by the
/// <see cref="IUntypedDataValue.ExecuteAtomically"/> method.
/// </summary>
/// <param name="dataValue">Data value in the locked data tree.</param>
public delegate void UntypedDataValueAction(IUntypedDataValue dataValue);
