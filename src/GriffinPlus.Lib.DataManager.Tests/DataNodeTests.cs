///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using GriffinPlus.Lib.DataManager.Viewer;
using GriffinPlus.Lib.Serialization;

using Xunit;

#pragma warning disable IDE0090 // Use 'new(...)'

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Tests targeting the <see cref="DataNode"/> class.
/// </summary>
public class DataNodeTests
{
	#region Generic Test Data

	public static IEnumerable<DataNodeProperties> InvalidDataNodePropertyFlags
	{
		get
		{
			unchecked
			{
				// yield return (DataNodeProperties)0x00000000; // None
				// yield return DataNodeProperties)0x00000001; // Persistent
				yield return (DataNodeProperties)0x00000002; // <Invalid>
				yield return (DataNodeProperties)0x00000004; // <Invalid>
				yield return (DataNodeProperties)0x00000008; // <Invalid>
				yield return (DataNodeProperties)0x00000010; // <Invalid>
				yield return (DataNodeProperties)0x00000020; // <Invalid>
				yield return (DataNodeProperties)0x00000040; // <Invalid>
				yield return (DataNodeProperties)0x00000080; // <Invalid>
				yield return (DataNodeProperties)0x00000100; // <Invalid>
				yield return (DataNodeProperties)0x00000200; // <Invalid>
				yield return (DataNodeProperties)0x00000400; // <Invalid>
				yield return (DataNodeProperties)0x00000800; // <Invalid>
				yield return (DataNodeProperties)0x00001000; // <Invalid>
				yield return (DataNodeProperties)0x00002000; // <Invalid>
				yield return (DataNodeProperties)0x00004000; // <Invalid>
				yield return (DataNodeProperties)0x00008000; // <Invalid>
				yield return (DataNodeProperties)0x00010000; // <Invalid>
				yield return (DataNodeProperties)0x00020000; // <Invalid>
				yield return (DataNodeProperties)0x00040000; // <Invalid>
				yield return (DataNodeProperties)0x00080000; // <Invalid>
				yield return (DataNodeProperties)0x00100000; // <Invalid>
				yield return (DataNodeProperties)0x00200000; // <Invalid>
				yield return (DataNodeProperties)0x00400000; // <Invalid>
				yield return (DataNodeProperties)0x00800000; // <Invalid>
				yield return (DataNodeProperties)0x01000000; // <Invalid>
				yield return (DataNodeProperties)0x02000000; // <Invalid>
				yield return (DataNodeProperties)0x04000000; // <Invalid>
				yield return (DataNodeProperties)0x08000000; // <Invalid>
				yield return (DataNodeProperties)0x10000000; // <Invalid>
				yield return (DataNodeProperties)0x20000000; // <Invalid>
				yield return (DataNodeProperties)0x40000000; // <Invalid>
				yield return (DataNodeProperties)0x80000000; // <Invalid>
			}
		}
	}

	#endregion

	#region DataNode(string name, DataNodeProperties properties, DataTreeManagerHost dataTreeManagerHost, IDataManagerSerializer serializer)

	/// <summary>
	/// Test data for the <see cref="Construct_RootNode"/> test method.
	/// </summary>
	public static IEnumerable<object[]> Construct_TestData
	{
		get
		{
			const string name = "Root Node";
			DataTreeManagerHost host = new("Custom Data Tree Manager Host");
			DataManagerDefaultSerializer serializer = new(SerializationOptimization.Speed, true);

			yield return [name, DataNodeProperties.None, null, null];
			yield return [name, DataNodeProperties.None, host, null];
			yield return [name, DataNodeProperties.None, null, serializer];
			yield return [name, DataNodeProperties.None, host, serializer];
		}
	}

