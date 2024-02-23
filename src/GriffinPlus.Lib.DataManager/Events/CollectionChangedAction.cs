///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// The kind of change to the collection of child nodes or values.
/// </summary>
public enum CollectionChangedAction
{
	/// <summary>
	/// Initial update containing the items in the collection at the time the event was registered.
	/// </summary>
	InitialUpdate,

	/// <summary>
	/// An item was added to the collection.
	/// </summary>
	Added,

	/// <summary>
	/// An item was removed from the collection.
	/// </summary>
	Removed
}
