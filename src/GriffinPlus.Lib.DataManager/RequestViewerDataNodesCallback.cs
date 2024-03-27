///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using GriffinPlus.Lib.DataManager.Viewer;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Callback required by <see cref="ViewerChildDataNodeCollection.RequestItems"/> and <see cref="ViewerChildDataNodeCollection.RequestItemsAsync"/>.
/// </summary>
/// <param name="sender">The collection itself.</param>
/// <param name="nodes">
/// Nodes in the collection at the time <see cref="ViewerChildDataNodeCollection.RequestItems"/> or
/// <see cref="ViewerChildDataNodeCollection.RequestItemsAsync"/> was called.
/// </param>
/// <param name="context">
/// Some context object passed to <see cref="ChildDataNodeCollection.RequestItems"/> or
/// <see cref="ViewerChildDataNodeCollection.RequestItemsAsync"/>.
/// </param>
public delegate void RequestViewerDataNodesCallback(ViewerChildDataNodeCollection sender, DataNode[] nodes, object context);
