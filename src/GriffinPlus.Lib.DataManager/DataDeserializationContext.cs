///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Context class used during deserialization of a data tree.
/// </summary>
class DataDeserializationContext
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DataDeserializationContext"/> class.
	/// </summary>
	/// <param name="parentNode">Parent node.</param>
	/// <param name="dataTreeManagerHost">Data tree manager host to use when creating the root node of the deserialized tree.</param>
	/// <param name="serializer">Serializer to use for serializing, deserializing and copying data in the data tree.</param>
	public DataDeserializationContext(DataNode parentNode, DataTreeManagerHost dataTreeManagerHost, IDataManagerSerializer serializer)
	{
		ParentNode = parentNode;
		DataTreeManagerHost = dataTreeManagerHost;
		Serializer = serializer;
	}

	/// <summary>
	/// Gets the parent node.
	/// </summary>
	public DataNode ParentNode { get; }

	/// <summary>
	/// Gets the data tree manager host to use when creating the root node of the deserialized tree.
	/// </summary>
	public DataTreeManagerHost DataTreeManagerHost { get; }

	/// <summary>
	/// Gets the serializer to use for serializing, deserializing and copying data in the data tree.
	/// </summary>
	public IDataManagerSerializer Serializer { get; }
}
