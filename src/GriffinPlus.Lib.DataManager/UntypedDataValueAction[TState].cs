///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Delegate for the callback method needed by the
/// <see cref="IUntypedDataValue.ExecuteAtomically{TState}"/> method.
/// </summary>
/// <typeparam name="TState">Some custom object passed to the action.</typeparam>
/// <param name="dataValue">Data value in the locked data tree.</param>
/// <param name="state">Some state object needed by the action.</param>
public delegate void UntypedDataValueAction<in TState>(IUntypedDataValue dataValue, TState state);
