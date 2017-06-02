using System;
using System.Text;
namespace Nox.Debug {
	public class AstPrinter : Expr.IVisitor<string>, Stmt.IVisitor<string> {

		#region ExprVisitors

		public string VisitAssignExpr(Expr.Assign expr) {
			return Parenthesize("=", expr.name, expr.value);
		}

		public string VisitBinaryExpr(Expr.Binary expr) {
			return Parenthesize(expr.op.lexeme, expr.left, expr.right);
		}

		public string VisitCallExpr(Expr.Call expr) {
			return Parenthesize("call", expr.callee, expr.arguments);
		}

		public string VisitGetExpr(Expr.Get expr) {
			return Parenthesize("get", expr.obj, expr.name);
		}

		public string VisitGroupingExpr(Expr.Grouping expr) {
			return Parenthesize("group", expr.expression);
		}

		public string VisitLiteralExpr(Expr.Literal expr) {
			if (expr.value is string) {
				return string.Format("\"{0}\"", expr.value);
			}
			return expr.value.ToString();
		}
		
		public string VisitLogicalExpr(Expr.Logical expr) {
			return Parenthesize(expr.op.lexeme, expr.left, expr.right);
		}

		public string VisitSetExpr(Expr.Set expr) {
			return Parenthesize("set", expr.obj, expr.name);
		}

		public string VisitThisExpr(Expr.This expr) {
			return "this";
		}

		public string VisitUnaryExpr(Expr.Unary expr) {
			return Parenthesize(expr.op.lexeme, expr.right);
		}

		public string VisitVariableExpr(Expr.Variable expr) {
			return expr.name.lexeme;
		}
		#endregion ExprVisitors

		#region StmtVisitors

		public string VisitBlockStmt(Stmt.Block stmt) {
			throw new NotImplementedException();
		}

		public string VisitClassStmt(Stmt.Class stmt) {
			throw new NotImplementedException();
		}

		public string VisitExpressionStmt(Stmt.Expression stmt) {
			return Parenthesize(";", stmt.expression);
		}

		public string VisitFunctionStmt(Stmt.Function stmt) {
			StringBuilder builder = new StringBuilder();

			throw new NotImplementedException();
		}

		public string VisitIfStmt(Stmt.If stmt) {
			if (stmt.elseBranch == null) {
				return Parenthesize("if-then", stmt.condition, stmt.thenBranch);
			}
			return Parenthesize("if-then-else", stmt.condition, stmt.thenBranch, stmt.elseBranch);
		}

		public string VisitPrintStmt(Stmt.Print stmt) {
			return Parenthesize("print", stmt.expression);
		}

		public string VisitReturnStmt(Stmt.Return stmt) {
			if (stmt.value == null) {
				return "return";
			}
			return Parenthesize("return", stmt.value);
		}

		public string VisitVarStmt(Stmt.Var stmt) {
			if (stmt.initializer == null) {
				return Parenthesize("var", stmt.name);
			}
			return Parenthesize("var", stmt.name, "=", stmt.initializer);
		}

		public string VisitWhileStmt(Stmt.While stmt) {
			throw new NotImplementedException();
		}

		#endregion StmtVisitors

		public string Print(Expr expr) {
			return expr.Accept(this);
		}

		public string Print(Stmt stmt) {
			return stmt.Accept(this);
		}

		private string Parenthesize(string name, params object[] parts) {
			StringBuilder builder = new StringBuilder();

			builder.Append("(").Append(name);
			foreach (object part in parts) {
				builder.Append(" ");
				if (part is Expr) {
					builder.Append(((Expr)part).Accept(this));
				} else if (part is Stmt) {
					builder.Append(((Stmt)part).Accept(this));
				} else if (part is Token) {
					builder.Append(((Token)part).lexeme);
				} else {
					builder.Append(part);
				}
			}
			builder.Append(")");

			return builder.ToString();
		}
	}
}
