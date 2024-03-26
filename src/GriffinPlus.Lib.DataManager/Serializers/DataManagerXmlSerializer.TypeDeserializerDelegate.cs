///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Xml;

namespace GriffinPlus.Lib.DataManager;

partial class DataManagerXmlSerializer
{
	/// <summary>
	/// A callback method that handles the deserialization of the value of a <see cref="DataValue{T}"/>
	/// from XML using the specified <see cref="XmlReader"/> instance.
	/// </summary>
	/// <param name="reader">The <see cref="XmlReader"/> instance to read from.</param>
	/// <param name="name">Name of the data value being deserialized.</param>
	/// <return>The deserialized value.</return>
	/// <exception cref="SerializationException">Deserialization failed due to some reason.</exception>
	public delegate object TypeDeserializerDelegate(XmlReader reader, string name);
}
