///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager;

partial class DataManagerXmlSerializer
{
	/// <summary>
	/// A callback method that handles copying the value of a <see cref="DataValue{T}"/>.
	/// </summary>
	/// <param name="obj">Object to copy.</param>
	/// <exception cref="SerializationException">Copying failed due to some reason.</exception>
	public delegate object TypeCopierDelegate(object obj);
}
