///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

// ReSharper disable CompareNonConstrainedGenericWithNull
// ReSharper disable InvertIf

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Base class for data manager serializers
/// (provides some functionality common to data manager serializer implementations).
/// </summary>
public abstract class DataManagerSerializerBase : IDataManagerSerializer
{
	/// <summary>
	/// Number of bytes used by the stream buffering file i/o when loading or saving to a file.
	/// </summary>
	internal const int SerializationStreamBufferSize = 80 * 1024; // small enough to be allocated on the regular heap, not on the LOH

	/// <inheritdoc/>
	public virtual T CopySerializableValue<T>(T obj)
	{
		// abort if the object to copy is null
		if (obj == null) return default;

		// return object as is if it is immutable
		Type type = obj.GetType();
		if (Immutability.IsImmutable(type))
			return obj;

		throw new SerializationException($"Copying objects of type '{type.FullName}' is not supported. Consider adding a type serializer for it.");
	}

	/// <inheritdoc/>
	public abstract void Serialize(DataNode node, Stream stream);

	/// <inheritdoc/>
	public abstract DataNode Deserialize(Stream stream, DataTreeManagerHost dataTreeManagerHost = null);

	/// <inheritdoc/>
	public virtual void WriteToFile(DataNode node, string fileName)
	{
		if (node == null) throw new ArgumentNullException(nameof(node));
		if (fileName == null) throw new ArgumentNullException(nameof(fileName));

		// determine the full path of the file and create the directory, if necessary
		string fullPath = Path.GetFullPath(fileName); // throws ArgumentException if the specified file name is invalid
		string directoryPath = Path.GetDirectoryName(fullPath) ?? throw new ArgumentException("The specified file name must not be the root directory.", nameof(fileName));
		Directory.CreateDirectory(directoryPath);

		// serialize the data tree to the file
		using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
		using var stream = new BufferedStream(fileStream, SerializationStreamBufferSize);
		Serialize(node, stream);
	}

	/// <inheritdoc/>
	public virtual DataNode ReadFromFile(string fileName, DataTreeManagerHost dataTreeManagerHost = null)
	{
		if (fileName == null) throw new ArgumentNullException(nameof(fileName));

		// deserialize the data tree from the file
		string fullPath = Path.GetFullPath(fileName); // throws ArgumentException if the specified file name is invalid
		using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		using var stream = new BufferedStream(fileStream, SerializationStreamBufferSize);
		return Deserialize(stream, dataTreeManagerHost);
	}
}
