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
					Stmt newStmt = Declaration();
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

		// program     → declaration* EOF
		// declaration → var_decl | statement
		// statement   → expr_stmt | print_stmt | block
		// block       → "{" declaration* "}"
		// print_stmt  → "print" expression ";"		
		// expr_stmt   → expression ";"
		// var_decl    → "var" IDENT ( "=" expression )? ";"

		private Stmt Declaration() {
			if (Match(TokenType.VAR)) {
				return VarDecl();
			}
			return Statement();
		}

		private Stmt Statement() {
			if (Match(TokenType.PRINT)) {
				return PrintStmt();
			}
			if (Match(TokenType.LEFT_BRACE)) {
				return BlockStmt();
			}
			return ExprStmt();
		}

		private Stmt BlockStmt() {
			List<Stmt> statements = new List<Stmt>();
			while (!IsEOF() && !Check(TokenType.RIGHT_BRACE)) {
				Stmt statement = Declaration();
				statements.Add(statement);
			}
			Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
			return new Stmt.Block(statements);
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

		private Stmt VarDecl() {
			Token ident = Consume(TokenType.IDENTIFIER, "Expect identifier");
			Expr initializer = null;
			if (Match(TokenType.EQUAL)) {
				initializer = Expression();
			}
			Consume(TokenType.SEMICOLON, "Expect semicolon");
			return new Stmt.Var(ident, initializer);
		}

		// expression → assignment
		// assignment → IDENT ( "=" assignment )? | equality
		// equality   → comparison ( ( "!=" | "==" ) comparison )*
		// comparison → term ( ( ">" | ">=" | "<" | "<=" ) term )*
		// term       → factor ( ( "-" | "+" ) factor )*
		// factor     → unary ( ( "/" | "*" ) unary )*
		// unary      → ( "!" | "-" ) unary
		//            | primary
		// primary    → NUMBER | STRING | "false" | "true" | "nil"
		//            | "(" expression ")" | IDENT


		private Expr Expression() {
			return Assignment();
		}

		private Expr Assignment() {
			Expr lvalue = Equality();

			if (Match(TokenType.EQUAL)) {
				Token eq = Previous();
				Expr rvalue = Assignment();

				if (lvalue is Expr.Variable) {
					Token name = ((Expr.Variable)lvalue).name;
					return new Expr.Assign(name, rvalue);
				}

				Error(eq, "Invalid assignment target.");
			}

			return lvalue;
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

			if (Match(TokenType.IDENTIFIER)) return new Expr.Variable(Previous());


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
