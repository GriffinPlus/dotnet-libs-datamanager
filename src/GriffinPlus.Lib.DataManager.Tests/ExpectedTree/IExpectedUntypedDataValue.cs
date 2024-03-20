using System;

using GriffinPlus.Lib.DataManager.Viewer;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Untyped interface to unify accessing generic <see cref="ExpectedDataValue{T}"/> objects.
/// </summary>
interface IExpectedUntypedDataValue : IExpectedItem
{
	/// <summary>
	/// Gets or sets the actual data value associated with the current expected data value.
	/// </summary>
	IUntypedDataValueInternal ActualDataValue { get; set; }

	/// <summary>
	/// Gets the expected value of the <see cref="DataValue{T}.Name"/> property,
	/// respectively the <see cref="IUntypedDataValue.Name"/> property.
	/// </summary>
	string Name { get; set; }

	/// <summary>
	/// Gets the expected value of the <see cref="DataValue{T}.Path"/> property,
	/// respectively the <see cref="IUntypedDataValue.Path"/> property.
	/// </summary>
	string Path { get; }

	/// <summary>
	/// Gets the expected value of the <see cref="DataValue{T}.Properties"/> property,
	/// respectively the <see cref="IUntypedDataValue.Properties"/> property.
	/// </summary>
	DataValueProperties Properties { get; set; }

	/// <summary>
	/// Gets the expected value of the <see cref="DataValue{T}.IsPersistent"/> property,
	/// respectively the <see cref="IUntypedDataValue.IsPersistent"/> property.
	/// </summary>
	bool IsPersistent { get; }

	/// <summary>
	/// Gets the expected value of the <see cref="DataValue{T}.IsDummyUnsynced"/> property,
	/// respectively the <see cref="IUntypedDataValueInternal.IsDummyUnsynced"/> property.
	/// </summary>
	bool IsDummy { get; }

	/// <summary>
	/// Gets the expected value of the <see cref="DataValue{T}.IsDetached"/> property,
	/// respectively the <see cref="IUntypedDataValue.IsDetached"/> property.
	/// </summary>
	bool IsDetached { get; }

	/// <summary>
	/// Gets the root node of the current expected data value.
	/// </summary>
	ExpectedDataNode RootNode { get; }

	/// Gets the expected value of the
	/// <see cref="DataValue{T}.Timestamp"/>
	/// property,
	/// respectively the
	/// <see cref="IUntypedDataValue.Timestamp"/>
	/// property.
	DateTime Timestamp { get; set; }

	/// <summary>
	/// Gets the expected value of the <see cref="DataValue{T}.Type"/> property,
	/// respectively the <see cref="IUntypedDataValue.Type"/> property.
	/// </summary>
	Type Type { get; }

	/// Gets the expected value of the
	/// <see cref="DataValue{T}.Value"/>
	/// property,
	/// respectively the
	/// <see cref="IUntypedDataValue.Value"/>
	/// property.
	object Value { get; set; }

	/// <summary>
	/// Adds an actual data value to the specified actual parent node as specified by the current expected data value
	/// and sets <see cref="ActualDataValue"/> to this data value.
	/// </summary>
	/// <param name="actualParent">Parent of the current value.</param>
	/// <returns>The added actual data value.</returns>
	void InitializeActualDataValue(DataNode actualParent);

	/// <summary>
	/// Checks whether the current bound actual data value corresponds to the current expected data value.
	/// </summary>
	void AssertConsistency();

	/// <summary>
	/// Checks whether the current bound actual viewer data value corresponds to the current expected data value.
	/// </summary>
	/// <param name="actualViewerDataValue">Actual viewer data value to compare with the expected data value.</param>
	void CheckViewerConsistency(IUntypedViewerDataValue actualViewerDataValue);

	/// <summary>
	/// Binds the specified actual data value to the current expected value and checks whether both are equal.
	/// </summary>
	/// <param name="actualDataValue">Actual data value to compare the current expected data value with.</param>
	void BindAndValidateActualDataTreeInternal(IUntypedDataValueInternal actualDataValue);

	/// <summary>
	/// Recursively check whether data value events were raised as expected.
	/// </summary>
	void AssertExpectedEventsWereRaised();

	/// <summary>
	/// Clears the lists of expected events.
	/// </summary>
	void ResetExpectedEvents();

	/// <summary>
	/// Clears the lists of received events.
	/// </summary>
	void ResetReceivedEvents();

	/// <summary>
	/// Raises events informing about a change to the <see cref="Path"/> property.
	/// </summary>
	void NotifyPathChanged();
}
