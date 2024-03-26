///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

using GriffinPlus.Lib.DataManager.Viewer;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Interface for data value classes providing functionality for internal use only.
/// </summary>
interface IUntypedDataValueInternal : IUntypedDataValue
{
	/// <summary>
	/// Gets a <see cref="IUntypedViewerDataValue"/> wrapping the current value for a viewer.
	/// </summary>
	/// <returns>The wrapper value.</returns>
	IUntypedViewerDataValue ViewerWrapper { get; }

	/// <summary>
	/// Gets the name of the data value (for internal use only, not synchronized)
	/// </summary>
	string NameUnsynced { get; }

	/// <summary>
	/// Gets the timestamp of the data value (for internal use only, not synchronized).
	/// </summary>
	DateTime TimestampUnsynced { get; }

	/// <summary>
	/// Gets the root node of the data tree the current data value resides in
	/// (for internal use only, not synchronized).
	/// </summary>
	DataNode RootNodeUnsynced { get; }

	/// <summary>
	/// Gets the <see cref="DataNode"/> containing the data value (for internal use only, not synchronized).
	/// </summary>
	DataNode ParentNodeUnsynced { get; }

	/// <summary>
	/// Gets or sets the internal value instance of the data value (for internal use only, not synchronized).
	/// </summary>
	/// <remarks>
	/// This property returns a copy of the object stored in the data value. Since the latest value is returned,
	/// the returned object might differ from the 'new value' passed to a handler of the following events:<br/>
	/// - <see cref="IUntypedDataValue.UntypedChanged"/><br/>
	/// - <see cref="IUntypedDataValue.UntypedChangedAsync"/>
	/// </remarks>
	object ValueUnsynced { get; }

	/// <summary>
	/// Gets the path of the data value in the data tree (for internal use only, not synchronized).
	/// </summary>
	string PathUnsynced { get; }

	/// <summary>
	/// Gets or sets the properties of the data value (for internal use only, not synchronized).
	/// </summary>
	/// <remarks>
	/// This property supports setting and clearing administrative flags.<br/>
	/// If the current value is a regular value, setting the <see cref="DataValuePropertiesInternal.Persistent"/> flag
	/// makes all nodes up to the root node persistent as well to ensure the current value is included when persisting
	/// the data tree.<br/>
	/// If the current value is a dummy value, the containing node and its parents are not touched. In this case the
	/// nodes are made persistent when the value is set the first time converting the dummy value to a regular value.
	/// </remarks>
	DataValuePropertiesInternal PropertiesUnsynced { get; set; }

	/// <summary>
	/// Gets a value indicating whether the current data value is dummy (for internal use only, not synchronized).
	/// </summary>
	bool IsDummyUnsynced { get; }

	/// <summary>
	/// Sets the value and the data value properties of the current data value in an atomic operation
	/// (for internal use only, not synchronized, does not copy the value, raises events)
	/// </summary>
	/// <param name="value">New value.</param>
	/// <param name="propertiesToSet">Data value property flags to set.</param>
	/// <param name="propertiesToClear">Data value property flags to clear.</param>
	/// <exception cref="InvalidCastException"><paramref name="value"/> is not assignable to the actual value type.</exception>
	/// <remarks>
	/// This method supports setting and clearing administrative flags.<br/>
	/// If <paramref name="propertiesToSet"/> contains the <see cref="DataValuePropertiesInternal.Persistent"/> flag, it
	/// makes all nodes up to the root node persistent as well to ensure the current value is included when
	/// persisting the data tree.<br/>
	/// If <paramref name="propertiesToSet"/> and <paramref name="propertiesToClear"/> specify the same flag, the
	/// flag is effectively set.
	/// </remarks>
	void SetUnsynced(object value, DataValuePropertiesInternal propertiesToSet, DataValuePropertiesInternal propertiesToClear);

	/// <summary>
	/// Removes the data value from the value collection of its parent node (for internal use only, not synchronized).<br/>
	/// This method is able to remove dummy data values.
	/// </summary>
	void RemoveUnsynced();

	/// <summary>
	/// Marks the data value as 'detached' and resets the reference to the containing node
	/// (for internal use only, not synchronized).
	/// </summary>
	void DetachFromDataTreeUnsynced();

	/// <summary>
	/// Re-evaluates the path of the current data value in the data tree and updates the path variable accordingly
	/// (for internal use only, not synchronized).
	/// </summary>
	void UpdatePathUnsynced();

	/// <summary>
	/// Updates the data tree manager (for internal use only, not synchronized).
	/// </summary>
	/// <param name="dataTreeManager">The data tree manager to use.</param>
	void UpdateDataTreeManagerUnsynced(DataTreeManager dataTreeManager);

	/// <summary>
	/// Copies the current data value to the data value collection of the specified node
	/// (for internal use only, not synchronized).
	/// </summary>
	/// <param name="destinationNode">The destination node to copy the current data value below.</param>
	void CopyValuesUnsynced(DataNode destinationNode);
}
