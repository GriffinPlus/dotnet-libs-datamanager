///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Event arguments for events concerning changes to a collection of data values.
/// </summary>
public sealed class DataValueCollectionChangedEventArgs : DataManagerEventArgs
{
	private readonly IUntypedDataValueInternal[] mDataValues;

	/// <summary>
	/// Initializes a new instance of the <see cref="DataValueCollectionChangedEventArgs"/> class (for an initial update).
	/// </summary>
	/// <param name="parentNode">Node the set of data values belongs to.</param>
	/// <param name="dataValues">The initial set of data values.</param>
	internal DataValueCollectionChangedEventArgs(
		DataNode                               parentNode,
		IEnumerable<IUntypedDataValueInternal> dataValues)
	{
		Action = CollectionChangedAction.InitialUpdate;
		ParentNode = parentNode;
		mDataValues = [.. dataValues];
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DataValueCollectionChangedEventArgs"/> class (for a collection change)
	/// </summary>
	/// <param name="action">Indicates whether <paramref name="dataValue"/> was added to or removed from <paramref name="parentNode"/>.</param>
	/// <param name="parentNode">Data node containing the added/removed data value.</param>
	/// <param name="dataValue">The added/removed data value.</param>
	internal DataValueCollectionChangedEventArgs(
		CollectionChangedAction   action,
		DataNode                  parentNode,
		IUntypedDataValueInternal dataValue)
	{
		Action = action;
		ParentNode = parentNode;
		mDataValues = [dataValue];
	}

	/// <summary>
	/// Gets a value indicating the kind of change to the collection.
	/// </summary>
	public CollectionChangedAction Action { get; }

	/// <summary>
	/// Gets the data node containing the affected data value(s).
	/// </summary>
	public DataNode ParentNode { get; }

	/// <summary>
	/// Gets the added/removed child node if
	/// <see cref="Action"/> is <see cref="CollectionChangedAction.Added"/> respectively
	/// <see cref="CollectionChangedAction.Removed"/>.
	/// </summary>
	public IUntypedDataValue DataValue => mDataValues.Length > 0 ? mDataValues[0] : null;

	/// <summary>
	/// Gets the added/removed data value if <see cref="Action"/> is
	/// <see cref="CollectionChangedAction.Added"/> respectively
	/// <see cref="CollectionChangedAction.Removed"/> or the initial set of data values if
	/// <see cref="Action"/> is <see cref="CollectionChangedAction.InitialUpdate"/>.
	/// </summary>
	public IEnumerable<IUntypedDataValue> DataValues => mDataValues;
}
