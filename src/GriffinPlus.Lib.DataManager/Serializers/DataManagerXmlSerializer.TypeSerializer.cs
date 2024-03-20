///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager;

partial class DataManagerXmlSerializer
{
	/// <summary>
	/// A record containing the name of a serialized value type and the methods to use for its serialization and deserialization.
	/// </summary>
	private struct TypeSerializer
	{
		public readonly string                   Type;
		public readonly TypeSerializerDelegate   SerializerCallback;
		public readonly TypeDeserializerDelegate DeserializerCallback; // TODO: xml deserialization is still missing, will be used when it is added...
		public readonly TypeCopierDelegate       CopierCallback;

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeSerializer"/> struct.
		/// </summary>
		/// <param name="type">Name of the handled type to use in XML.</param>
		/// <param name="serializerCallback">Callback method to use when serializing an instance of the type.</param>
		/// <param name="deserializerCallback">Callback method to use when deserializing an instance of the type.</param>
		/// <param name="copierCallback">Callback method to use when copying an instance of the type.</param>
		public TypeSerializer(
			string                   type,
			TypeSerializerDelegate   serializerCallback,
			TypeDeserializerDelegate deserializerCallback,
			TypeCopierDelegate       copierCallback)
		{
			Type = type;
			SerializerCallback = serializerCallback;
			DeserializerCallback = deserializerCallback;
			CopierCallback = copierCallback;
		}
	}
}
