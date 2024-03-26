///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager.Viewer;

/// <summary>
/// Delegate for the callback method needed by the
/// <see cref="IUntypedViewerDataValue.ExecuteAtomically{TState}(UntypedViewerDataValueAction{TState},TState)"/> method.
/// </summary>
/// <param name="value">The current data value.</param>
/// <param name="state">Some state object needed by the action.</param>
public delegate void UntypedViewerDataValueAction<in TState>(IUntypedViewerDataValue value, TState state);
