///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.IO;

using GriffinPlus.Lib.Serialization;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// The data manager.
/// </summary>
public sealed class DataManager
{
	#region Definitions

	/// <summary>
	/// Default file name to use when no file name is specified.
	/// </summary>
	public const string DefaultFileName = "DataManager.gpdat";

	/// <summary>
	/// Number of bytes used by the stream buffering file i/o when loading or saving to a file.
	/// (80 kBytes, small enough to be allocated on the regular heap, not on the Large Object Heap).
	/// </summary>
	public const int SerializationStreamBufferSize = 80 * 1024;

	#endregion

	#region Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="DataManager"/> class.
	/// </summary>
	/// <param name="root">Root node of the data tree to use.</param>
	/// <param name="filename">Path to the loaded file (is used for saving).</param>
	private DataManager(DataNode root, string filename)
	{
		Debug.Assert(root != null);
		Debug.Assert(filename != null);

		RootNode = root;
		FileName = filename;
	}

	#endregion

	#region Default Instance

	private static volatile DataManager sDefaultInstance;
	private static readonly object      sDefaultInstanceLock = new();

	/// <summary>
	/// Gets the default data manager instance
	/// (<c>null</c> if the data manager is not initialized).
	/// </summary>
	/// <exception cref="InvalidOperationException">The data manager is already initialized.</exception>
	/// <remarks>
	/// The default data manager uses the default persistence strategy as specified by the <see cref="DefaultSerializer"/> property.
	/// </remarks>
	public static DataManager Default
	{
		get
		{
			lock (sDefaultInstanceLock)
			{
				if (sDefaultInstance == null) throw new InvalidOperationException("The data manager is not initialized.");
				return sDefaultInstance;
			}
		}
	}

	/// <summary>
	/// Gets a value indicating whether the data manager subsystem is initialized.
	/// </summary>
	public static bool IsInitialized
	{
		get
		{
			lock (sDefaultInstanceLock)
			{
				return sDefaultInstance != null;
			}
		}
	}

	/// <summary>
	/// Initializes the data manager loading the default data file (DataManager.gpdat).
	/// </summary>
	/// <param name="dataTreeManagerHost">
	/// Data tree manager host to use
	/// (<c>null</c> to use the default host specified by <see cref="DataTreeManagerHost.Default"/>).
	/// </param>
	/// <param name="serializer">
	/// Serializer to use for serializing, deserializing and copying data in the data tree
	/// (<c>null</c> to use the default serializer specified by <see cref="DefaultSerializer"/>).
	/// </param>
	/// <exception cref="InvalidOperationException">The data manager is already initialized.</exception>
	/// <exception cref="Exception">Loading the data tree failed due to some reason.</exception>
	public static void Init(DataTreeManagerHost dataTreeManagerHost = null, IDataManagerSerializer serializer = null)
	{
		Init(DefaultFileName, dataTreeManagerHost, serializer);
	}

	/// <summary>
	/// Initializes the data manager loading the specified data file.
	/// </summary>
	/// <param name="filename">File to load initially.</param>
	/// <param name="dataTreeManagerHost">
	/// Data tree manager host to use
	/// (<c>null</c> to use the default host specified by <see cref="DataTreeManagerHost.Default"/>).
	/// </param>
	/// <param name="serializer">
	/// Serializer to use for serializing, deserializing and copying data in the data tree
	/// (<c>null</c> to use the default serializer specified by <see cref="DefaultSerializer"/>).
	/// </param>
	/// <exception cref="InvalidOperationException">The data manager is already initialized.</exception>
	/// <exception cref="Exception">Loading the data tree failed due to some reason.</exception>
	public static void Init(
		string                 filename,
		DataTreeManagerHost    dataTreeManagerHost = null,
		IDataManagerSerializer serializer          = null)
	{
		Init(filename, filename, dataTreeManagerHost, serializer);
	}

