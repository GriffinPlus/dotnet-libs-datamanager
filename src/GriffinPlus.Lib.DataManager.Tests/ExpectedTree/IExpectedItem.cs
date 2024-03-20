namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Interface providing access to common properties of expected data nodes and data values.
/// </summary>
interface IExpectedItem
{
	/// <summary>
	/// Gets or sets the expected parent data node.
	/// </summary>
	ExpectedDataNode Parent { get; set; }
}
