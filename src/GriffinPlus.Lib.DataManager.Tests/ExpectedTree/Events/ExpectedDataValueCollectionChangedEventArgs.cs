///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Expected event arguments for events concerning changes to a data value collection.
/// </summary>
/// <param name="action">The expected value of the <see cref="DataValueCollectionChangedEventArgs.Action"/> property.</param>
/// <param name="parentNode">The expected value of the <see cref="DataValueCollectionChangedEventArgs.ParentNode"/> property.</param>
/// <param name="dataValues">The expected value of the <see cref="DataValueCollectionChangedEventArgs.DataValues"/> property.</param>
sealed class ExpectedDataValueCollectionChangedEventArgs(
	CollectionChangedAction action,
	DataNode                parentNode,
	IUntypedDataValue[]     dataValues)
{
	/// <summary>
	/// Gets the expected value of the <see cref="DataValueCollectionChangedEventArgs.Action"/> property.
	/// </summary>
	public CollectionChangedAction Action { get; } = action;

	/// <summary>
	/// Gets the expected value of the <see cref="DataValueCollectionChangedEventArgs.ParentNode"/> property.
	/// </summary>
	public DataNode ParentNode { get; } = parentNode;

	/// <summary>
	/// Gets the expected value of the <see cref="DataValueCollectionChangedEventArgs.DataValue"/> property.
	/// </summary>
	public IUntypedDataValue DataValue => DataValues?.FirstOrDefault();

	/// <summary>
	/// Gets the expected value of the <see cref="DataValueCollectionChangedEventArgs.DataValues"/> property.
	/// </summary>
	public IUntypedDataValue[] DataValues = dataValues;
}
