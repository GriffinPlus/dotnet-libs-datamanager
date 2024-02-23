///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.DataManager.Viewer;

/// <summary>
/// Accessor for the internal state of a <see cref="DataValue{T}"/> (for viewers).
/// </summary>
/// <remarks>
/// This accessor can be used to gain access to dummy nodes and values in the data tree,
/// which is important for viewers to show the correct state of the data tree - with regular and dummy data.
/// Modifying operations do not affect dummy nodes as dummy nodes are managed by the data tree manager.
/// </remarks>
public sealed class ViewerDataValue<T> : IUntypedViewerDataValue
{
	/// <summary>
	/// Initializes a new <see cref="ViewerDataValue{T}"/> wrapping a <see cref="DataValue{T}"/>.
	/// </summary>
	/// <param name="value">The <see cref="DataValue{T}"/> to wrap.</param>
	internal ViewerDataValue(DataValue<T> value)
	{
		WrappedValue = value;
	}

	/// <inheritdoc cref="DataValue{T}.ViewerChanged"/>
	public event EventHandler<ViewerDataValueEventArgs<T>> Changed
	{
		add => WrappedValue.ViewerChanged += value;
		remove => WrappedValue.ViewerChanged -= value;
	}

	/// <inheritdoc cref="DataValue{T}.ViewerChangedAsync"/>
	public event EventHandler<ViewerDataValueEventArgs<T>> ChangedAsync
	{
		add => WrappedValue.ViewerChangedAsync += value;
		remove => WrappedValue.ViewerChangedAsync -= value;
	}

	/// <inheritdoc cref="IUntypedViewerDataValue.UntypedChanged"/>
	event EventHandler<UntypedViewerDataValueEventArgs> IUntypedViewerDataValue.UntypedChanged
	{
		add => WrappedValue.ViewerUntypedChanged += value;
		remove => WrappedValue.ViewerUntypedChanged -= value;
	}

	/// <inheritdoc cref="IUntypedViewerDataValue.UntypedChangedAsync"/>
	event EventHandler<UntypedViewerDataValueEventArgs> IUntypedViewerDataValue.UntypedChangedAsync
	{
		add => WrappedValue.ViewerUntypedChangedAsync += value;
		remove => WrappedValue.ViewerUntypedChangedAsync -= value;
	}

	/// <summary>
	/// Gets the wrapped data value.
	/// </summary>
	internal DataValue<T> WrappedValue { get; }

	/// <inheritdoc cref="IUntypedViewerDataValue.DataTreeManager"/>
	public DataTreeManager DataTreeManager => WrappedValue.DataTreeManager;

	/// <inheritdoc cref="IUntypedViewerDataValue.Name"/>
	public string Name => WrappedValue.Name;

	/// <inheritdoc cref="IUntypedViewerDataValue.Type"/>
	public Type Type => WrappedValue.Type;

	/// <summary>
	/// Gets or sets the value of the data value.
	/// </summary>
	/// <exception cref="DataValueDoesNotExistException">
	/// The data value is a dummy data value, i.e. it does not exist actually (thrown by getter only).
	/// </exception>
	/// <exception cref="SerializationException">Serializing the specified <paramref name="value"/> failed.</exception>
	/// <remarks>
	/// This property returns a copy of the object stored in the data value. Since the latest value is returned,
	/// the returned object might differ from the 'new value' passed to a handler of the following events:<br/>
	/// - <see cref="ViewerDataValue{T}.Changed"/><br/>
	/// - <see cref="ViewerDataValue{T}.ChangedAsync"/><br/>
	/// - <see cref="IUntypedViewerDataValue.UntypedChanged"/><br/>
	/// - <see cref="IUntypedViewerDataValue.UntypedChangedAsync"/><br/>
	/// If the data value is dummy and persistent, setting this property the first time makes the containing node
	/// and its parent nodes up to the root node persistent as well to ensure that the value is included when
	/// persisting.
	/// </remarks>
	public T Value
	{
		get => WrappedValue.Value;
		set => WrappedValue.Value = value;
	}

