using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PingUI.Tags;

/// <summary>
/// Base class for tag matching system.
/// </summary>
public abstract record Tag : IParsable<Tag>
{
	private static readonly Dictionary<TokenType, int> OperatorPrecedence = new()
	{
		[TokenType.LeftParenthesis] = 3,
		[TokenType.RightParenthesis] = 3,
		[TokenType.Not] = 2,
		[TokenType.And] = 1,
		[TokenType.Or] = 0,
	};

	private static readonly System.Buffers.SearchValues<char> TokenSeparatorChars = System.Buffers.SearchValues.Create(" \t()&|!");

	/// <inheritdoc />
	public static Tag Parse(string s, IFormatProvider? provider)
	{
		if (TryParseInner(s, out var result, out var exception))
		{
			return result;
		}
		else
		{
			throw exception;
		}
	}

	/// <inheritdoc />
	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Tag result)
	{
		if (s is null)
		{
			result = null;
			return false;
		}
		return TryParseInner(s, out result, out var _);
	}

	/// <inheritdoc cref="TryParse" />
	/// <param name="exception">The <see cref="Exception" /> to be throw if <see cref="TryParseInner" /> returns <see langword="false" />.</param>
	internal static bool TryParseInner(string s, [MaybeNullWhen(false)] out Tag result, [NotNullWhen(false)] out Exception? exception)
	{
		static Token? ReadString(ref ReadOnlySpan<char> span)
		{
			Token token;
			if (span[0] == '\"' || span[0] == '\'')
			{
				var nextQuote = span[1..].IndexOf(span[0]) + 1;
				if (nextQuote == 0)
				{
					return null;
				}
				token = new Token(new string(span[1..nextQuote]));
				span = span[(nextQuote + 1)..].TrimStart();
			}
			else
			{
				var nextSymbol = span.IndexOfAny(TokenSeparatorChars);
				if (nextSymbol == -1)
				{
					token = new Token(new string(span));
					span = span[span.Length..];
				}
				else
				{
					token = new Token(new string(span[..nextSymbol]));
					span = span[nextSymbol..].TrimStart();
				}
			}
			return token;
		}

		result = null;
		if (s is null)
		{
			exception = new ArgumentNullException(nameof(s));
			return false;
		}
		var span = s.AsSpan().Trim();
		var tokens = new List<Token>();
		while (!span.IsEmpty)
		{
			var token = span[0] switch
			{
				'(' => Token.LeftParenthesis,
				')' => Token.RightParenthesis,
				'&' => Token.And,
				'|' => Token.Or,
				'!' => Token.Not,
				_ => ReadString(ref span),
			};
			if (token is null)
			{
				exception = new FormatException("Missing end quote.");
				return false;
			}
			tokens.Add(token);
			if (token.Type != TokenType.Value)
			{
				span = span[1..].TrimStart();
			}
		}
		var operatorStack = new Stack<Token>();
		var outputQueue = new Queue<Token>();
		foreach (var token in tokens)
		{
			switch (token.Type)
			{
				case TokenType.Value:
					outputQueue.Enqueue(token);
					break;
				case TokenType.LeftParenthesis:
					operatorStack.Push(token);
					break;
				case TokenType.RightParenthesis:
					while (operatorStack.Count > 0 && operatorStack.Peek().Type != TokenType.LeftParenthesis)
					{
						outputQueue.Enqueue(operatorStack.Pop());
					}
					if (operatorStack.Count == 0)
					{
						exception = new FormatException("Mismatched parenthesis.");
						return false;
					}
					operatorStack.Pop();
					break;
				// case TokenType.Not:
				// case TokenType.And:
				// case TokenType.Or:
				default:
					while (operatorStack.Count > 0 && operatorStack.Peek().Type != TokenType.LeftParenthesis && OperatorPrecedence[operatorStack.Peek().Type] >= OperatorPrecedence[token.Type])
					{
						outputQueue.Enqueue(operatorStack.Pop());
					}
					operatorStack.Push(token);
					break;
			}
		}
		while (operatorStack.Count > 0)
		{
			outputQueue.Enqueue(operatorStack.Pop());
		}
		var tagStack = new Stack<Tag>();
		while (outputQueue.Count > 0)
		{
			var currentToken = outputQueue.Dequeue();
			switch (currentToken.Type)
			{
				case TokenType.Value:
					tagStack.Push(new Literal(currentToken.Content!));
					break;
				case TokenType.Not:
					if (tagStack.Count == 0)
					{
						exception = new FormatException("Not operator with no operand on stack.");
						return false;
					}
					tagStack.Push(new Not(tagStack.Pop()));
					break;
				case TokenType.And:
					if (tagStack.Count < 2)
					{
						exception = new FormatException("And operator with not enough operands on stack.");
						return false;
					}
					tagStack.Push(new And(new Tag[] { tagStack.Pop(), tagStack.Pop() }.Reverse()));
					break;
				case TokenType.Or:
					if (tagStack.Count < 2)
					{
						exception = new FormatException("Or operator with not enough operands on stack.");
						return false;
					}
					tagStack.Push(new Or(new Tag[] { tagStack.Pop(), tagStack.Pop() }.Reverse()));
					break;
				default:
					throw new UnreachableException();
			}
		}
		if (tagStack.Count == 1)
		{
			result = tagStack.Pop();
			exception = null;
			return true;
		}
		else
		{
			exception = new FormatException("Incorrect number of tags on the stack after processing.");
			return false;
		}
	}

	/// <summary>
	/// Checks to see if an item matches by its tag list.
	/// </summary>
	/// <param name="itemTags">A set of strings that represents all tags of the item to match.</param>
	/// <returns><see langword="true" /> if <paramref name="itemTags" /> matches this <see cref="Tag" />; otherwise <see langword="false" />.</returns>
	public abstract bool IsMatch(IReadOnlyList<string> itemTags);

	private enum TokenType
	{
		Value,
		LeftParenthesis,
		RightParenthesis,
		And,
		Or,
		Not,
	}

	private record Token
	{
		public static readonly Token LeftParenthesis = new(TokenType.LeftParenthesis);

		public static readonly Token RightParenthesis = new(TokenType.RightParenthesis);

		public static readonly Token And = new(TokenType.And);

		public static readonly Token Or = new(TokenType.Or);

		public static readonly Token Not = new(TokenType.Not);

		public Token(string content)
		{
			Type = TokenType.Value;
			Content = content;
		}

		private Token(TokenType type)
		{
			Type = type;
		}

		public string? Content
		{
			get;
		}

		public int Length => Type == TokenType.Value ? Content!.Length : 1;

		public TokenType Type
		{
			get;
		}
	}
}