	/// <summary>
	/// Tests whether the <see cref="DataNode(string, DataNodeProperties, DataTreeManagerHost, IDataManagerSerializer)"/>
	/// constructor works as expected.
	/// </summary>
	[Theory]
	[MemberData(nameof(Construct_TestData))]
	public void Construct_RootNode(
		string                 name,
		DataNodeProperties     properties,
		DataTreeManagerHost    host,
		IDataManagerSerializer serializer)
	{
		// create a root node
		var node = new DataNode(name, properties, host, serializer);

		// check whether the data tree manager host is set as expected
		// ('null' lets the node use the default data tree manager host provided by the DataManager class)
		DataTreeManagerHost expectedHost = host ?? DataManager.DefaultDataTreeManagerHost;
		Assert.Same(expectedHost, node.DataTreeManager.Host);

		// check whether the serializer is set as expected
		// ('null' lets the node use the default serializer provided by the DataManager class)
		IDataManagerSerializer expectedSerializer = serializer ?? DataManager.DefaultSerializer;
		Assert.Same(expectedSerializer, node.DataTreeManager.Serializer);

		// a root node should neither have a parent node nor a name
		Assert.Null(node.Parent);
		Assert.Equal(name, node.Name);
		Assert.Equal("/", node.Path);

		// check node properties
		Assert.Equal(properties, node.Properties);
		Assert.Equal(properties.HasFlag(DataNodeProperties.Persistent), node.IsPersistent);

		// the node should have neither child nodes nor values
		Assert.Empty(node.Children);
		Assert.Empty(node.Values);

		// a viewer node should reflect pretty the same as the node itself
		ViewerDataNode viewerWrapper = node.ViewerWrapper; // creates the viewer wrapper on demand
		Assert.NotNull(viewerWrapper);
		Assert.Same(viewerWrapper, node.ViewerWrapper); // calling twice should return the same instance
		Assert.Null(viewerWrapper.Parent);
		Assert.Equal(name, viewerWrapper.Name);
		Assert.Equal("/", viewerWrapper.Path);
		Assert.Equal(properties, viewerWrapper.Properties);
		Assert.Equal(properties.HasFlag(DataNodeProperties.Persistent), viewerWrapper.IsPersistent);
		Assert.Empty(viewerWrapper.Children);
		Assert.Empty(viewerWrapper.Values);
	}

	/// <summary>
	/// Tests whether the <see cref="DataNode(string, DataNodeProperties, DataTreeManagerHost, IDataManagerSerializer)"/>
	/// constructor throws an <see cref="ArgumentNullException"/> if the specified node name is a null reference.
	/// </summary>
	[Fact]
	public void Construct_RootNode_NameIsNull()
	{
		var exception = Assert.Throws<ArgumentNullException>(
			() => new DataNode(
				null,
				DataNodeProperties.None,
				null,
				null));

		Assert.Equal("name", exception.ParamName);
	}

	/// <summary>
	/// Tests whether the <see cref="DataNode(string, DataNodeProperties, DataTreeManagerHost, IDataManagerSerializer)"/>
	/// constructor throws an <see cref="ArgumentException"/> if the specified node name is invalid. The constructor
	/// uses the <see cref="PathHelpers.IsValidName"/> method to check the name, so this test case does _not_ cover
	/// all possible invalid names.
	/// </summary>
	[Theory]
	[InlineData("")]      // empty name
	[InlineData(" Node")] // leading whitespace
	[InlineData("Node ")] // trailing whitespace
	public void Construct_RootNode_NameIsInvalid(string name)
	{
		var exception = Assert.Throws<ArgumentException>(
			() => new DataNode(
				name,
				DataNodeProperties.None,
				null,
				null));

		Assert.Equal("name", exception.ParamName);
	}

	/// <summary>
	/// Tests whether the <see cref="DataNode(string, DataNodeProperties, DataTreeManagerHost, IDataManagerSerializer)"/>
	/// constructor throws an <see cref="ArgumentException"/> if the specified node properties contain invalid flags.
	/// </summary>
	[Fact]
	public void Construct_RootNode_PropertiesContainsInvalidFlags()
	{
		foreach (DataNodeProperties properties in InvalidDataNodePropertyFlags)
		{
			var exception = Assert.Throws<ArgumentException>(
				() => new DataNode(
					"Root Node",
					properties,
					null,
					null));

			Assert.Equal("properties", exception.ParamName);
		}
	}

	#endregion

	#region DataNode(string name, DataNodeProperties properties, DataTreeManagerHost dataTreeManagerHost, IDataManagerSerializer serializer)

