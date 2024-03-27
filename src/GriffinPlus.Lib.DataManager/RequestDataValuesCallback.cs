///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Callback required by <see cref="DataValueCollection.RequestItems"/> and <see cref="DataValueCollection.RequestItemsAsync"/>.
/// </summary>
/// <param name="sender">The collection itself.</param>
/// <param name="values">
/// Values in the collection at the time <see cref="DataValueCollection.RequestItems"/> or
/// <see cref="DataValueCollection.RequestItemsAsync"/> was called.
/// </param>
/// <param name="context">
/// Some context object passed to <see cref="DataValueCollection.RequestItems"/> or
/// <see cref="DataValueCollection.RequestItemsAsync"/>.
/// </param>
public delegate void RequestDataValuesCallback(DataValueCollection sender, IUntypedDataValue[] values, object context);
