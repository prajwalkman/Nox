using System;
using System.Collections.Generic;

namespace Nox.Frontend {
	public class Lexer {
		private readonly string source;
		private List<Token> tokens = new List<Token>();

		private int start = 0;
		private int current = 0;
		private int line = 1;
		private int column = 0;

		private static readonly Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>() {
			{ "and",    TokenType.AND },
			{ "class",  TokenType.CLASS },
			{ "else",   TokenType.ELSE },
			{ "false",  TokenType.FALSE },
			{ "for",    TokenType.FOR },
			{ "fun",    TokenType.FUN },
			{ "if",     TokenType.IF },
			{ "nil",    TokenType.NIL },
			{ "or",     TokenType.OR },
			{ "print",  TokenType.PRINT },
			{ "return", TokenType.RETURN },
			{ "super",  TokenType.SUPER },
			{ "this",   TokenType.THIS },
			{ "true",   TokenType.TRUE },
			{ "var",    TokenType.VAR },
			{ "while",  TokenType.WHILE },
		};

		public Lexer(string Source) {
			source = Source;
		}

		public List<Token> ScanTokens() {
			while (!IsEOF()) {
				start = current;
				ScanToken();
			}

			tokens.Add(new Token(TokenType.EOF, "", null, line, column));

			return tokens;
		}

		private void ScanToken() {
			char c = Advance();
			switch (c) {
				case '(': AddToken(TokenType.LEFT_PAREN); break;
				case ')': AddToken(TokenType.RIGHT_PAREN); break;
				case '{': AddToken(TokenType.LEFT_BRACE); break;
				case '}': AddToken(TokenType.RIGHT_BRACE); break;
				case ',': AddToken(TokenType.COMMA); break;
				case '.': AddToken(TokenType.DOT); break;
				case '-': AddToken(TokenType.MINUS); break;
				case '+': AddToken(TokenType.PLUS); break;
				case ';': AddToken(TokenType.SEMICOLON); break;
				case '*': AddToken(TokenType.STAR); break;
				case ':': AddToken(TokenType.COLON); break;

				case '!': AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
				case '=': AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
				case '<': AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
				case '>': AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
				
				case '/': {
						if (Match('/')) {
							// Comment until EOL
							while (Peek() != '\n' &&
								   !IsEOF()) {
								Advance();
							}
						} else {
							AddToken(TokenType.SLASH);
						}
					} break;

				case ' ':
				case '\r':
				case '\t':
					// Ignore whitespace.
					break;

				case '\n':
					line++;
					column = 0;
					break;

				case '"': ScanString(); break;

				default:
					if (IsDigit(c)) {
						ScanNumber();
					} else if (IsAlpha(c)) {
						ScanIdentifier();
					} else {
						Nox.Error(line, column, "unexpected character");
					}
					break;
			}
		}

		private void ScanIdentifier() {
			while (IsAlphaNumeric(Peek())) Advance();

			string literal = source.Substring(start, current - start);

			TokenType type = TokenType.IDENTIFIER;
			if (keywords.ContainsKey(literal)) {
				type = keywords[literal];
			}

			AddToken(type);
		}

		private void ScanNumber() {
			while (IsDigit(Peek())) Advance();
			if (Peek() == '.' && IsDigit(PeekNext())) {
				// Consume the decimal
				Advance();

				while (IsDigit(Peek())) Advance();
			}

			double literal = double.Parse(source.Substring(start, current - start));

			AddToken(TokenType.NUMBER, literal);
		}

		private void ScanString() {
			while (Peek() != '"' && !IsEOF()) {
				if (Peek() == '\n') {
					line++;
				}
				Advance();
			}

			if (IsEOF()) {
				Nox.Error(line, column, "Unterminated string");
				return;
			}

			// closing '"'
			Advance();

			string literal = source.Substring(start + 1, current - start - 2);
			AddToken(TokenType.STRING, literal);
		}

		private bool Match(char expected) {
			if (IsEOF()) return false;
			if (source[current] != expected) return false;

			current++;
			return true;
		}

		private char Peek() {
			if (IsEOF()) return '\0';
			return source[current];
		}

		private char PeekNext() {
			if (current + 1 >= source.Length) return '\0';
			return source[current + 1];
		}

		private char Advance() {
			current++;
			column++;
			return source[current - 1];
		}

		private void AddToken(TokenType type) {
			AddToken(type, null);
		}

		private void AddToken(TokenType type, object literal) {
			string text = source.Substring(start, current - start);
			tokens.Add(new Token(type, text, literal, line, column));
		}

		private bool IsEOF() {
			return current >= source.Length;
		}

		private bool IsAlpha(char c) {
			return
				(c >= 'a' && c <= 'z') ||
				(c >= 'A' && c <= 'Z') ||
				c == '_';
		}

		private bool IsDigit(char c) {
			return c >= '0' && c <= '9';
		}

		private bool IsAlphaNumeric(char c) {
			return IsAlpha(c) || IsDigit(c);
		}
	}
}
