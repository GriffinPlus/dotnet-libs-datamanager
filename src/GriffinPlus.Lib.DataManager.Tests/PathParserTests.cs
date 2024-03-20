///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

#pragma warning disable xUnit1026 // Theory methods should use all of their parameters

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Tests targeting the <see cref="PathParser"/> struct.
/// </summary>
public class PathParserTests
{
	#region Test Data

	/// <summary>
	/// Test data with valid paths only.
	/// </summary>
	public static IEnumerable<object[]> TestData_Valid
	{
		get
		{
			foreach (bool absolute in new[] { false, true })
			{
				string prefix = absolute ? "/" : "";
				yield return [prefix + "", absolute, new string[] { }];                           // root path
				yield return [prefix + "A", absolute, new[] { "A" }];                             // depth = 1
				yield return [prefix + "A/B", absolute, new[] { "A", "B" }];                      // depth = 2
				yield return [prefix + "A/B/C", absolute, new[] { "A", "B", "C" }];               // depth = 3
				yield return [prefix + " A /  B  /   C   ", absolute, new[] { "A", "B", "C" }];   // depth = 3, padded path tokens
				yield return [prefix + @"A\/B\/C/D", absolute, new[] { "A/B/C", "D" }];           // depth = 2, with escaping (A/B/C)
				yield return [prefix + @" A \/ B \/ C /D", absolute, new[] { "A / B / C", "D" }]; // depth = 2, with escaping (A/B/C), padded path tokens

				// testing with tokens of maximum supported length
				string maxLengthToken = new('X', PathHelpers.MaxNameLength);

				// max length path token, depth = 1
				yield return
				[
					prefix + maxLengthToken,
					absolute,
					new[] { maxLengthToken }
				];

				// max length path token, depth = 2
				yield return
				[
					prefix + maxLengthToken + "/" + maxLengthToken,
					absolute,
					new[] { maxLengthToken, maxLengthToken }
				];

				// max length path token, depth = 2, with padding (should not trigger the length check)
				yield return
				[
					prefix + " " + maxLengthToken + " / " + maxLengthToken + " ",
					absolute,
					new[] { maxLengthToken, maxLengthToken }
				];
			}
		}
	}

	/// <summary>
	/// Test data with invalid paths only.
	/// </summary>
	public static IEnumerable<object[]> TestData_Invalid
	{
		get
		{
			foreach (bool absolute in new[] { false, true })
			{
				string prefix = absolute ? "/" : "";
				yield return [prefix + "A//C", absolute, null];         // zero-length path token, without padding
				yield return [prefix + " A //   C   ", absolute, null]; // zero-length path token, with padding
				yield return [prefix + "\x01", absolute, null];         // unsupported character (0x01)

				// path token too long
				yield return
				[
					prefix + new string('X', PathHelpers.MaxNameLength + 1),
					absolute,
					null
				];
			}
		}
	}

	#endregion

	#region Construction

	/// <summary>
	/// Tests whether the constructor of <see cref="PathParser"/> works as expected.
	/// </summary>
	[Fact]
	public void Construct()
	{
		var parser = new PathParser();
		Assert.False(parser.IsAbsolutePath);
		Assert.True(parser.Path.IsEmpty);
		Assert.True(parser.Remaining.IsEmpty);
	}

	#endregion

	#region Create(ReadOnlySpan<char> path, bool validate)

	/// <summary>
	/// Test data for test methods targeting <see cref="PathParser.Create"/> and implicitly <see cref="PathParser.Reinitialize"/>.
	/// This property provides test data with valid paths only.
	/// </summary>
	public static IEnumerable<object[]> TestData_Valid_ForCreate => TestData_Valid.Select(data => (object[]) [data[0], data[1]]);

	/// <summary>
	/// Test data for test methods targeting <see cref="PathParser.Create"/> and implicitly <see cref="PathParser.Reinitialize"/>.
	/// This property provides test data with invalid paths only.
	/// </summary>
	public static IEnumerable<object[]> TestData_Invalid_ForCreate => TestData_Invalid.Select(data => (object[]) [data[0], data[1]]);

	/// <summary>
	/// Tests whether <see cref="PathParser.Create"/> and implicitly <see cref="PathParser.Reinitialize"/>
	/// succeeds with valid and invalid paths, if validation is disabled.
	/// </summary>
	/// <param name="path">Path to parse.</param>
	/// <param name="isAbsolutePath">
	/// <c>true</c> if <paramref name="path"/> is an absolute path;<br/>
	/// <c>false</c> if <paramref name="path"/> is a relative path.
	/// </param>
	[Theory]
	[MemberData(nameof(TestData_Valid_ForCreate))]
	[MemberData(nameof(TestData_Invalid_ForCreate))]
	public void Create_WithoutValidation(string path, bool isAbsolutePath)
	{
		var parser = PathParser.Create(path.AsSpan(), false);
		Assert.Equal(isAbsolutePath, parser.IsAbsolutePath);
		Assert.Equal(path.Trim(), parser.Path.ToString());
		Assert.Equal(isAbsolutePath ? path.Trim()[1..] : path.Trim(), parser.Remaining.ToString());
	}

	/// <summary>
	/// Tests whether <see cref="PathParser.Create"/> and implicitly <see cref="PathParser.Reinitialize"/>
	/// succeeds with valid paths, if validation is enabled.
	/// </summary>
	/// <param name="path">Path to parse.</param>
	/// <param name="isAbsolutePath">
	/// <c>true</c> if <paramref name="path"/> is an absolute path;<br/>
	/// <c>false</c> if <paramref name="path"/> is a relative path.
	/// </param>
	[Theory]
	[MemberData(nameof(TestData_Valid_ForCreate))]
	public void Init_WithValidation_OK(string path, bool isAbsolutePath)
	{
		var parser = PathParser.Create(path.AsSpan(), false);
		Assert.Equal(isAbsolutePath, parser.IsAbsolutePath);
		Assert.Equal(path.Trim(), parser.Path.ToString());
		Assert.Equal(isAbsolutePath ? path.Trim()[1..] : path.Trim(), parser.Remaining.ToString());
	}

	/// <summary>
	/// Tests whether <see cref="PathParser.Create"/> and implicitly <see cref="PathParser.Reinitialize"/>
	/// fails with invalid paths, if validation is enabled.
	/// </summary>
	/// <param name="path">Path to parse.</param>
	/// <param name="isAbsolutePath">
	/// <c>true</c> if <paramref name="path"/> is an absolute path;<br/>
	/// <c>false</c> if <paramref name="path"/> is a relative path.
	/// </param>
	[Theory]
	[MemberData(nameof(TestData_Invalid_ForCreate))]
	public void Create_WithValidation_NOK(string path, bool isAbsolutePath)
	{
		Assert.Throws<ArgumentException>(() => PathParser.Create(path.AsSpan(), true));
	}

	#endregion
}
