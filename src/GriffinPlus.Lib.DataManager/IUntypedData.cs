///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Interface providing unified untyped access to <see cref="Data{T}"/>.
/// </summary>
public interface IUntypedData
{
	/// <summary>
	/// Occurs when the data value reference changes.<br/>
	/// The first invocation of the event handler notifies about the initial state of the data value reference.<br/>
	/// Subsequent invocations notify about changes to the data value reference.<br/>
	/// Event invocations will be scheduled using the <see cref="SynchronizationContext"/> of the registering thread, if available.<br/>
	/// If the registering thread does not have a synchronization context, event invocations will be scheduled on the data tree manager thread.
	/// </summary>
	/// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
	event EventHandler<UntypedDataChangedEventArgs> UntypedChanged;

	/// <summary>
	/// Occurs when the data value reference changes.<br/>
	/// The first invocation of the event handler notifies about the initial state of the data value reference.<br/>
	/// Subsequent invocations notify about changes to the data value reference.<br/>
	/// Event invocations will be scheduled on the data tree manager thread.
	/// </summary>
	/// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
	event EventHandler<UntypedDataChangedEventArgs> UntypedChangedAsync;

	/// <summary>
	/// Gets the root node of the tree the referenced data value resides in.
	/// </summary>
	DataNode RootNode { get; }

	/// <summary>
	/// Gets the name of the referenced data value.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets the type of the referenced data value.
	/// The actual type of the object in the data value may be of a derived type.
	/// </summary>
	Type Type { get; }

	/// <summary>
	/// Gets a value indicating whether <see cref="Value"/> provides a valid value.
	/// </summary>
	/// <exception cref="ObjectDisposedException">
	/// The object has been disposed.
	/// </exception>
	bool HasValue { get; }

	/// <summary>
	/// Gets or sets the value of the referenced data value.
	/// </summary>
	/// <exception cref="DataValueDoesNotExistException">
	/// The referenced data value does not exist, yet (thrown by getter only).
	/// </exception>
	/// <exception cref="DataValueReferenceBrokenException">
	/// The link between the data value reference and its data value is broken (thrown by setter only).
	/// </exception>
	/// <exception cref="InvalidCastException">
	/// <paramref name="value"/> is not assignable to the actual value type.
	/// </exception>
	/// <exception cref="ObjectDisposedException">
	/// The object has been disposed.
	/// </exception>
	/// <exception cref="SerializationException">
	/// Serializing the specified <paramref name="value"/> failed.
	/// </exception>
	/// <remarks>
	/// This property returns a copy of the object stored in the referenced data value. Since the latest value is
	/// returned, the returned object might differ from the 'new value' passed to a handler of the following events:<br/>
	/// - <see cref="UntypedChanged"/><br/>
	/// - <see cref="UntypedChangedAsync"/><br/>
	/// If the referenced data value does not exist and should be persistent, setting this property the first time
	/// makes the containing node and all nodes up to the root node persistent as well to ensure that the value is
	/// included when persisting.
	/// </remarks>
	object Value { get; set; }

	/// <summary>
	/// Gets the timestamp of the referenced data value, i.e. the date/time the value was set the last time.
	/// </summary>
	/// <exception cref="ObjectDisposedException">
	/// The object has been disposed.
	/// </exception>
	DateTime Timestamp { get; }