	/// <summary>
	/// Tests whether the <see cref="DataNode(DataNode, ReadOnlySpan{char}, DataNodePropertiesInternal)"/>
	/// constructor works as expected.
	/// </summary>
	[Fact]
	public void Construct_ChildNode()
	{
		// prepare a root node that will become the parent of the child node
		DataTreeManagerHost host = new();
		DataManagerDefaultSerializer serializer = new(SerializationOptimization.Speed, true);
		var root = new DataNode("Root", DataNodeProperties.None, host, serializer);

		// create a child node
		// (the constructor requires the tree synchronization object to be locked)
		lock (root.DataTreeManager.Sync)
		{
			const string childName = "Child";
			const DataNodePropertiesInternal properties = DataNodePropertiesInternal.Persistent;
			var child = new DataNode(root, childName.AsSpan(), properties);
			Assert.Same(root.DataTreeManager, child.DataTreeManager);
			Assert.Same(root, child.Parent);
			Assert.Equal(childName, child.Name);
			Assert.Equal((DataNodeProperties)properties, child.Properties);
			Assert.Equal("/Child", child.Path);
			Assert.Empty(child.Children);
			Assert.Empty(child.Values);
		}
	}

	#endregion

	#region DataNode(DeserializationArchive archive) / object IInternalObjectSerializer.Serialize(SerializerArchive archive)

	/// <summary>
	/// Tests the implementation of the <see cref="IInternalObjectSerializer.Serialize(SerializationArchive)"/>
	/// method and the <see cref="DataNode(DeserializationArchive)"/> constructor.
	/// </summary>
	[Fact]
	public void SerializeAndDeserialize()
	{
		TestSerializationAndDeserialization(
			new DataTreeManagerHost(),                                               // data tree manager host of the original data tree
			new DataManagerDefaultSerializer(SerializationOptimization.Speed, true), // serializer of the original data tree
			new DataTreeManagerHost(),                                               // data tree manager host of the copied data tree
			new DataManagerDefaultSerializer(SerializationOptimization.Speed, true), // serializer of the copied data tree
			(node, host, serializer) =>
			{
				using MemoryStream stream = new();
				Serializer.Serialize(stream, node, null, SerializationOptimization.Speed);
				stream.Position = 0;
				var context = new DataDeserializationContext(null, host, serializer);
				return (DataNode)Serializer.Deserialize(stream, context, true);
			});
	}

	/// <summary>
	/// Tests whether the <see cref="DataNode(DeserializationArchive)"/> constructor throws a
	/// <see cref="SerializationException"/> if the deserialization context is <c>null</c> or not a
	/// <see cref="DataDeserializationContext"/> object.
	/// </summary>
	[Theory]
	[InlineData(null)]
	[InlineData(42)]
	public void Deserialize_ContextIsInvalid(object context)
	{
		using MemoryStream stream = new();
		var root = new DataNode("Root", DataNodeProperties.Persistent);
		Serializer.Serialize(stream, root, null, SerializationOptimization.Speed);
		stream.Position = 0;
		var exception = Assert.Throws<SerializationException>(() => Serializer.Deserialize(stream, context, true));
		Assert.Equal($"The serialization archive does not contain a {nameof(DataDeserializationContext)}.", exception.Message);
	}

	/// <summary>
	/// Tests whether the implementation of the <see cref="IInternalObjectSerializer.Serialize"/> method
	/// throws a <see cref="VersionNotSupportedException"/> if the requested serializer version is not supported.
	/// </summary>
	[Fact]
	public void Serialize_VersionNotSupported()
	{
		using MemoryStream stream = new();
		var root = new DataNode("Root", DataNodeProperties.Persistent);
		var versions = new SerializerVersionTable();
		versions.Set(typeof(DataNode), 2); // DataNode supports version 1 only
		var serializer = new Serializer(versions);
		var exception = Assert.Throws<VersionNotSupportedException>(() => serializer.Serialize(stream, root, null));
		Assert.Equal(1u, exception.MaxVersion);
	}

