using System;
using System.Collections.Generic;
using System.Linq;

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
				statements.Add(Declaration());
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
		// declaration → var_decl | fun_decl | statement
		// statement   → for_stmt | while_stmt | if_stmt
		//               | expr_stmt | print_stmt | block
		//               | return_stmt
		// for_stmt    → "for" "(" ( var_decl | expr_stmt | ";" ) expression? ";" expression? ")" statement
		// while_stmt  → "while" "(" expression ")" statement
		// if_stmt     → "if" "(" expression ")" statement ( "else" statement )?
		// block       → "{" declaration* "}"
		// print_stmt  → "print" expression ";"		
		// expr_stmt   → expression ";"
		// return_stmt → "return" expression? ";"
		// var_decl    → "var" IDENT ( "=" expression )? ";"
		// fun_decl    → "fun" function
		// function    → IDENT "(" parameters? ")" block
		// parameters  → IDENT ( "," IDENT )*

		private Stmt Declaration() {
			try {
				if (Match(TokenType.VAR)) {
					return VarDecl();
				}
				if (Match(TokenType.FUN)) {
					return FunDecl();
				}
				return Statement();
			} catch (ParserException) {
				Synchronize();
				return null;
			}
		}

		private Stmt Statement() {
			if (Match(TokenType.PRINT)) {
				return PrintStmt();
			}
			if (Match(TokenType.LEFT_BRACE)) {
				return BlockStmt();
			}
			if (Match(TokenType.IF)) {
				return IfStmt();
			}
			if (Match(TokenType.WHILE)) {
				return WhileStmt();
			}
			if (Match(TokenType.FOR)) {
				return ForStmt();	
			}
			if (Match(TokenType.RETURN)) {
				return ReturnStmt();
			}

			return ExprStmt();
		}

		private Stmt ReturnStmt() {
			Expr returnVal = null;
			Token keyword = Previous();
			if (!Check(TokenType.SEMICOLON)) {
				returnVal = Expression();
			}
			Consume(TokenType.SEMICOLON, "Expect ';' after return");
			return new Stmt.Return(keyword, returnVal);
		}

		private Stmt ForStmt() {
			// We desugar the for loop into a block

			// for (intitializer; condition; action) body

			// {
			//   intitializer
			//   while (condition) {
			//     body
			//     action
			//   }
			// }

			Stmt initializer = null;
			Expr condition = null;
			Expr action = null;

			Consume(TokenType.LEFT_PAREN, "Expect '('");
			if (!Match(TokenType.SEMICOLON)) {
				if (Match(TokenType.VAR)) {
					initializer = VarDecl();
				} else if (!Match(TokenType.SEMICOLON)){
					initializer = ExprStmt();
				} else {
					Consume(TokenType.SEMICOLON, "Expect ';'");
				}
			}
			if (!Match(TokenType.SEMICOLON)) {
				condition = Expression();
				Consume(TokenType.SEMICOLON, "Expect ';'");
			}
			if (!Match(TokenType.RIGHT_PAREN)) {
				action = Expression();
				Consume(TokenType.RIGHT_PAREN, "Expect ')'");
			}

			Stmt body = Statement();
			List<Stmt> bodyStmts = new List<Stmt>();
			if (body is Stmt.Block) {
				bodyStmts.AddRange(((Stmt.Block)body).statements);
			} else {
				bodyStmts.Add(body);
			}
			if (action != null) {
				bodyStmts.Add(new Stmt.Expression(action));
			}
			if (condition == null) {
				condition = new Expr.Literal(true);
			}
			
			Stmt whileStmt = new Stmt.While(condition, new Stmt.Block(bodyStmts));

			List<Stmt> blockStmts = new List<Stmt>();
			if (initializer != null) {
				blockStmts.Add(initializer);
			}
			blockStmts.Add(whileStmt);

			return new Stmt.Block(blockStmts);
		}

		private Stmt WhileStmt() {
			Consume(TokenType.LEFT_PAREN, "Expect '('");
			Expr condition = Expression();
			Consume(TokenType.RIGHT_PAREN, "Expect ')'");
			Stmt body = Statement();
			return new Stmt.While(condition, body);
		}

		private Stmt IfStmt() {
			Consume(TokenType.LEFT_PAREN, "Expect '('");
			Expr condition = Expression();
			Consume(TokenType.RIGHT_PAREN, "Expect ')'");
			Stmt thenBranch = Statement();
			Stmt elseBranch = null;
			if (Match(TokenType.ELSE)) {
				elseBranch = Statement();
			}
			return new Stmt.If(condition, thenBranch, elseBranch);
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

		private Stmt FunDecl() {
			Token name = Consume(TokenType.IDENTIFIER, "Expect function name");
			Consume(TokenType.LEFT_PAREN, "Expect '(' after function name");
			List<Token> parameters = new List<Token>();

			if (!Check(TokenType.RIGHT_PAREN)) {
				do {
					Token param = Consume(TokenType.IDENTIFIER, "Expect parameter name");
					parameters.Add(param);
				} while (Match(TokenType.COMMA));
			}

			Consume(TokenType.RIGHT_PAREN, "Expect ')' after param list");
			Consume(TokenType.LEFT_BRACE, "Expect '{' after function declaration");

			Stmt body = BlockStmt();
			List<Stmt> bodyStatements = ((Stmt.Block)body).statements;

			return new Stmt.Function(name, parameters, bodyStatements);
		}

		// expression → assignment
		// assignment → IDENT ( "=" assignment )? | equality
		// equality   → comparison ( ( "!=" | "==" ) comparison )*
		// comparison → term ( ( ">" | ">=" | "<" | "<=" ) term )*
		// term       → factor ( ( "-" | "+" ) factor )*
		// factor     → unary ( ( "/" | "*" ) unary )*
		// unary      → ( "!" | "-" ) unary | call
		// call       → primary ( "(" arguments ")" | IDENT )*
		// primary    → NUMBER | STRING | "false" | "true" | "nil"
		//            | "(" expression ")" | IDENT
		// arguments  → expression ( "," expression )*


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

			return Call();
		}

		private Expr Call() {
			Expr expr = Primary();
			if (Match(TokenType.LEFT_PAREN)) {
				List<Expr> args = Arguments();
				Token paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after call.");
				return new Expr.Call(expr, paren, args);
			}

			return expr;
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

		private List<Expr> Arguments() {
			List<Expr> result = new List<Expr>();
			if (!Check(TokenType.RIGHT_PAREN)) {
				do {
					result.Add(Expression());
				} while (Match(TokenType.COMMA));
			}
			return result;
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
