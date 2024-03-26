///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

// ReSharper disable CompareNonConstrainedGenericWithNull
// ReSharper disable InvertIf

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// A data manager serializer using an XML file (thread-safe).<br/>
/// The serializer supports the following types out of the box:<br/>
/// - .NET type: <see cref="System.Boolean"/>, XML type: Boolean<br/>
/// - .NET type: <see cref="System.SByte"/>, XML type: Int8<br/>
/// - .NET type: <see cref="System.Byte"/>, XML type: UInt8<br/>
/// - .NET type: <see cref="System.Int16"/>, XML type: Int16<br/>
/// - .NET type: <see cref="System.UInt16"/>, XML type: UInt16<br/>
/// - .NET type: <see cref="System.Int32"/>, XML type: Int32<br/>
/// - .NET type: <see cref="System.UInt32"/>, XML type: UInt32<br/>
/// - .NET type: <see cref="System.Int64"/>, XML type: Int64<br/>
/// - .NET type: <see cref="System.UInt64"/>, XML type: UInt64<br/>
/// - .NET type: <see cref="System.Single"/>, XML type: Float32<br/>
/// - .NET type: <see cref="System.Double"/>, XML type: Float64<br/>
/// - .NET type: <see cref="System.String"/>, XML type: String<br/>
/// - .NET type: <see cref="System.DateTime"/> , XML type: DateTime<br/>
/// Support for additional types can be added using <see cref="AddTypeSerializer"/>.
/// </summary>
public partial class DataManagerXmlSerializer : DataManagerSerializerBase
{
	#region Constants

	private const string NodeElementName        = "Node";
	private const string ValueElementName       = "Value";
	private const string NameAttributeName      = "name";
	private const string ValueAttributeName     = "value";
	private const string TypeAttributeName      = "type";
	private const string ValueTypeAttributeName = "valtype";

	#endregion

	#region Class Initialization (Support for Built-In Types)

	private static readonly Dictionary<Type, TypeSerializer> sTypeSerializersByType = [];

	/// <summary>
	/// Initializes the <see cref="DataManagerXmlSerializer"/> class.
	/// </summary>
	static DataManagerXmlSerializer()
	{
		// System.Boolean
		// --------------------------------------------------------------------------------------------------------------
		AddStaticTypeSerializer<bool>(
			"Boolean",
			(writer, value) =>
			{
				writer.WriteAttributeString(ValueAttributeName, value.ToString());
			},
			(reader, name) =>
			{
				string value = GetAndCheckValueAttribute(reader, name);

				if (bool.TryParse(value, out bool result))
					return result;

				int lineNumber = ((IXmlLineInfo)reader).LineNumber;
				throw new SerializationException($"Deserializing data value '{name}' failed (line: {lineNumber}). Parsing the '{ValueAttributeName}' attribute failed.");
			},
			obj => obj);

		// System.SByte
		// --------------------------------------------------------------------------------------------------------------
		AddStaticTypeSerializer<sbyte>(
			"Int8",
			(writer, value) =>
			{
				writer.WriteAttributeString(ValueAttributeName, value.ToString());
			},
			(reader, name) =>
			{
				string value = GetAndCheckValueAttribute(reader, name);

				if (sbyte.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out sbyte result))
					return result;

				int lineNumber = ((IXmlLineInfo)reader).LineNumber;
				throw new SerializationException($"Deserializing data value '{name}' failed (line: {lineNumber}). Parsing the '{ValueAttributeName}' attribute failed.");
			},
			obj => obj);