	/// <summary>
	/// Sets up a small data tree consisting of a root node with a persistent and a transient child node.
	/// This tests far more than the <see cref="DataNode"/> class itself, but serializing child nodes can only
	/// be tested in conjunction with a parent node.
	/// </summary>
	/// <param name="host1">Data tree manager host of the original data tree.</param>
	/// <param name="serializer1">Serializer to use within the original data tree.</param>
	/// <param name="host2">Data tree manager host of the copied data tree.</param>
	/// <param name="serializer2">Serializer to use within the copied data tree (must be of the same type as <paramref name="serializer1"/>).</param>
	/// <param name="copy">Function that copies the original data tree.</param>
	internal static void TestSerializationAndDeserialization(
		DataTreeManagerHost                                                   host1,
		IDataManagerSerializer                                                serializer1,
		DataTreeManagerHost                                                   host2,
		IDataManagerSerializer                                                serializer2,
		Func<DataNode, DataTreeManagerHost, IDataManagerSerializer, DataNode> copy)
	{
		// root node
		const string expectedRootName = "Root";
		const DataNodeProperties expectedRootProperties = DataNodeProperties.Persistent;

		// child node 1
		// - persistent, should be serialized
		// - name contains a character to escape in the path
		const string expectedChild1Name = "Persistent Child with Character ('/') to Escape";
		const string expectedChild1Path = @"/Persistent Child with Character ('\/') to Escape";
		const DataNodeProperties expectedChild1Properties = DataNodeProperties.Persistent; // should be serialized

		// child node 2
		// - not persistent, should not be serialized
		const string expectedChild2Name = "Transient Child"; // the slash should be escaped properly in the path
		const string expectedChild2Path = "/Transient Child";
		const DataNodeProperties expectedChild2Properties = DataNodeProperties.None; // should not be serialized

		// prepare a small data tree with (1) a persistent child node and (2) a transient node
		var root1 = new DataNode(expectedRootName, expectedRootProperties, host1, serializer1);
		DataNode child11 = root1.Children.Add(expectedChild1Name, expectedChild1Properties);
		Assert.Equal(expectedChild1Path, child11.Path);
		DataNode child12 = root1.Children.Add(expectedChild2Name, expectedChild2Properties);
		Assert.Equal(expectedChild2Path, child12.Path);

		// serialize and deserialize the data tree
		// using the specified copy delegate
		DataNode root2 = copy(root1, host2, serializer2);
		Assert.NotNull(root2);

		// the data trees should use the expected data tree managers, data tree manager hosts and serializers
		Assert.Same(host2, root2.DataTreeManager.Host);
		Assert.Same(serializer2, root2.DataTreeManager.Serializer);

		// the copied data tree should not use the same data tree manager as the original data tree
		Assert.NotSame(root1.DataTreeManager, root2.DataTreeManager);

		// check whether the deserialized data tree is as expected

		// root node
		Assert.Null(root2.Parent);
		Assert.Equal(expectedRootProperties, root2.Properties);
		Assert.Equal(expectedRootName, root2.Name);
		Assert.Equal("/", root2.Path);
		Assert.Equal(1, root2.Children.Count);
		Assert.Empty(root2.Values);

		// child node 1 (was persistent and therefore serialized)
		DataNode child21 = root2.Children.GetByName(expectedChild1Name);
		Assert.NotNull(child21);
		Assert.Same(root2.DataTreeManager, child21.DataTreeManager);
		Assert.Same(root2, child21.Parent);
		Assert.Equal(expectedChild1Properties, child21.Properties);
		Assert.Equal(expectedChild1Name, child21.Name);
		Assert.Equal(expectedChild1Path, child21.Path);
		Assert.Empty(child21.Children);
		Assert.Empty(child21.Values);

		// child node 2 (was not persistent and therefore not serialized)
		DataNode child22 = root2.Children.GetByName(expectedChild2Name);
		Assert.Null(child22);
	}

	#endregion

	#region void ToStream(Stream stream) / DataNode FromStream(Stream stream)

