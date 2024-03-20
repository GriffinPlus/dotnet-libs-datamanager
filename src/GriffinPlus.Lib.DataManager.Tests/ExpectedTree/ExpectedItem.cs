using System;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// An item in the expected data tree.
/// </summary>
class ExpectedItem : IExpectedItem
{
	/// <summary>
	/// Gets the time to wait for events to arrive at maximum.
	/// </summary>
	public TimeSpan MaximumTimeToWaitForEvents = TimeSpan.FromMilliseconds(5000);

	/// <summary>
	/// Gets or sets the expected parent data node.
	/// </summary>
	public ExpectedDataNode Parent { get; set; }
}
