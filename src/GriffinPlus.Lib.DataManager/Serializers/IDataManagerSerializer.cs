///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Interface a data manager serializer must implement.<br/>
/// The serializer must be implemented thread-safe as multiple threads may use it concurrently
/// or in an interleaved fashion to serialize data trees.
/// </summary>
public interface IDataManagerSerializer
{
	/// <summary>
	/// Copies a serializable object once.
	/// </summary>
	/// <typeparam name="T">Type of the value to copy.</typeparam>
	/// <param name="obj">Object to copy.</param>
	/// <returns>Copy of the specified object.</returns>
	/// <exception cref="SerializationException">Copying object failed. See exception text and inner exception for details.</exception>
	T CopySerializableValue<T>(T obj);

	/// <summary>
	/// Copies a serializable object multiple times.
	/// </summary>
	/// <typeparam name="T">Type of the value to copy.</typeparam>
	/// <param name="obj">Object to copy.</param>
	/// <param name="count">Number of copies to create.</param>
	/// <returns>Copies of the specified object.</returns>
	/// <exception cref="SerializationException">Copying object failed. See exception text and inner exception for details.</exception>
	T[] CopySerializableValue<T>(T obj, int count);

	/// <summary>
	/// Serializes the specified data tree starting with the specified data node to the specified stream.
	/// </summary>
	/// <param name="node">Root node to serialize.</param>
	/// <param name="stream">Stream to write the node to.</param>
	/// <exception cref="ArgumentNullException"><paramref name="node"/> or <paramref name="stream"/> is <c>null</c>.</exception>
	/// <exception cref="SerializationException">Serializing object failed. See exception text and inner exception for details.</exception>
	void Serialize(DataNode node, Stream stream);

	/// <summary>
	/// Deserializes the serialized data tree from the specified stream.
	/// </summary>
	/// <param name="stream">Stream containing the serialized data tree to load.</param>
	/// <param name="dataTreeManagerHost">
	/// Data tree manager host to use (<c>null</c> to use <see cref="DataManager.DefaultDataTreeManagerHost"/>).
	/// </param>
	/// <returns>Root node of the loaded data tree.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
	/// <exception cref="SerializationException">Deserializing object failed. See exception text and inner exception for details.</exception>
	DataNode Deserialize(Stream stream, DataTreeManagerHost dataTreeManagerHost = null);

	/// <summary>
	/// Writes the specified data tree starting with the specified data node to the specified file.
	/// </summary>
	/// <param name="node">Root node to serialize.</param>
	/// <param name="fileName">Name of the file to write the node to.</param>
	/// <exception cref="ArgumentNullException"><paramref name="node"/> or <paramref name="fileName"/> is <c>null</c>.</exception>
	/// <exception cref="SerializationException">Serializing object failed. See exception text and inner exception for details.</exception>
	void WriteToFile(DataNode node, string fileName);

	/// <summary>
	/// Reads the serialized data tree from the specified file.
	/// </summary>
	/// <param name="fileName">Name of the file to read the specified file from.</param>
	/// <param name="dataTreeManagerHost">
	/// Data tree manager host to use (<c>null</c> to use <see cref="DataManager.DefaultDataTreeManagerHost"/>).
	/// </param>
	/// <returns>Root node of the loaded data tree.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <c>null</c>.</exception>
	/// <exception cref="FileNotFoundException"><paramref name="fileName"/> does not exist.</exception>
	/// <exception cref="SerializationException">Deserializing object failed. See exception text and inner exception for details.</exception>
	DataNode ReadFromFile(string fileName, DataTreeManagerHost dataTreeManagerHost = null);
}