	/// <summary>
	/// Tests whether the <see cref="DataNode.ToStream"/> method followed by the <see cref="DataNode.FromStream"/> method
	/// creates a deep copy of the data tree using the default serialization mechanism.
	/// </summary>
	[Fact]
	public void ToStream_and_FromStream()
	{
		TestSerializationAndDeserialization(
			new DataTreeManagerHost(),                                               // data tree manager host of the original data tree
			new DataManagerDefaultSerializer(SerializationOptimization.Speed, true), // serializer of the original data tree
			new DataTreeManagerHost(),                                               // data tree manager host of the copied data tree
			new DataManagerDefaultSerializer(SerializationOptimization.Speed, true), // serializer of the copied data tree
			(node, host, serializer) =>
			{
				using MemoryStream stream = new();
				node.ToStream(stream);
				stream.Position = 0;
				return DataNode.FromStream(stream, host, serializer);
			});
	}

	/// <summary>
	/// Tests whether the <see cref="DataNode.ToStream"/> method throws an <see cref="ArgumentNullException"/> if
	/// the specified stream is <c>null</c>.
	/// </summary>
	[Fact]
	public void ToStream_StreamIsNull()
	{
		var node = new DataNode("Root", DataNodeProperties.Persistent);
		var exception = Assert.Throws<ArgumentNullException>(() => node.ToStream(null));
		Assert.Equal("stream", exception.ParamName);
	}

	/// <summary>
	/// Tests whether the <see cref="DataNode.FromStream"/> method throws an <see cref="ArgumentNullException"/> if
	/// the specified stream is <c>null</c>.
	/// </summary>
	[Fact]
	public void FromStream_StreamIsNull()
	{
		var exception = Assert.Throws<ArgumentNullException>(() => DataNode.FromStream(null));
		Assert.Equal("stream", exception.ParamName);
	}

	#endregion

	#region Name

	/// <summary>
	/// Tests getting the <see cref="DataNode.Name"/> property.
	/// </summary>
	[Fact]
	public void Name_Get()
	{
		// create an expected tree to work with
		ExpectedDataNode expectedDataTree = ExpectedDataTree.Create();

		// get the Name property and compare it to the expected name
		expectedDataTree.RunTest(
			expectedRootNode =>
			{
				Assert.Equal(expectedRootNode.Name, expectedRootNode.ActualDataNode.Name);
				// other nodes are also checked as part of the consistency check in the RunTest() method
			});
	}

	/// <summary>
	/// Tests setting the <see cref="DataNode.Name"/> property.
	/// </summary>
	[Fact]
	public void Name_Set()
	{
		// rename the root node to "Renamed Root" and check that paths of child nodes and values
		// do not change (the name of the root node does not have any effect on paths)
		ExpectedDataTree.Create()
			.RunTest(
				expectedRootNode =>
				{
					expectedRootNode.GetNode("/").ActualDataNode.Name = "Renamed Root";
					expectedRootNode.GetNode("/").Name = "Renamed Root";
				});

		// rename the node at path "/Child #1" to "Renamed Child #1"
		// => all paths below should be adjusted accordingly to reflect the name change
		ExpectedDataTree.Create()
			.RunTest(
				expectedRootNode =>
				{
					expectedRootNode.GetNode("/Child #1").ActualDataNode.Name = "Renamed Child #1";
					expectedRootNode.GetNode("/Child #1").Name = "Renamed Child #1";
				});
	}

	/// <summary>
	/// Tests whether setting the <see cref="DataNode.Name"/> property to <c>null</c> throws an
	/// <see cref="ArgumentNullException"/>.
	/// </summary>
	[Fact]
	public void Name_Set_ValueIsNull()
	{
		ExpectedDataNode expectedDataTree = ExpectedDataTree.Create();

		expectedDataTree.RunTest(
			expectedRootNode =>
			{
				// setting the name of the root node to null should throw an ArgumentNullException
				Assert.Throws<ArgumentNullException>(() => { expectedRootNode.ActualDataNode.Name = null; });
			});
	}

