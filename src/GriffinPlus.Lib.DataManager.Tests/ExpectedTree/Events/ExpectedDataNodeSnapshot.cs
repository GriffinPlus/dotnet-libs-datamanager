///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// An expected snapshot of a <see cref="DataNode"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ExpectedDataNodeSnapshot"/> class.
/// </remarks>
/// <param name="name">The name of the data node.</param>
/// <param name="path">The path of the data node.</param>
/// <param name="properties">The properties of the data node.</param>
readonly struct ExpectedDataNodeSnapshot(string name, string path, DataNodePropertiesInternal properties)
{
	#region Name

	/// <summary>
	/// Gets the expected value of the <see cref="DataNodeSnapshot.Name"/> property.
	/// </summary>
	public string Name { get; } = name;

	#endregion

	#region Path

	/// <summary>
	/// Gets the expected value of the <see cref="DataNodeSnapshot.Path"/> property.
	/// </summary>
	public string Path { get; } = path;

	#endregion

	#region Properties (incl. Boolean Accessors)

	private readonly DataNodePropertiesInternal mProperties = properties;

	/// <summary>
	/// Gets the snapshotted value of the <see cref="DataNodeSnapshot.Properties"/> property.
	/// </summary>
	public DataNodeProperties Properties => (DataNodeProperties)(mProperties & DataNodePropertiesInternal.UserProperties);

	/// <summary>
	/// Gets the snapshotted value of the <see cref="DataNodeSnapshot.IsPersistent"/> property.
	/// </summary>
	public bool IsPersistent => (mProperties & DataNodePropertiesInternal.Persistent) != 0;

	#endregion
}