	/// <summary>
	/// Gets or sets the properties of the referenced data value.
	/// </summary>
	/// <exception cref="ArgumentException">
	/// The specified property flags are not supported.
	/// </exception>
	/// <exception cref="DataValueReferenceBrokenException">
	/// The link between the data value reference and its data value is broken (thrown by setter only).
	/// </exception>
	/// <exception cref="ObjectDisposedException">
	/// The object has been disposed.
	/// </exception>
	/// <remarks>
	/// If the referenced data value exists, setting the <see cref="DataValueProperties.Persistent"/> flag makes
	/// all nodes up to the root node persistent as well to ensure that the data value is included when persisting
	/// the data tree.<br/>
	/// If the referenced data value does not exist, the containing node and its parents are not touched. In this case the
	/// nodes are made persistent when the data value is set the first time.
	/// </remarks>
	DataValueProperties Properties { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the referenced data value is included when persisting the data tree.
	/// </summary>
	/// <exception cref="DataValueReferenceBrokenException">
	/// The link between the data value reference and its data value is broken (thrown by setter only).
	/// </exception>
	/// <exception cref="ObjectDisposedException">
	/// The object has been disposed.
	/// </exception>
	/// <remarks>
	/// If the referenced data value exists, setting this property to <c>true</c> makes all nodes up to the root
	/// node persistent as well to ensure that the data value is included when persisting the data tree.<br/>
	/// If the referenced data value does not exist, the containing node and its parents are not touched. In this case
	/// the nodes are made persistent when the data value is set the first time.
	/// </remarks>
	bool IsPersistent { get; set; }

	/// <summary>
	/// Gets a value indicating whether the data value reference is healthy,
	/// i.e. it is successfully bound to its data value and tracking its changes.
	/// </summary>
	/// <exception cref="ObjectDisposedException">
	/// The object has been disposed.
	/// </exception>
	bool IsHealthy { get; }

	/// <summary>
	/// Gets the path of the referenced data value in the data tree.
	/// </summary>
	string Path { get; }

	/// <summary>
	/// Sets the value and the data value properties of the referenced data value in an atomic operation.
	/// </summary>
	/// <param name="value">New value.</param>
	/// <param name="properties">New data value properties.</param>
	/// <exception cref="ArgumentException">
	/// The specified property flags are not supported.
	/// </exception>
	/// <exception cref="DataValueReferenceBrokenException">
	/// The link between the data value reference and its data value is broken.
	/// </exception>
	/// <exception cref="InvalidCastException">
	/// <paramref name="value"/> is not assignable to the actual value type.
	/// </exception>
	/// <exception cref="ObjectDisposedException">
	/// The object has been disposed.
	/// </exception>
	/// <exception cref="SerializationException">
	/// Serializing the specified <paramref name="value"/> failed.
	/// </exception>
	/// <remarks>
	/// If <paramref name="properties"/> contains the <see cref="DataValueProperties.Persistent"/> flag, it makes
	/// all nodes up to the root node persistent as well to ensure the referenced data value is included when
	/// persisting the data tree.
	/// </remarks>
	void Set(object value, DataValueProperties properties);

	/// <summary>
	/// Sets the value and the data value properties of the referenced data value in an atomic operation.
	/// </summary>
	/// <param name="value">New value.</param>
	/// <param name="propertiesToSet">Data value property flags to set.</param>
	/// <param name="propertiesToClear">Data value property flags to clear.</param>
	/// <exception cref="ArgumentException">
	/// The specified property flags are not supported.
	/// </exception>
	/// <exception cref="DataValueReferenceBrokenException">
	/// The link between the data value reference and its data value is broken.
	/// </exception>
	/// <exception cref="InvalidCastException">
	/// <paramref name="value"/> is not assignable to the actual value type.
	/// </exception>
	/// <exception cref="ObjectDisposedException">
	/// The object has been disposed.
	/// </exception>
	/// <exception cref="SerializationException">
	/// Serializing the specified <paramref name="value"/> failed.
	/// </exception>
	/// <remarks>
	/// If <paramref name="propertiesToSet"/> contains the <see cref="DataValueProperties.Persistent"/> flag, it
	/// makes all nodes up to the root node persistent as well to ensure the referenced data value is included when
	/// persisting the data tree.<br/>
	/// If <paramref name="propertiesToSet"/> and <paramref name="propertiesToClear"/> specify the same flag, the flag is
	/// effectively set.
	/// </remarks>
	void Set(object value, DataValueProperties propertiesToSet, DataValueProperties propertiesToClear);
}