	/// <summary>
	/// Tests whether setting the <see cref="DataNode.Name"/> property to an invalid name throws an
	/// <see cref="ArgumentException"/>. The property uses <see cref="PathHelpers.IsValidName"/> to validate the name,
	/// so this test case does not test all possible wrong names which is already excessively done by the test for
	/// <see cref="PathHelpers.IsValidName"/>.
	/// </summary>
	[Fact]
	public void Name_Set_ValueIsInvalidName()
	{
		ExpectedDataNode expectedDataTree = ExpectedDataTree.Create();
		expectedDataTree.RunTest(
			expectedRootNode =>
			{
				// setting the name of the node to an invalid name should throw an ArgumentException
				// (trailing whitespaces are not allowed)
				var exception = Assert.Throws<ArgumentException>(() => { expectedRootNode.ActualDataNode.Name = "Root "; });
				Assert.Equal("value", exception.ParamName);
				Assert.StartsWith("The specified name (Root ) does not comply with the rules for node names.", exception.Message);
			});
	}

	/// <summary>
	/// Tests whether setting <see cref="DataNode.Name"/> to the same value does not modify the node and raise any event.
	/// </summary>
	[Fact]
	public void Name_Set_ValueIsTheSame()
	{
		ExpectedDataNode expectedDataTree = ExpectedDataTree.Create();

		expectedDataTree.RunTest(
			expectedRootNode =>
			{
				string name = expectedRootNode.ActualDataNode.Name;
				expectedRootNode.ActualDataNode.Name = name;
			});
	}

	/// <summary>
	/// Tests whether setting <see cref="DataNode.Name"/> to a name that is also used by some other node in the same
	/// collection throws a <see cref="DataNodeExistsAlreadyException"/>.
	/// </summary>
	[Fact]
	public void Name_Set_NameIsAlreadyPresent()
	{
		ExpectedDataNode expectedDataTree = ExpectedDataTree.Create();

		expectedDataTree.RunTest(
			expectedRootNode =>
			{
				// get the two child nodes of the root node
				DataNode actualChild1 = expectedRootNode.GetNode("/Child #1").ActualDataNode;
				DataNode actualChild2 = expectedRootNode.GetNode("/Child #2").ActualDataNode;
				Assert.NotNull(actualChild1);
				Assert.NotNull(actualChild2);

				// change the name of the second child node from "Child #2" to "Child #1" to trigger the conflict
				Assert.Throws<DataNodeExistsAlreadyException>(() => actualChild2.Name = "Child #1");
			});
	}

	#endregion

	#region Properties

	/// <summary>
	/// Tests getting the <see cref="DataNode.Properties"/> property.
	/// </summary>
	[Theory]
	[MemberData(nameof(ExpectedDataTree.AllNodePaths), MemberType = typeof(ExpectedDataTree))]
	public void Properties_Get(string path)
	{
		ExpectedDataNode expectedDataTree = ExpectedDataTree.Create();

		expectedDataTree.RunTest(
			expectedRootNode =>
			{
				ExpectedDataNode expectedNode = expectedRootNode.GetNode(path);
				Assert.NotNull(expectedNode);
				DataNode actualNode = expectedNode.ActualDataNode;
				Assert.Equal(expectedNode.Properties, actualNode.Properties);
			});
	}

	/// <summary>
	/// Tests setting the <see cref="DataNodeProperties.Persistent"/> flag via the <see cref="DataNode.Properties"/> property.
	/// This should set the flag on all other nodes up to the root node as well.
	/// </summary>
	[Theory]
	[MemberData(nameof(ExpectedDataTree.AllNodePaths), MemberType = typeof(ExpectedDataTree))]
	public void Properties_Set_SetPersistent(string path)
	{
		ExpectedDataNode expectedDataTree = ExpectedDataTree.Create();

		expectedDataTree.RunTest(
			expectedRootNode =>
			{
				ExpectedDataNode expectedNode = expectedRootNode.GetNode(path);
				Assert.NotNull(expectedNode);
				DataNode actualNode = expectedNode.ActualDataNode;
				expectedNode.Properties |= DataNodeProperties.Persistent;
				actualNode.Properties |= DataNodeProperties.Persistent;
			});
	}

