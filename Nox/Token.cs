﻿using System;
namespace Nox {
	public struct Token {
		public readonly TokenType type;
		public readonly string lexeme;
		public readonly object literal;
		public readonly int line;

		public Token(TokenType type, string lexeme, object literal, int line) {
			this.type = type;
			this.lexeme = lexeme;
			this.literal = literal;
			this.line = line;
		}

		public override string ToString() {
			return string.Format("{0} {1} {2}", type, lexeme, literal);
		}
	}
}
