///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Interface providing unified untyped access to <see cref="DataValue{T}"/>.
/// </summary>
public interface IUntypedDataValue
{
	/// <summary>
	/// Occurs when something in the current data value changes.<br/>
	/// The first invocation of the event handler notifies about the initial state of the data value.<br/>
	/// Subsequent invocations notify about changes to the collection.<br/>
	/// Event invocations will be scheduled using the <see cref="SynchronizationContext"/> of the registering thread, if available.<br/>
	/// If the registering thread does not have a synchronization context, event invocations will be scheduled on the data tree manager thread.
	/// </summary>
	event EventHandler<UntypedDataValueChangedEventArgs> UntypedChanged;

	/// <summary>
	/// Is called when something in the current data value changes.<br/>
	/// The first invocation of the event handler notifies about the initial state of the data value.<br/>
	/// Subsequent invocations notify about changes to the collection.<br/>
	/// Event invocations will be scheduled on the data tree manager thread.
	/// </summary>
	event EventHandler<UntypedDataValueChangedEventArgs> UntypedChangedAsync;

	/// <summary>
	/// Gets the data tree manager that is responsible for the data tree the data value belongs to.
	/// </summary>
	DataTreeManager DataTreeManager { get; }

	/// <summary>
	/// Gets the name of the data value.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets the type of the data value.
	/// The actual type of the object in the data value may be of a derived type.
	/// </summary>
	Type Type { get; }

	/// <summary>
	/// Gets or sets the value of the data value.
	/// </summary>
	/// <exception cref="InvalidCastException"><paramref name="value"/> is not assignable to the actual value type.</exception>
	/// <exception cref="SerializationException">Serializing the specified <paramref name="value"/> failed.</exception>
	/// <remarks>
	/// This property returns a copy of the object stored in the data value. Since the latest value is returned,
	/// the returned object might differ from the 'new value' passed to a handler of the following events:<br/>
	/// - <see cref="UntypedChanged"/><br/>
	/// - <see cref="UntypedChangedAsync"/>
	/// </remarks>
	object Value { get; set; }

	/// <summary>
	/// Gets the timestamp of the data value, i.e. the date/time the value was set the last time.
	/// </summary>
	DateTime Timestamp { get; }

	/// <summary>
	/// Gets or sets the properties of the data value.
	/// </summary>
	/// <exception cref="ArgumentException">The specified property flags are not supported.</exception>
	/// <remarks>
	/// Setting the <see cref="DataValueProperties.Persistent"/> flag makes all nodes up to the root node persistent
	/// as well to ensure that the data value is included when persisting the data tree.
	/// </remarks>
	DataValueProperties Properties { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the data value is included when persisting the data tree.
	/// </summary>
	/// <remarks>
	/// Setting this property to <c>true</c> makes all nodes up to the root node persistent as well to ensure that
	/// the data value is included when persisting the data tree.
	/// </remarks>
	bool IsPersistent { get; set; }

	/// <summary>
	/// Gets a value indicating whether the current data value has been detached from the data tree.
	/// </summary>
	bool IsDetached { get; }

	/// <summary>
	/// Gets the path of the current data value in the data tree.
	/// </summary>
	string Path { get; }

	/// <summary>
	/// Gets the node containing the current data value.<br/>
	/// <c>null</c> if the data value has been removed from the data tree.
	/// </summary>
	DataNode ParentNode { get; }

	/// <summary>
	/// Gets the root node of the data tree the current data value resides in.<br/>
	/// <c>null</c> if the data value has been removed from the data tree.
	/// </summary>
	DataNode RootNode { get; }

	/// <summary>
	/// Sets the value and the data value properties of the current data value in an atomic operation.
	/// </summary>
	/// <param name="value">New value.</param>
	/// <param name="properties">New data value properties.</param>
	/// <exception cref="ArgumentException">The specified property flags are not supported.</exception>
	/// <exception cref="InvalidCastException"><paramref name="value"/> is not assignable to the actual value type.</exception>
	/// <exception cref="SerializationException">Serializing the specified <paramref name="value"/> failed.</exception>
	/// <remarks>
	/// If <paramref name="properties"/> contains the <see cref="DataValueProperties.Persistent"/> flag, it makes all
	/// nodes up to the root node persistent as well to ensure the current value is included when persisting the data tree.
	/// </remarks>
	void Set(object value, DataValueProperties properties);

	/// <summary>
	/// Sets the value and the data value properties of the current data value in an atomic operation.
	/// </summary>
	/// <param name="value">New value.</param>
	/// <param name="propertiesToSet">Data value property flags to set.</param>
	/// <param name="propertiesToClear">Data value property flags to clear.</param>
	/// <exception cref="ArgumentException">The specified property flags are not supported.</exception>
	/// <exception cref="InvalidCastException"><paramref name="value"/> is not assignable to the actual value type.</exception>
	/// <exception cref="SerializationException">Serializing the specified <paramref name="value"/> failed.</exception>
	/// <remarks>
	/// If <paramref name="propertiesToSet"/> contains the <see cref="DataValueProperties.Persistent"/> flag, it makes all
	/// nodes up to the root node persistent as well to ensure the current value is included when persisting the data tree.<br/>
	/// If <paramref name="propertiesToSet"/> and <paramref name="propertiesToClear"/> specify the same flag, the flag is
	/// effectively set.
	/// </remarks>
	void Set(object value, DataValueProperties propertiesToSet, DataValueProperties propertiesToClear);

	/// <summary>
	/// Removes the current data value from the value collection of its parent node.<br/>
	/// This effectively detaches the data value from the data tree.
	/// </summary>
	void Remove();

	/// <summary>
	/// Locks the entire data tree and calls the specified action method within the lock.
	/// </summary>
	/// <param name="action">Action to perform within the locked section.</param>
	void ExecuteAtomically(UntypedDataValueAction action);

	/// <summary>
	/// Locks the entire data tree and calls the specified action method within the lock.
	/// </summary>
	/// <param name="action">Action to perform within the locked section.</param>
	/// <param name="state">Some state object passed to the action.</param>
	void ExecuteAtomically<TState>(UntypedDataValueAction<TState> action, TState state);
}
