using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Provides an example expected data tree used by tests.
/// </summary>
class ExpectedDataTree
{
	/// <summary>
	/// Creates a rudimentary expected data tree with the following structure:
	/// <code>
	/// Path: /                   (Node,  Properties: Persistent                        )
	/// Path: /Child #1           (Node,  Properties: Persistent                        )
	/// Path: /Child #1/Child #1  (Node,  Properties: Persistent                        )
	/// Path: /Child #1/Child #2  (Node,  Properties: None                              )
	/// Path: /Child #1/Value #1  (Value, Properties: Persistent, Type: Int32, Value: 1 )
	/// Path: /Child #1/Value #2  (Value, Properties: None,       Type: Int32, Value: 2 )
	/// Path: /Child #2           (Node,  Properties: None                              )
	/// Path: /Child #2/Child #1  (Node,  Properties: None                              )
	/// Path: /Child #2/Child #2  (Node,  Properties: None                              )
	/// Path: /Child #2/Value #1  (Value, Properties: None,       Type: Int32, Value: 3 )
	/// Path: /Child #2/Value #2  (Value, Properties: None,       Type: Int32, Value: 4 )
	/// Path: /Value #1           (Value, Properties: Persistent, Type: Int32, Value: 5 )
	/// Path: /Value #2           (Value, Properties: None,       Type: Int32, Value: 6 )
	/// </code>
	/// </summary>
	/// <returns>The expected data tree.</returns>
	public static ExpectedDataNode Create()
	{
		return new ExpectedDataNode("Root", DataNodePropertiesInternal.Persistent)
		{
			Children =
			[
				new ExpectedDataNode("Child #1", DataNodePropertiesInternal.Persistent)
				{
					Children =
					[
						new ExpectedDataNode("Child #1", DataNodePropertiesInternal.Persistent),
						new ExpectedDataNode("Child #2", DataNodePropertiesInternal.None)
					],
					Values =
					[
						new ExpectedDataValue<int>("Value #1", DataValuePropertiesInternal.Persistent, 1),
						new ExpectedDataValue<int>("Value #2", DataValuePropertiesInternal.None, 2)
					]
				},
				new ExpectedDataNode("Child #2", DataNodePropertiesInternal.None)
				{
					Children =
					[
						new ExpectedDataNode("Child #1", DataNodePropertiesInternal.None),
						new ExpectedDataNode("Child #2", DataNodePropertiesInternal.None)
					],
					Values =
					[
						new ExpectedDataValue<int>("Value #1", DataValuePropertiesInternal.None, 3),
						new ExpectedDataValue<int>("Value #2", DataValuePropertiesInternal.None, 4)
					]
				}
			],
			Values =
			[
				new ExpectedDataValue<int>("Value #1", DataValuePropertiesInternal.Persistent, 5),
				new ExpectedDataValue<int>("Value #2", DataValuePropertiesInternal.None, 6)
			]
		};
	}

	/// <summary>
	/// Gets the paths of all nodes in the expected data tree created by the <see cref="Create"/> method.
	/// </summary>
	public static TheoryData<string> AllNodePaths
	{
		get
		{
			ExpectedDataNode expectedRootNode = Create();
			var nodes = new List<ExpectedDataNode>();
			Collect(expectedRootNode, nodes);
			return new TheoryData<string>(nodes.Select(x => x.Path));

			static void Collect(ExpectedDataNode node, ICollection<ExpectedDataNode> nodes)
			{
				nodes.Add(node);
				foreach (ExpectedDataNode child in node.Children)
				{
					Collect(child, nodes);
				}
			}
		}
	}
}
