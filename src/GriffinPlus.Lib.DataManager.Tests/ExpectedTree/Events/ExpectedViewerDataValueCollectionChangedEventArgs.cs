///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using GriffinPlus.Lib.DataManager.Viewer;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Expected event arguments for events concerning changes to a viewer data value collection.
/// </summary>
/// <param name="action">The expected value of the <see cref="System.Action"/> property.</param>
/// <param name="parentNode">The expected value of the <see cref="ViewerDataValueCollectionChangedEventArgs.ParentNode"/> property.</param>
/// <param name="dataValues">The expected value of the <see cref="ViewerDataValueCollectionChangedEventArgs.DataValues"/> property.</param>
sealed class ExpectedViewerDataValueCollectionChangedEventArgs(
	CollectionChangedAction   action,
	ViewerDataNode            parentNode,
	IUntypedViewerDataValue[] dataValues)
{
	/// <summary>
	/// Gets the expected value of the <see cref="ViewerDataValueCollectionChangedEventArgs.Action"/> property.
	/// </summary>
	public CollectionChangedAction Action { get; } = action;

	/// <summary>
	/// Gets the expected value of the <see cref="ViewerDataValueCollectionChangedEventArgs.ParentNode"/> property.
	/// </summary>
	public ViewerDataNode ParentNode { get; } = parentNode;

	/// <summary>
	/// Gets the expected value of the <see cref="ViewerDataValueCollectionChangedEventArgs.DataValue"/> property.
	/// </summary>
	public IUntypedViewerDataValue DataValue => DataValues?.FirstOrDefault();

	/// <summary>
	/// Gets the expected value of the <see cref="ViewerDataValueCollectionChangedEventArgs.DataValues"/> property.
	/// </summary>
	public IUntypedViewerDataValue[] DataValues = dataValues;
}
