///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.CompilerServices;
#if !NET8_0_OR_GREATER
using CommunityToolkit.HighPerformance;
#endif

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Helper functions that come in handy when working with path strings.
/// </summary>
public static class PathHelpers
{
	internal const char   EscapeChar    = '\\';
	internal const char   SeparatorChar = '/';
	internal const string RootPath      = "/";
	internal const int    MaxNameLength = 255; // unescaped

	/// <summary>
	/// Checks whether the specified name is a valid name in the data manager (see remarks).
	/// </summary>
	/// <param name="name">Name to check.</param>
	/// <returns>
	/// <c>true</c> if the specified string is a valid data manager name;<br/>
	/// otherwise <c>false</c>.
	/// </returns>
	/// <remarks>
	/// This method considers any name valid that fulfills the following requirements:<br/>
	/// - Name is not empty (zero length)<br/>
	/// - Name does not contain any leading or trailing whitespaces<br/>
	/// - Name does not contain control characters from control code set C0 (U+0000 to U+001F)<br/>
	/// - Name does not contain control characters from control code set C1 (U+0080 to U+009F)<br/>
	/// - Name does not contain a DEL character (U+007F)
	/// </remarks>
	public static bool IsValidName(ReadOnlySpan<char> name)
	{
		// trim leading/trailing whitespaces to prevent the user from entering an empty string
		ReadOnlySpan<char> trimmedName = name.Trim();

		// names with leading or trailing whitespaces are not valid
		if (trimmedName.Length < name.Length)
			return false;

		// empty names are not valid
		if (name.Length == 0)
			return false;

		// names exceeding the maximum allowed name length are not valid
		if (name.Length > MaxNameLength)
			return false;

		// check whether valid characters are used only
		foreach (char c in name)
		{
			if (!IsValidNameChar(c))
				return false;
		}

		// check passed
		return true;
	}

	/// <summary>
	/// Appends the specified name to the specified path escaping the name properly (for internal use only).<br/>
	/// This method skips consistency checks for maximum performance.
	/// </summary>
	/// <param name="path">Path to add <paramref name="name"/> to (must already be escaped properly).</param>
	/// <param name="name">Name to add to the path (unescaped).</param>
	/// <returns>The combined path.</returns>
	internal static string AppendNameToPath(string path, ReadOnlySpan<char> name)
	{
		int separatorCount = name.Count(SeparatorChar);

		if (separatorCount > 0)
		{
			// the name contains path separators that need to be escaped
			// ----------------------------------------------------------------------------------------------------

			// assembling buffer is allocated on the stack, string is constructed from this buffer
			int length = path.Length + name.Length + separatorCount;
			bool isRootPath = path.Length == 1;
			if (!isRootPath) length++;
			Span<char> buffer = stackalloc char[length];
			Span<char> writer = buffer;

			// copy base path to buffer
			if (isRootPath)
			{
				writer[0] = SeparatorChar;
				writer = writer[1..];
			}
			else
			{
				path.AsSpan().CopyTo(writer);
				writer = writer[path.Length..];
				writer[path.Length] = SeparatorChar;
				writer = writer[1..];
			}

			// write escaped name to the buffer
			ReadOnlySpan<char> reader = name;
			while (!reader.IsEmpty)
			{
				int index = reader.IndexOf(SeparatorChar);

				// copy part before the separator,
				// respectively at the end of the path
				if (index > 0)
				{
					reader[..index].CopyTo(writer);
					writer = writer[index..];
					reader = reader[(index + 1)..];
				}
				else if (index < 0)
				{
					reader.CopyTo(writer);
					writer = writer[reader.Length..];
					reader = reader[reader.Length..];
					continue;
				}

				// escape separator
				writer[0] = EscapeChar;
				writer[1] = SeparatorChar;
				writer = writer[2..];
			}

			return buffer.ToString();
		}
		else
		{
			// name does not contain any path separators that need to be escaped
			// ----------------------------------------------------------------------------------------------------

			// assembling buffer is allocated on the stack, string is constructed from this buffer
			int length = path.Length + name.Length;
			if (path.Length > 1) length++;
			Span<char> buffer = stackalloc char[length];

			if (path == RootPath)
			{
				// path 1 is the root path
				// => no additional path delimiter needed
				path.AsSpan().CopyTo(buffer);
				name.CopyTo(buffer[path.Length..]);
			}
			else
			{
				// path 1 is not the root path
				// => no additional path delimiter needed
				path.AsSpan().CopyTo(buffer);
				buffer[path.Length] = SeparatorChar;
				name.CopyTo(buffer[(path.Length + 1)..]);
			}

			return buffer.ToString();
		}
	}

	#region Helpers

	/// <summary>
	/// Checks whether the specified character is allowed to be used within a path string.
	/// </summary>
	/// <param name="character">Character to check.</param>
	/// <returns>
	/// <c>true</c> if <paramref name="character"/> is a valid path character;<br/>
	/// otherwise <c>false</c>.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsValidNameChar(char character)
	{
		if (character >= 0x0000 && character <= 0x001F) return false; // c0 control ([0x0000..0x001F])
		return character < 0x007F || character > 0x009F;              // 0x7F (DEL) + c1 control ([0x0080..0x009F])
	}

	#endregion
}
