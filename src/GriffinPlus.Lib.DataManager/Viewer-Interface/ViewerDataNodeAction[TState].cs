///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager.Viewer;

/// <summary>
/// Delegate for the callback method needed by the
/// <see cref="ViewerDataNode.ExecuteAtomically{TState}(ViewerDataNodeAction{TState},TState)"/> method.
/// </summary>
/// <param name="node">Node in the locked data tree.</param>
/// <param name="state">Some state object needed by the action.</param>
public delegate void ViewerDataNodeAction<in TState>(ViewerDataNode node, TState state);