	/// <summary>
	/// Tests clearing the <see cref="DataNodeProperties.Persistent"/> flag via the <see cref="DataNode.Properties"/> property.
	/// This should affect the node itself only.
	/// </summary>
	[Theory]
	[MemberData(nameof(ExpectedDataTree.AllNodePaths), MemberType = typeof(ExpectedDataTree))]
	public void Properties_Set_ClearPersistent(string path)
	{
		ExpectedDataNode expectedDataTree = ExpectedDataTree.Create();

		expectedDataTree.RunTest(
			expectedRootNode =>
			{
				ExpectedDataNode expectedNode = expectedRootNode.GetNode(path);
				Assert.NotNull(expectedNode);
				expectedNode.Properties &= ~DataNodeProperties.Persistent;
				expectedNode.ActualDataNode.Properties &= ~DataNodeProperties.Persistent;
			});
	}

	/// <summary>
	/// Tests whether setting the <see cref="DataNode.Properties"/> property with invalid flags throws an
	/// <see cref="ArgumentException"/>.
	/// </summary>
	[Fact]
	public void Properties_Set_InvalidFlags()
	{
		ExpectedDataNode expectedDataTree = ExpectedDataTree.Create();

		expectedDataTree.RunTest(
			expectedRootNode =>
			{
				DataNode actualRootNode = expectedRootNode.ActualDataNode;

				int flags = 1;
				while (flags != 0)
				{
					if (((DataNodeProperties)flags & ~DataNodeProperties.All) != 0)
					{
						// the flag is invalid
						// => test whether the property recognises it as invalid
						DataNodeProperties properties = DataNodeProperties.All | (DataNodeProperties)flags; // all valid properties + 1 invalid flag
						var exception = Assert.Throws<ArgumentException>(
							() =>
							{
								return actualRootNode.Properties = properties;
							});

						Assert.Equal("value", exception.ParamName);
						Assert.StartsWith($"The specified properties (0x{properties:X}) contain unsupported flags (0x{properties & ~DataNodeProperties.All:X}).", exception.Message);
					}

					// proceed with the next flag...
					flags <<= 1;
				}
			});
	}

	#endregion

	#region IsPersistent

	/// <summary>
	/// Tests getting the <see cref="DataNode.IsPersistent"/> property.
	/// </summary>
	[Theory]
	[MemberData(nameof(ExpectedDataTree.AllNodePaths), MemberType = typeof(ExpectedDataTree))]
	public void IsPersistent_Get(string path)
	{
		ExpectedDataTree.Create()
			.RunTest(
				expectedRootNode =>
				{
					ExpectedDataNode expectedNode = expectedRootNode.GetNode(path);
					Assert.NotNull(expectedNode);
					bool isPersistent = (expectedRootNode.Properties & DataNodeProperties.Persistent) != 0;
					Assert.Equal(isPersistent, expectedRootNode.ActualDataNode.IsPersistent);
				});
	}

	/// <summary>
	/// Tests setting the <see cref="DataNode.IsPersistent"/> property to <c>false</c>.
	/// </summary>
	[Theory]
	[MemberData(nameof(ExpectedDataTree.AllNodePaths), MemberType = typeof(ExpectedDataTree))]
	public void IsPersistent_Set_SetToTrue(string path)
	{
		ExpectedDataTree.Create()
			.RunTest(
				expectedRootNode =>
				{
					ExpectedDataNode expectedNode = expectedRootNode.GetNode(path);
					Assert.NotNull(expectedNode);
					expectedNode.ActualDataNode.IsPersistent = true;
					expectedNode.IsPersistent = true;
				});
	}

	/// <summary>
	/// Tests setting the <see cref="DataNode.IsPersistent"/> property to <c>false</c>.
	/// </summary>
	[Theory]
	[MemberData(nameof(ExpectedDataTree.AllNodePaths), MemberType = typeof(ExpectedDataTree))]
	public void IsPersistent_Set_SetToFalse(string path)
	{
		ExpectedDataTree.Create()
			.RunTest(
				expectedRootNode =>
				{
					ExpectedDataNode expectedNode = expectedRootNode.GetNode(path);
					Assert.NotNull(expectedNode);
					expectedNode.ActualDataNode.IsPersistent = false;
					expectedNode.IsPersistent = false;
				});
	}

	#endregion
}
