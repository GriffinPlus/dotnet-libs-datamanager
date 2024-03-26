///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace GriffinPlus.Lib.DataManager.Viewer;

/// <summary>
/// Event arguments for events concerning adding or removing a data value to/from <see cref="ViewerDataValueCollection"/>.
/// </summary>
public sealed class ViewerDataValueCollectionChangedEventArgs : DataManagerEventArgs
{
	private readonly IUntypedViewerDataValue[] mDataValues;

	/// <summary>
	/// Initializes a new instance of the <see cref="ViewerDataValueCollectionChangedEventArgs"/> class (for an initial update).
	/// </summary>
	/// <param name="parentNode">Node the set of data values belongs to.</param>
	/// <param name="dataValues">The initial set of data values.</param>
	internal ViewerDataValueCollectionChangedEventArgs(
		DataNode                                 parentNode,
		IReadOnlyList<IUntypedDataValueInternal> dataValues)
	{
		Action = CollectionChangedAction.InitialUpdate;
		ParentNode = parentNode.ViewerWrapper;
		int count = dataValues.Count;
		mDataValues = new IUntypedViewerDataValue[count];
		for (int i = 0; i < count; i++) mDataValues[i] = dataValues[i].ViewerWrapper;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ViewerDataValueCollectionChangedEventArgs"/> class (for a collection change).
	/// </summary>
	/// <param name="action">Indicates whether <paramref name="dataValue"/> was added to or removed from <paramref name="parentNode"/>.</param>
	/// <param name="parentNode">Data node containing the added/removed data value.</param>
	/// <param name="dataValue">The added/removed data value.</param>
	internal ViewerDataValueCollectionChangedEventArgs(
		CollectionChangedAction   action,
		DataNode                  parentNode,
		IUntypedDataValueInternal dataValue)
	{
		Action = action;
		ParentNode = parentNode.ViewerWrapper;
		mDataValues = [dataValue.ViewerWrapper];
	}

	/// <summary>
	/// Gets a value indicating the kind of change to the collection.
	/// </summary>
	public CollectionChangedAction Action { get; }

	/// <summary>
	/// Gets the data node containing the affected data value(s).
	/// </summary>
	public ViewerDataNode ParentNode { get; }

	/// <summary>
	/// Gets the added/removed child node if
	/// <see cref="Action"/> is <see cref="CollectionChangedAction.Added"/> respectively
	/// <see cref="CollectionChangedAction.Removed"/>.
	/// </summary>
	public IUntypedViewerDataValue DataValue => mDataValues.Length > 0 ? mDataValues[0] : null;

	/// <summary>
	/// Gets the added/removed data value if <see cref="Action"/> is
	/// <see cref="CollectionChangedAction.Added"/> respectively
	/// <see cref="CollectionChangedAction.Removed"/> or the initial set of data values if
	/// <see cref="Action"/> is <see cref="CollectionChangedAction.InitialUpdate"/>.
	/// </summary>
	public IEnumerable<IUntypedViewerDataValue> DataValues => mDataValues;
}
