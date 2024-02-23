///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

// ReSharper disable InvertIf

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// A parser that validates and splits a path string into names of data nodes and data values.
/// </summary>
unsafe ref struct PathParser
{
	private const char EscapeChar    = PathHelpers.EscapeChar;
	private const char SeparatorChar = PathHelpers.SeparatorChar;
	private const int  MaxNameLength = PathHelpers.MaxNameLength; // unescaped

	private fixed char               mAssemblingBuffer[MaxNameLength];
	internal      ReadOnlySpan<char> Path;
	internal      ReadOnlySpan<char> Remaining;
	public        bool               IsAbsolutePath;

	/// <summary>
	/// Initializes a parser to parse the specified path.
	/// </summary>
	/// <param name="path">Path to parse.</param>
	/// <param name="validate"><c>true</c> to validate <paramref name="path"/>; otherwise <c>false</c>.</param>
	/// <exception cref="ArgumentException">The specified <paramref name="path"/> is malformed and failed validation.</exception>
	public static PathParser Create(in ReadOnlySpan<char> path, in bool validate = true)
	{
		var parser = new PathParser();
		parser.Reinitialize(path, validate);
		return parser;
	}

	/// <summary>
	/// Sets the parser up to parse the specified path.
	/// </summary>
	/// <param name="path">Path to parse.</param>
	/// <param name="validate"><c>true</c> to validate <paramref name="path"/>; otherwise <c>false</c>.</param>
	/// <exception cref="ArgumentException">The specified <paramref name="path"/> is malformed and failed validation.</exception>
	public void Reinitialize(in ReadOnlySpan<char> path, in bool validate = true)
	{
		Path = path.Trim();
		IsAbsolutePath = Path.Length > 0 && Path[0] == SeparatorChar;
		Remaining = IsAbsolutePath ? Path[1..] : Path;
		if (validate) Validate(Path); // can throw ArgumentException
	}

	/// <summary>
	/// Gets the next path token.
	/// </summary>
	/// <param name="token">Receives the next path token.</param>
	/// <returns>
	/// <c>true</c> if the path contained another token that was returned via <paramref name="token"/>;<br/>
	/// <c>false</c> if the path does not contain any more tokens.
	/// </returns>
	/// <param name="isLastToken">
	/// Receives <c>true</c> if <paramref name="token"/> is the last token in the parsed path;<br/>
	/// otherwise <c>false</c>.
	/// </param>
	/// <exception cref="ArgumentException">The parsed path is malformed.</exception>
	public bool GetNext(out ReadOnlySpan<char> token, out bool isLastToken)
	{
		token = null;
		isLastToken = true;
		if (Remaining.IsEmpty) return false;

		int pathLength = Remaining.Length;
		fixed (char* pcTokenAssemblingBufferStart = mAssemblingBuffer)
		{
			char* pcTokenAssemblingBuffer = pcTokenAssemblingBufferStart;

			// split the path at path separators
			bool foundEscapeChar = false;
			fixed (char* pcPathStart = Remaining)
			{
				char* pcPath = pcPathStart;
				char* pcPathEnd = pcPathStart + pathLength;

				// skip valid whitespaces at the beginning
				while (pcPath != pcPathEnd)
				{
					char c = *pcPath;
					if (!PathHelpers.IsValidNameChar(c) || !char.IsWhiteSpace(c)) break;
					pcPath++;
				}

				ReadOnlySpan<char> span;
				while (pcPath != pcPathEnd)
				{
					char c = *pcPath++;

					// check whether the character is allowed
					if (!PathHelpers.IsValidNameChar(c))
						throw new ArgumentException($"The specified path ({Path.ToString()}) contains an unexpected character ({c}).");

					// handle escaped characters
					if (foundEscapeChar)
					{
						if (pcTokenAssemblingBuffer - pcTokenAssemblingBufferStart >= MaxNameLength)
							throw new ArgumentException($"The specified path ({Path.ToString()}) contains a token with more than {MaxNameLength} characters.");

						*pcTokenAssemblingBuffer++ = c;
						foundEscapeChar = false;
						continue;
					}

					// handle unescaped characters
					switch (c)
					{
						case EscapeChar:
							foundEscapeChar = true;
							break;

						case SeparatorChar:
						{
							span = new ReadOnlySpan<char>(pcTokenAssemblingBufferStart, (int)(pcTokenAssemblingBuffer - pcTokenAssemblingBufferStart)).TrimEnd();

							if (span.Length == 0)
							{
								// token is empty
								// => not allowed as this means an empty node/value name...
								throw new ArgumentException($"The specified path ({Path.ToString()}) contains a zero-length token.");
							}

							// token is not empty
							// => return it...
							token = span;
							Remaining = new ReadOnlySpan<char>(pcPath, (int)(pcPathEnd - pcPath));
							isLastToken = Remaining.Length == 0;
							return true;
						}

						default:
							if (pcTokenAssemblingBuffer - pcTokenAssemblingBufferStart >= MaxNameLength)
								throw new ArgumentException($"The specified path ({Path.ToString()}) contains a token with more than {MaxNameLength} characters.");

							*pcTokenAssemblingBuffer++ = c;
							break;
					}
				}

				if (foundEscapeChar)
					throw new ArgumentException($"The specified path ({Path.ToString()}) is malformed (unpaired backslash at the end).");

				// process part from the last separator to the end of the path string, if necessary
				if (pcTokenAssemblingBuffer != pcTokenAssemblingBufferStart)
				{
					span = new ReadOnlySpan<char>(pcTokenAssemblingBufferStart, (int)(pcTokenAssemblingBuffer - pcTokenAssemblingBufferStart)).TrimEnd();

					if (span.Length == 0)
					{
						// subsequent token is empty
						// => not allowed as this means an empty node/value name...
						throw new ArgumentException($"The specified path ({Path.ToString()}) contains a zero-length token.");
					}

					token = span;
					Remaining = new ReadOnlySpan<char>(pcPath, (int)(pcPathEnd - pcPath));
					isLastToken = Remaining.Length == 0;
					return true;
				}

				return false;
			}
		}
	}

	/// <summary>
	/// Validates the specified path to ensure it can be processed successfully by <see cref="GetNext"/>.
	/// </summary>
	/// <param name="path">Path to validate.</param>
	/// <exception cref="ArgumentException">The parsed path is malformed.</exception>
	public static void Validate(in ReadOnlySpan<char> path)
	{
		int effectiveTokenLength = 0;
		bool foundEscapeChar = false;
		fixed (char* pcPathStart = path)
		{
			char* pcPath = pcPathStart;
			char* pcPathEnd = pcPathStart + path.Length;

			while (pcPath != pcPathEnd)
			{
				// skip valid whitespaces at the beginning of the token
				while (pcPath != pcPathEnd)
				{
					char c = *pcPath;
					if (!PathHelpers.IsValidNameChar(c) || !char.IsWhiteSpace(c)) break;
					pcPath++;
				}

				// abort, if the end of the path string is reached
				if (pcPath == pcPathEnd)
					break;

				// process path token
				while (pcPath != pcPathEnd)
				{
					char c = *pcPath++;

					// check whether the character is allowed
					if (!PathHelpers.IsValidNameChar(c))
						throw new ArgumentException($"The specified path ({path.ToString()}) contains an unexpected character ({c}).");

					// handle escaped characters
					if (foundEscapeChar)
					{
						effectiveTokenLength++;
						foundEscapeChar = false;
						continue;
					}

					// handle the 'escape' character (does not count as a regular character)
					if (c == EscapeChar)
					{
						foundEscapeChar = true;
						continue;
					}

					// handle path separators
					if (c == SeparatorChar)
					{
						if (effectiveTokenLength == 0)
							throw new ArgumentException($"The specified path ({path.ToString()}) contains a zero-length token.");
						if (effectiveTokenLength > MaxNameLength)
							throw new ArgumentException($"The specified path ({path.ToString()}) contains a token with more than {MaxNameLength} characters.");
						effectiveTokenLength = 0;
						break;
					}

					// handle regular characters
					effectiveTokenLength++;
				}

				if (foundEscapeChar)
					throw new ArgumentException($"The specified path ({path.ToString()}) is malformed (unpaired backslash at the end).");

				// process part from the last separator to the end of the path string, if necessary
				if (effectiveTokenLength > MaxNameLength)
					throw new ArgumentException($"The specified path ({path.ToString()}) contains a token with more than {MaxNameLength} characters.");
			}
		}
	}
}