	/// <inheritdoc cref="IUntypedViewerDataValue.Value"/>
	object IUntypedViewerDataValue.Value
	{
		get => (WrappedValue as IUntypedDataValue).Value;
		set => (WrappedValue as IUntypedDataValue).Value = value;
	}

	/// <inheritdoc cref="IUntypedViewerDataValue.Timestamp"/>
	public DateTime Timestamp => WrappedValue.Timestamp;

	/// <inheritdoc cref="IUntypedViewerDataValue.Properties"/>
	public DataValueProperties Properties
	{
		get => WrappedValue.Properties;
		set => WrappedValue.Properties = value;
	}

	/// <inheritdoc cref="IUntypedViewerDataValue.IsPersistent"/>
	public bool IsPersistent
	{
		get => WrappedValue.IsPersistent;
		set => WrappedValue.IsPersistent = value;
	}

	/// <inheritdoc cref="IUntypedViewerDataValue.IsDetached"/>
	public bool IsDetached => WrappedValue.IsDetached;

	/// <inheritdoc cref="IUntypedViewerDataValue.IsDummy"/>
	public bool IsDummy => WrappedValue.ViewerIsDummy;

	/// <inheritdoc cref="IUntypedViewerDataValue.Path"/>
	public string Path => WrappedValue.Path;

	/// <inheritdoc cref="IUntypedViewerDataValue.ParentNode"/>
	public ViewerDataNode ParentNode => WrappedValue.ParentNode?.ViewerWrapper;

	/// <inheritdoc cref="IUntypedViewerDataValue.RootNode"/>
	public ViewerDataNode RootNode => WrappedValue.RootNode?.ViewerWrapper;

	/// <inheritdoc cref="DataValue{T}.Set(T,DataValueProperties)"/>
	public void Set(T value, DataValueProperties properties)
	{
		WrappedValue.Set(value, properties);
	}

	/// <inheritdoc cref="DataValue{T}.Set(T,DataValueProperties,DataValueProperties)"/>
	public void Set(T value, DataValueProperties propertiesToSet, DataValueProperties propertiesToClear)
	{
		WrappedValue.Set(value, propertiesToSet, propertiesToClear);
	}

	/// <inheritdoc cref="IUntypedViewerDataValue.Set(object,DataValueProperties)"/>
	public void Set(object value, DataValueProperties properties)
	{
		(WrappedValue as IUntypedDataValue).Set(value, properties);
	}

	/// <inheritdoc cref="IUntypedViewerDataValue.Set(object,DataValueProperties,DataValueProperties)"/>
	public void Set(object value, DataValueProperties propertiesToSet, DataValueProperties propertiesToClear)
	{
		(WrappedValue as IUntypedDataValue).Set(value, propertiesToSet, propertiesToClear);
	}

	/// <inheritdoc cref="IUntypedViewerDataValue.Remove()"/>
	public void Remove()
	{
		WrappedValue.Remove();
	}

	/// <inheritdoc cref="DataValue{T}.ViewerExecuteAtomically(ViewerDataValueAction{T})"/>
	public void ExecuteAtomically(ViewerDataValueAction<T> action)
	{
		WrappedValue.ViewerExecuteAtomically(action);
	}

	/// <inheritdoc cref="DataValue{T}.ViewerExecuteAtomically{TState}(ViewerDataValueAction{T,TState},TState)"/>
	public void ExecuteAtomically<TState>(ViewerDataValueAction<T, TState> action, TState state)
	{
		WrappedValue.ViewerExecuteAtomically(action, state);
	}

	/// <inheritdoc cref="IUntypedViewerDataValue.ExecuteAtomically(UntypedViewerDataValueAction)"/>
	public void ExecuteAtomically(UntypedViewerDataValueAction action)
	{
		WrappedValue.ViewerExecuteAtomically(action);
	}

	/// <inheritdoc cref="IUntypedViewerDataValue.ExecuteAtomically{TState}(UntypedViewerDataValueAction{TState},TState)"/>
	public void ExecuteAtomically<TState>(UntypedViewerDataValueAction<TState> action, TState state)
	{
		WrappedValue.ViewerExecuteAtomically(action, state);
	}
}
