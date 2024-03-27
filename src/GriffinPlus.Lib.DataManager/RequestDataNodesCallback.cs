///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Callback required by <see cref="ChildDataNodeCollection.RequestItems"/> and <see cref="ChildDataNodeCollection.RequestItemsAsync"/>.
/// </summary>
/// <param name="sender">The collection itself.</param>
/// <param name="nodes">
/// Nodes in the collection at the time <see cref="ChildDataNodeCollection.RequestItems"/> or
/// <see cref="ChildDataNodeCollection.RequestItemsAsync"/> was called.
/// </param>
/// <param name="context">
/// Some context object passed to <see cref="ChildDataNodeCollection.RequestItems"/> or
/// <see cref="ChildDataNodeCollection.RequestItemsAsync"/>.
/// </param>
public delegate void RequestDataNodesCallback(ChildDataNodeCollection sender, DataNode[] nodes, object context);