		// System.Byte
		// --------------------------------------------------------------------------------------------------------------
		AddStaticTypeSerializer<byte>(
			"UInt8",
			(writer, value) =>
			{
				writer.WriteAttributeString(ValueAttributeName, value.ToString());
			},
			(reader, name) =>
			{
				string value = GetAndCheckValueAttribute(reader, name);

				if (byte.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out byte result))
					return result;

				int lineNumber = ((IXmlLineInfo)reader).LineNumber;
				throw new SerializationException($"Deserializing data value '{name}' failed (line: {lineNumber}). Parsing the '{ValueAttributeName}' attribute failed.");
			},
			obj => obj);

		// System.Int16
		// --------------------------------------------------------------------------------------------------------------
		AddStaticTypeSerializer<short>(
			"Int16",
			(writer, value) =>
			{
				writer.WriteAttributeString(ValueAttributeName, value.ToString());
			},
			(reader, name) =>
			{
				string value = GetAndCheckValueAttribute(reader, name);

				if (short.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out short result))
					return result;

				int lineNumber = ((IXmlLineInfo)reader).LineNumber;
				throw new SerializationException($"Deserializing data value '{name}' failed (line: {lineNumber}). Parsing the '{ValueAttributeName}' attribute failed.");
			},
			obj => obj);

		// System.UInt16
		// --------------------------------------------------------------------------------------------------------------
		AddStaticTypeSerializer<ushort>(
			"UInt16",
			(writer, value) =>
			{
				writer.WriteAttributeString(ValueAttributeName, value.ToString());
			},
			(reader, name) =>
			{
				string value = GetAndCheckValueAttribute(reader, name);

				if (ushort.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out ushort result))
					return result;

				int lineNumber = ((IXmlLineInfo)reader).LineNumber;
				throw new SerializationException($"Deserializing data value '{name}' failed (line: {lineNumber}). Parsing the '{ValueAttributeName}' attribute failed.");
			},
			obj => obj);

		// System.Int32
		// --------------------------------------------------------------------------------------------------------------
		AddStaticTypeSerializer<int>(
			"Int32",
			(writer, value) =>
			{
				writer.WriteAttributeString(ValueAttributeName, value.ToString());
			},
			(reader, name) =>
			{
				string value = GetAndCheckValueAttribute(reader, name);

				if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
					return result;

				int lineNumber = ((IXmlLineInfo)reader).LineNumber;
				throw new SerializationException($"Deserializing data value '{name}' failed (line: {lineNumber}). Parsing the '{ValueAttributeName}' attribute failed.");
			},
			obj => obj);

		// System.UInt32
		// --------------------------------------------------------------------------------------------------------------
		AddStaticTypeSerializer<uint>(
			"UInt32",
			(writer, value) =>
			{
				writer.WriteAttributeString(ValueAttributeName, value.ToString());
			},
			(reader, name) =>
			{
				string value = GetAndCheckValueAttribute(reader, name);

				if (uint.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out uint result))
					return result;

				int lineNumber = ((IXmlLineInfo)reader).LineNumber;
				throw new SerializationException($"Deserializing data value '{name}' failed (line: {lineNumber}). Parsing the '{ValueAttributeName}' attribute failed.");
			},
			obj => obj);

		// System.Int64
		// --------------------------------------------------------------------------------------------------------------
		AddStaticTypeSerializer<long>(
			"Int64",
			(writer, value) =>
			{
				writer.WriteAttributeString(ValueAttributeName, value.ToString());
			},
			(reader, name) =>
			{
				string value = GetAndCheckValueAttribute(reader, name);

				if (long.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out long result))
					return result;

				int lineNumber = ((IXmlLineInfo)reader).LineNumber;
				throw new SerializationException($"Deserializing data value '{name}' failed (line: {lineNumber}). Parsing the '{ValueAttributeName}' attribute failed.");
			},
			obj => obj);

		// System.UInt64
		// --------------------------------------------------------------------------------------------------------------
		AddStaticTypeSerializer<ulong>(
			"UInt64",
			(writer, value) =>
			{
				writer.WriteAttributeString(ValueAttributeName, value.ToString());
			},
			(reader, name) =>
			{
				string value = GetAndCheckValueAttribute(reader, name);

				if (ulong.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out ulong result))
					return result;

				int lineNumber = ((IXmlLineInfo)reader).LineNumber;
				throw new SerializationException($"Deserializing data value '{name}' failed (line: {lineNumber}). Parsing the '{ValueAttributeName}' attribute failed.");
			},
			obj => obj);

		// System.Single
		// --------------------------------------------------------------------------------------------------------------
		AddStaticTypeSerializer<float>(
			"Float32",
			(writer, value) =>
			{
				string valueString = string.Format(CultureInfo.InvariantCulture, "{0}", value);
				writer.WriteAttributeString(ValueAttributeName, valueString);
			},
			(reader, name) =>
			{
				string value = GetAndCheckValueAttribute(reader, name);

				if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float result))
					return result;

				int lineNumber = ((IXmlLineInfo)reader).LineNumber;
				throw new SerializationException($"Deserializing data value '{name}' failed (line: {lineNumber}). Parsing the '{ValueAttributeName}' attribute failed.");
			},
			obj => obj);

		// System.Double
		// --------------------------------------------------------------------------------------------------------------
		AddStaticTypeSerializer<double>(
			"Float64",
			(writer, value) =>
			{
				string valueString = string.Format(CultureInfo.InvariantCulture, "{0}", value);
				writer.WriteAttributeString(ValueAttributeName, valueString);
			},
			(reader, name) =>
			{
				string value = GetAndCheckValueAttribute(reader, name);

				if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
					return result;

				int lineNumber = ((IXmlLineInfo)reader).LineNumber;
				throw new SerializationException($"Deserializing data value '{name}' failed (line: {lineNumber}). Parsing the '{ValueAttributeName}' attribute failed.");
			},
			obj => obj);

		// System.String
		// --------------------------------------------------------------------------------------------------------------
		AddStaticTypeSerializer<string>(
			"String",
			(writer, value) =>
			{
				writer.WriteAttributeString(ValueAttributeName, value.ToString());
			},
			(reader, name) => GetAndCheckValueAttribute(reader, name, true),
			obj => obj // string is immutable
		);

		// System.DateTime
		// --------------------------------------------------------------------------------------------------------------
		AddStaticTypeSerializer<DateTime>(
			"DateTime",
			(writer, value) =>
			{
				writer.WriteAttributeString(ValueAttributeName, ((DateTime)value).ToString("o", CultureInfo.InvariantCulture));
			},
			(reader, name) =>
			{
				string value = GetAndCheckValueAttribute(reader, name);

				if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime result))
					return result;

				int lineNumber = ((IXmlLineInfo)reader).LineNumber;
				throw new SerializationException($"Deserializing data value '{name}' failed (line: {lineNumber}). Parsing the '{ValueAttributeName}' attribute failed.");
			},
			obj => obj // DateTime is a value type
		);
	}

	/// <summary>
	/// Adds a serializer/deserializer callback for the specified type.
	/// </summary>
	/// <param name="type">String identifying the type in XML.</param>
	/// <param name="serializer">Callback to invoke when serializing an instance of the specified type.</param>
	/// <param name="deserializer">Callback to invoke when deserializing an instance of the specified type.</param>
	/// <param name="copier">Callback to invoke when copying an instance of the specified type.</param>
	private static void AddStaticTypeSerializer<T>(
		string                   type,
		TypeSerializerDelegate   serializer,
		TypeDeserializerDelegate deserializer,
		TypeCopierDelegate       copier)
	{
		sTypeSerializersByType.Add(typeof(T), new TypeSerializer(type, serializer, deserializer, copier));
	}

	/// <summary>
	/// Gets the value of the 'value' attribute of the element the xml reader is currently reading.
	/// </summary>
	/// <param name="reader">The <see cref="XmlReader"/> to read from.</param>
	/// <param name="name">Name of the data value to deserialize.</param>
	/// <param name="allowMissingValue">
	/// <c>true</c> to return <c>null</c> if the 'value' attribute is missing;<br/>
	/// <c>false</c> to throw an exception if it is missing.
	/// </param>
	/// <returns>The value of the 'value' attribute.</returns>
	/// <exception cref="SerializationException">Deserializing the specified data value failed. The 'value' attribute does not exist.</exception>
	private static string GetAndCheckValueAttribute(XmlReader reader, string name, bool allowMissingValue = false)
	{
		// if the reader is at an attribute
		// => move it to the element containing the attribute before iterating through the attributes
		if (reader.NodeType == XmlNodeType.Attribute)
			reader.MoveToElement();

		// return the value of the 'value' attribute
		while (reader.MoveToNextAttribute())
		{
			if (reader.Value == ValueAttributeName)
				return reader.Value;
		}

		// the 'value' attribute does not exist
		// => return null if missing value is ok
		if (allowMissingValue)
			return null;

		// the 'value' attribute must not be missing
		// => throw exception...
		int lineNumber = ((IXmlLineInfo)reader).LineNumber;
		throw new SerializationException($"Deserializing data value '{name}' failed (line: {lineNumber}). The 'value' attribute does not exist.");
	}

	#endregion

	#region Adding Type Serializers

	private readonly Dictionary<Type, TypeSerializer> mTypeSerializersByType = new(sTypeSerializersByType);

	/// <summary>
	/// Adds a serializer/deserializer/copier callbacks for the specified type.
	/// </summary>
	/// <typeparam name="T">Type of the data type to handle.</typeparam>
	/// <param name="xmlTypeName">String identifying the type in XML.</param>
	/// <param name="serializer">Callback to invoke when serializing an instance of the specified type.</param>
	/// <param name="deserializer">Callback to invoke when deserializing an instance of the specified type.</param>
	/// <param name="copier">Callback to invoke when copying an instance of the specified type.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="xmlTypeName"/>, <paramref name="serializer"/>, <paramref name="deserializer"/> or <paramref name="copier"/> is <c>null</c>.
	/// </exception>
	public void AddTypeSerializer<T>(
		string                   xmlTypeName,
		TypeSerializerDelegate   serializer,
		TypeDeserializerDelegate deserializer,
		TypeCopierDelegate       copier)
	{
		if (xmlTypeName == null) throw new ArgumentNullException(nameof(xmlTypeName));
		if (serializer == null) throw new ArgumentNullException(nameof(serializer));
		if (deserializer == null) throw new ArgumentNullException(nameof(deserializer));
		if (copier == null) throw new ArgumentNullException(nameof(copier));

		mTypeSerializersByType.Add(typeof(T), new TypeSerializer(xmlTypeName, serializer, deserializer, copier));
	}

	/// <summary>
	/// Adds a serializer/deserializer/copier callbacks for the specified type.
	/// </summary>
	/// <param name="type">Type of the data type to handle.</param>
	/// <param name="xmlTypeName">String identifying the type in XML.</param>
	/// <param name="serializer">Callback to invoke when serializing an instance of the specified type.</param>
	/// <param name="deserializer">Callback to invoke when deserializing an instance of the specified type.</param>
	/// <param name="copier">Callback to invoke when copying an instance of the specified type.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="type"/>, <paramref name="xmlTypeName"/>, <paramref name="serializer"/>, <paramref name="deserializer"/>
	/// or <paramref name="copier"/> is <c>null</c>.
	/// </exception>
	public void AddTypeSerializer(
		Type                     type,
		string                   xmlTypeName,
		TypeSerializerDelegate   serializer,
		TypeDeserializerDelegate deserializer,
		TypeCopierDelegate       copier)
	{
		if (type == null) throw new ArgumentNullException(nameof(type));
		if (xmlTypeName == null) throw new ArgumentNullException(nameof(xmlTypeName));
		if (serializer == null) throw new ArgumentNullException(nameof(serializer));
		if (deserializer == null) throw new ArgumentNullException(nameof(deserializer));
		if (copier == null) throw new ArgumentNullException(nameof(copier));

		mTypeSerializersByType.Add(type, new TypeSerializer(xmlTypeName, serializer, deserializer, copier));
	}

	#endregion

	#region Serialization

	/// <summary>
	/// Gets or sets the settings to use when writing XML with <see cref="XmlWriter"/>.
	/// The default settings result in writing UTF-8 encoded XML with indentation using tabulators
	/// and without newlines on attributes.
	/// </summary>
	private XmlWriterSettings XmlWriterSettings { get; } = new()
	{
		Indent = true,
		IndentChars = "\t",
		NewLineOnAttributes = false,
		Encoding = Encoding.UTF8,
		ConformanceLevel = ConformanceLevel.Document
	};

	/// <inheritdoc/>
	public override void Serialize(DataNode node, Stream stream)
	{
		if (node == null) throw new ArgumentNullException(nameof(node));
		if (stream == null) throw new ArgumentNullException(nameof(stream));

		try
		{
			using var xmlWriter = XmlWriter.Create(stream, XmlWriterSettings);
			xmlWriter.WriteStartDocument();
			node.ExecuteAtomically(
				(root, writer) => WriteNode(writer, root),
				xmlWriter);
			xmlWriter.WriteEndDocument();
		}
		catch (Exception ex) when (ex is not SerializationException)
		{
			throw new SerializationException("Serializing failed. See inner exception for details.", ex);
		}
	}

	/// <summary>
	/// Writes the specified <see cref="DataNode"/> using a <see cref="XmlWriter"/>.
	/// </summary>
	/// <param name="writer">The <see cref="XmlWriter"/> instance to use.</param>
	/// <param name="node">The <see cref="DataNode"/> instance to write.</param>
	private void WriteNode(XmlWriter writer, DataNode node)
	{
		writer.WriteStartElement(NodeElementName);
		writer.WriteAttributeString(NameAttributeName, node.Name);
		foreach (DataNode child in node.Children) WriteNode(writer, child);
		foreach (IUntypedDataValue value in node.Values) WriteValue(writer, value);
		writer.WriteEndElement();
	}

	/// <summary>
	/// Writes the specified data value using a <see cref="XmlWriter"/>.
	/// </summary>
	/// <param name="writer">The <see cref="XmlWriter"/> instance to use.</param>
	/// <param name="dataValue">The <see cref="IUntypedDataValue"/> instance to write.</param>
	/// <exception cref="SerializationException"><paramref name="dataValue"/> is of an unsupported type.</exception>
	private void WriteValue(XmlWriter writer, IUntypedDataValue dataValue)
	{
		writer.WriteStartElement(ValueElementName);
		writer.WriteAttributeString(NameAttributeName, dataValue.Name);

		// try to get a serializer callback that handles the value type
		if (!mTypeSerializersByType.TryGetValue(dataValue.Type, out TypeSerializer typeSerializer))
			throw new SerializationException($"Serializing data values of type '{dataValue.GetType().FullName}' is not supported (data value path: {dataValue.Path}).");

		// write the type of the property value
		writer.WriteAttributeString(TypeAttributeName, typeSerializer.Type);

		object value = dataValue.Value;
		if (value != null)
		{
			// write the type of the value, if it differs from the type of the data value
			Type valueType = value.GetType();
			if (dataValue.Type != valueType)
			{
				typeSerializer = mTypeSerializersByType[valueType];
				writer.WriteAttributeString(ValueTypeAttributeName, typeSerializer.Type);
			}
		}

		// write the value using the type serializer callback
		if (value != null)
		{
			typeSerializer.SerializerCallback(writer, dataValue.Value);
		}

		writer.WriteEndElement();
	}

	#endregion

	#region Deserialization

	/// <inheritdoc/>
	public override DataNode Deserialize(Stream stream, DataTreeManagerHost dataTreeManagerHost = null)
	{
		// TODO: Implement deserialization...
		throw new NotSupportedException("Deserialization is not supported, yet.");
	}

	#endregion

	#region Copying

	/// <inheritdoc/>
	public override T CopySerializableValue<T>(T obj)
	{
		// abort if the object to copy is null
		if (obj == null) return default;

		// copy the object using the registered copier callback of the type
		Type type = obj.GetType();
		if (mTypeSerializersByType.TryGetValue(type, out TypeSerializer serializer))
			return (T)serializer.CopierCallback(obj);

		// return object as is if it is immutable
		if (Immutability.IsImmutable(type))
			return obj;

		throw new SerializationException($"Copying objects of type '{type.FullName}' is not supported. Consider adding a type serializer for it.");
	}

	#endregion
}
