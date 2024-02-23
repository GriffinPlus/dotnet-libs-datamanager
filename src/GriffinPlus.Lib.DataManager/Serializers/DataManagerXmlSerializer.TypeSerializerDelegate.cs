///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Xml;

namespace GriffinPlus.Lib.DataManager;

partial class DataManagerXmlSerializer
{
	/// <summary>
	/// A callback method that handles the serialization of the value of a <see cref="DataValue{T}"/>
	/// to XML using the specified <see cref="XmlWriter"/> instance.
	/// </summary>
	/// <param name="writer">The <see cref="XmlWriter"/> instance to use for writing.</param>
	/// <param name="value">Object to write to XML.</param>
	/// <exception cref="SerializationException">Serialization failed due to some reason.</exception>
	public delegate void TypeSerializerDelegate(XmlWriter writer, object value);
}
