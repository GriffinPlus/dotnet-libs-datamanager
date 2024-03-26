///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using GriffinPlus.Lib.Serialization;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// The default data manager serializer
/// (uses the Griffin+ <see cref="Serializer"/> from the 'GriffinPlus.Lib.Serialization' package).
/// </summary>
public class DataManagerDefaultSerializer : DataManagerSerializerBase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DataManagerDefaultSerializer"/> class.
	/// </summary>
	/// <param name="optimization">The kind of optimization to use when serializing.</param>
	/// <param name="useTolerantDeserialization">
	/// <c>true</c> to be tolerant when deserializing (see <see cref="Serializer.UseTolerantDeserialization"/>);<br/>
	/// <c>false</c> to be strict when deserializing.
	/// </param>
	public DataManagerDefaultSerializer(SerializationOptimization optimization, bool useTolerantDeserialization)
	{
		Optimization = optimization;
		UseTolerantDeserialization = useTolerantDeserialization;
	}

	/// <summary>
	/// Gets the kind of optimization the serializer uses when serializing.
	/// </summary>
	public SerializationOptimization Optimization { get; }

	/// <summary>
	/// Gets a value indicating whether the serializer is tolerant when deserializing
	/// (see <see cref="Serializer.UseTolerantDeserialization"/>).
	/// </summary>
	public bool UseTolerantDeserialization { get; }

	/// <inheritdoc/>
	public override void Serialize(DataNode node, Stream stream)
	{
		if (node == null) throw new ArgumentNullException(nameof(node));
		if (stream == null) throw new ArgumentNullException(nameof(stream));

		try
		{
			Serializer.Serialize(stream, node, null, Optimization);
		}
		catch (Exception ex)
		{
			throw new SerializationException("Serializing failed. See inner exception for details.", ex);
		}
	}

	/// <inheritdoc/>
	public override DataNode Deserialize(Stream stream, DataTreeManagerHost dataTreeManagerHost = null)
	{
		if (stream == null) throw new ArgumentNullException(nameof(stream));

		object obj;
		try
		{
			var context = new DataDeserializationContext(null, dataTreeManagerHost, this);
			obj = Serializer.Deserialize(stream, context, UseTolerantDeserialization);
		}
		catch (Exception ex)
		{
			throw new SerializationException("Deserializing failed. See inner exception for details.", ex);
		}

		if (obj.GetType() != typeof(DataNode))
			throw new SerializationException("Deserializing succeeded, but the stream does not contain a data node.");

		return obj as DataNode;
	}

	/// <inheritdoc/>
	public override T CopySerializableValue<T>(T obj)
	{
		try
		{
			return Serializer.CopySerializableObject(obj);
		}
		catch (Exception ex)
		{
			throw new SerializationException("Copying object failed. See inner exception for details.", ex);
		}
	}
}
