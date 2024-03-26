///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager;

partial class DataManagerXmlSerializer
{
	/// <summary>
	/// A record containing the name of a serialized value type and the methods to use for its serialization and deserialization.
	/// </summary>
	/// <param name="type">Name of the handled type to use in XML.</param>
	/// <param name="serializerCallback">Callback method to use when serializing an instance of the type.</param>
	/// <param name="deserializerCallback">Callback method to use when deserializing an instance of the type.</param>
	/// <param name="copierCallback">Callback method to use when copying an instance of the type.</param>
	private readonly struct TypeSerializer(
		string                   type,
		TypeSerializerDelegate   serializerCallback,
		TypeDeserializerDelegate deserializerCallback,
		TypeCopierDelegate       copierCallback)
	{
		public readonly string                   Type                 = type;
		public readonly TypeSerializerDelegate   SerializerCallback   = serializerCallback;
		public readonly TypeDeserializerDelegate DeserializerCallback = deserializerCallback; // TODO: xml deserialization is still missing, will be used when it is added...
		public readonly TypeCopierDelegate       CopierCallback       = copierCallback;
	}
}