	/// <summary>
	/// Initializes the data manager loading the specified data file, but sets some other file for saving.
	/// </summary>
	/// <param name="fileToLoad">File to load initially.</param>
	/// <param name="fileToSave">File to save to (when saving is triggered).</param>
	/// <param name="dataTreeManagerHost">
	/// Data tree manager host to use
	/// (<c>null</c> to use the default host specified by <see cref="DataTreeManagerHost.Default"/>).
	/// </param>
	/// <param name="serializer">
	/// Serializer to use for serializing, deserializing and copying data in the data tree
	/// (<c>null</c> to use the default serializer specified by <see cref="DefaultSerializer"/>).
	/// </param>
	/// <exception cref="InvalidOperationException">The data manager is already initialized.</exception>
	/// <exception cref="Exception">Loading the data tree failed due to some reason.</exception>
	public static void Init(
		string                 fileToLoad,
		string                 fileToSave,
		DataTreeManagerHost    dataTreeManagerHost = null,
		IDataManagerSerializer serializer          = null)
	{
		dataTreeManagerHost ??= DataTreeManagerHost.Default;
		serializer ??= DefaultSerializer;

		lock (sDefaultInstanceLock)
		{
			if (sDefaultInstance != null)
				throw new InvalidOperationException("The data manager is already initialized.");

			DataNode root;

			try
			{
				root = serializer.ReadFromFile(fileToLoad, dataTreeManagerHost);
			}
			catch (FileNotFoundException)
			{
				// file does not exist
				// => create an empty data tree
				root = new DataNode(
					"Data Manager",
					DataNodeProperties.Persistent,
					dataTreeManagerHost,
					serializer);
			}

			// create data manager
			sDefaultInstance = new DataManager(root, fileToSave);
		}
	}

	/// <summary>
	/// Triggers shutting down the data manager down and returns immediately.
	/// Pending operations will terminate on their own as soon as they are finished.
	/// </summary>
	public static void Shutdown()
	{
		lock (sDefaultInstanceLock)
		{
			// just reset the strong reference to the default instance
			// (pending operations will stop after the object is garbage collected)
			sDefaultInstance = null;
		}
	}

	#endregion

	#region Changing the Default Serializer

	/// <summary>
	/// Gets or sets the default serializer to use for serializing, deserializing and copying data in the data tree.
	/// </summary>
	public static IDataManagerSerializer DefaultSerializer { get; set; } = new DataManagerDefaultSerializer(SerializationOptimization.Speed, true);

	/// <summary>
	/// Checks whether the specified type is a valid serializer (throws an exception, if the type is not a valid serializer type).
	/// </summary>
	/// <param name="type">Type to check</param>
	/// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentException"><paramref name="type"/> is not a class, abstract or does not implement <see cref="IDataManagerSerializer"/>.</exception>
	internal static void CheckSerializerType(Type type)
	{
		if (type == null) throw new ArgumentNullException(nameof(type));
		if (!type.IsClass) throw new ArgumentException("The specified type must be a class.");
		if (type.IsAbstract) throw new ArgumentException("The specified type must not be an abstract class.");
		if (!typeof(IDataManagerSerializer).IsAssignableFrom(type))
			throw new ArgumentException($"The specified type must implement {nameof(IDataManagerSerializer)}.");
	}

	#endregion

	#region Properties

	/// <summary>
	/// Gets the root node of the data manager.
	/// </summary>
	public DataNode RootNode { get; }

	/// <summary>
	/// Gets the name of the file that is used when saving.
	/// </summary>
	public string FileName { get; }

	#endregion

	#region Saving

	/// <summary>
	/// Saves the entire data manager tree to disk using the filename specified at construction time.<br/>
	/// Also see:<br/>
	/// - <see cref="Init(DataTreeManagerHost,IDataManagerSerializer)"/><br/>
	/// - <see cref="Init(string,DataTreeManagerHost,IDataManagerSerializer)"/><br/>
	/// - <see cref="Init(string,string,DataTreeManagerHost,IDataManagerSerializer)"/>
	/// </summary>
	public void Save()
	{
		RootNode.Save(FileName);
	}

	/// <summary>
	/// Saves the entire data tree to disk.
	/// </summary>
	/// <param name="filename">Name of the file to save the data tree to.</param>
	public void Save(string filename)
	{
		RootNode.Save(filename);
	}

	#endregion
}
