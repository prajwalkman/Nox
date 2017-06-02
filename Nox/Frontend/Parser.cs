using System;
using System.Collections.Generic;

namespace Nox.Frontend {
	public class Parser {
		private readonly List<Token> tokens;
		private int current = 0;

		public Parser(List<Token> tokens) {
			this.tokens = tokens;
		}

		public List<Stmt> Parse() {
			List<Stmt> statements = new List<Stmt>();

			while (!IsEOF()) {
				try {
					Stmt newStmt = Statement();
					statements.Add(newStmt);
				} catch (ParserException) {
					return null;
				}
			}
			return statements;
		}

		#region utils

		private Token Peek() {
			return tokens[current];
		}

		private Token Previous() {
			return tokens[current - 1];
		}

		private bool IsEOF() {
			return Peek().type == TokenType.EOF;
		}

		private bool Check(TokenType type) {
			if (IsEOF()) return false;
			return Peek().type == type;
		}

		private Token Advance() {
			if (!IsEOF()) current++;
			return Previous();
		}

		private bool Match(params TokenType[] types) {
			foreach (TokenType type in types) {
				if (Check(type)) {
					Advance();
					return true;
				}
			}
			return false;
		}

		#endregion utils

		// program    → statement* EOF
		// statement  → expr_stmt | print_stmt
		// print_stmt → "print" expression ";"		//! Note - this is a terminal, 
													//! so cannot be represented 
													//! as `"print" expr_stmt`
		// expr_stmt  → expression ";"

		private Stmt Statement() {
			if (Match(TokenType.PRINT)) {
				return PrintStmt();
			}
			return ExprStmt();
		}

		private Stmt PrintStmt() {
			Expr expression = Expression();
			Consume(TokenType.SEMICOLON, "Expect ';' after statement");
			return new Stmt.Print(expression);
		}

		private Stmt ExprStmt() {
			Expr expression = Expression();
			Consume(TokenType.SEMICOLON, "Expect ';' after statement");
			return new Stmt.Expression(expression);
		}

		// expression → equality
		// equality   → comparison ( ( "!=" | "==" ) comparison )*
		// comparison → term ( ( ">" | ">=" | "<" | "<=" ) term )*
		// term       → factor ( ( "-" | "+" ) factor )*
		// factor     → unary ( ( "/" | "*" ) unary )*
		// unary      → ( "!" | "-" ) unary
		//            | primary
		// primary    → NUMBER | STRING | "false" | "true" | "nil"
		//            | "(" expression ")"

		private Expr Expression() {
			return Equality();
		}

		private Expr Equality() {
			Expr expr = Comparison();

			while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL)) {
				Token op = Previous();
				Expr right = Comparison();
				expr = new Expr.Binary(expr, op, right);
			}

			return expr;
		}

		private Expr Comparison() {
			Expr expr = Term();

			while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL)) {
				Token op = Previous();
				Expr right = Term();
				expr = new Expr.Binary(expr, op, right);
			}

			return expr;
		}

		private Expr Term() {
			Expr expr = Factor();

			while (Match(TokenType.MINUS, TokenType.PLUS)) {
				Token op = Previous();
				Expr right = Factor();
				expr = new Expr.Binary(expr, op, right);
			}

			return expr;
		}

		private Expr Factor() {
			Expr expr = Unary();

			while (Match(TokenType.SLASH, TokenType.STAR)) {
				Token op = Previous();
				Expr right = Unary();
				expr = new Expr.Binary(expr, op, right);
			}

			return expr;
		}

		private Expr Unary() {
			if (Match(TokenType.BANG, TokenType.MINUS)) {
				Token op = Previous();
				Expr right = Unary();
				return new Expr.Unary(op, right);
			}

			return Primary();
		}

		private Expr Primary() {
			if (Match(TokenType.FALSE)) return new Expr.Literal(false);
			if (Match(TokenType.TRUE)) return new Expr.Literal(true);
			if (Match(TokenType.NIL)) return new Expr.Literal(null);

			if (Match(TokenType.NUMBER, TokenType.STRING)) {
				return new Expr.Literal(Previous().literal);
			}

			if (Match(TokenType.LEFT_PAREN)) {
				Expr expr = Expression();
				Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
				return new Expr.Grouping(expr);
			}

			throw Error(Peek(), "Expect expression.");
		}

		private Token Consume(TokenType type, string errorMsg) {
			if (Check(type)) return Advance();

			throw Error(Peek(), errorMsg);
		}

		private ParserException Error(Token token, string msg) {
			Nox.Error(token, msg);
			return new ParserException();
		}

		public class ParserException : Exception {}

		// Unused until Statements are implemented
		private void Synchronize() {
			Advance();

			while (!IsEOF()) {
				if (Previous().type == TokenType.SEMICOLON) return;

				switch(Peek().type) {
					case TokenType.CLASS:
					case TokenType.FUN:
					case TokenType.VAR:
					case TokenType.FOR:
					case TokenType.IF:
					case TokenType.WHILE:
					case TokenType.PRINT:
					case TokenType.RETURN:
						return;				
				}

				Advance();
			}
		}
	}
}
