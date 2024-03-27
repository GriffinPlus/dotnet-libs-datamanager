///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager.Viewer;

/// <summary>
/// Callback required by <see cref="ViewerDataValueCollection.RequestItems"/> and <see cref="ViewerDataValueCollection.RequestItemsAsync"/>.
/// </summary>
/// <param name="sender">The collection itself.</param>
/// <param name="values">
/// Values in the collection at the time <see cref="ViewerDataValueCollection.RequestItems"/> or
/// <see cref="ViewerDataValueCollection.RequestItemsAsync"/> was called.
/// </param>
/// <param name="context">
/// Some context object passed to <see cref="ViewerDataValueCollection.RequestItems"/> or
/// <see cref="ViewerDataValueCollection.RequestItemsAsync"/>.
/// </param>
public delegate void RequestViewerDataValuesCallback(ViewerDataValueCollection sender, IUntypedViewerDataValue[] values, object context);
