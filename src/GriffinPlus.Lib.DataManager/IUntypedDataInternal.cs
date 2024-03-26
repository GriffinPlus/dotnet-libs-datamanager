///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Interface providing unified untyped access to <see cref="Data{T}"/> (for internal use only).
/// </summary>
interface IUntypedDataInternal : IUntypedData
{
	/// <summary>
	/// Gets the referenced data value
	/// (<c>null</c> if the link between the data value reference and its data tree is broken).
	/// </summary>
	IUntypedDataValueInternal DataValueUnsynced { get; }

	/// <summary>
	/// Gets the timestamp of the referenced data value, i.e. the date/time the value was set the last time
	/// (for internal use only, not synchronized).
	/// </summary>
	DateTime TimestampUnsynced { get; }

	/// <summary>
	/// Gets the properties of the referenced data value (for internal use only, not synchronized).
	/// </summary>
	DataValuePropertiesInternal PropertiesUnsynced { get; }

	/// <summary>
	/// Gets or sets the value of the referenced data value.
	/// </summary>
	object ValueUnsynced { get; }

	/// <summary>
	/// Updates the data value reference (for internal use only, not synchronized).
	/// </summary>
	/// <returns>Data value that was previously referenced.</returns>
	IUntypedDataValueInternal UpdateDataValueReferenceUnsynced();

	/// <summary>
	/// Invalidates the data value reference (for internal use only, not synchronized).
	/// </summary>
	/// <param name="suppressChangedEvent">
	/// <c>true</c> to suppress raising the <see cref="Data{T}.Changed"/> event and the <see cref="Data{T}.ChangedAsync"/> event;<br/>
	/// otherwise <c>false</c>.
	/// </param>
	/// <returns>Data value that was previously referenced.</returns>
	IUntypedDataValueInternal InvalidateDataValueReferenceUnsynced(bool suppressChangedEvent);
}
