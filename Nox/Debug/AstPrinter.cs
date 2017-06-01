using System;
using System.Text;
namespace Nox.Debug {
	public class AstPrinter : Expr.IVisitor<string> {

		public string VisitAssignExpr(Expr.Assign expr) {
			throw new NotImplementedException();
		}

		public string VisitBinaryExpr(Expr.Binary expr) {
			return Parenthesize(expr.op.lexeme, expr.left, expr.right);
		}

		public string VisitCallExpr(Expr.Call expr) {
			throw new NotImplementedException();
		}

		public string VisitGetExpr(Expr.Get expr) {
			throw new NotImplementedException();
		}

		public string VisitGroupingExpr(Expr.Grouping expr) {
			return Parenthesize("group", expr.expression);
		}

		public string VisitLiteralExpr(Expr.Literal expr) {
			return expr.value.ToString();
		}
		
		public string VisitLogicalExpr(Expr.Logical expr) {
			throw new NotImplementedException();
		}

		public string VisitSetExpr(Expr.Set expr) {
			throw new NotImplementedException();
		}

		public string VisitThisExpr(Expr.This expr) {
			throw new NotSupportedException();
		}

		public string VisitUnaryExpr(Expr.Unary expr) {
			return Parenthesize(expr.op.lexeme, expr.right);
		}

		public string VisitVariableExpr(Expr.Variable expr) {
			throw new NotImplementedException();
		}

		public string Print(Expr expr) {
			return expr.Accept(this);
		}

		private string Parenthesize(string name, params Expr[] exprs) {
			StringBuilder builder = new StringBuilder();

			builder.Append("(").Append(name);
			foreach (Expr expr in exprs) {
				builder.Append(" ");
				builder.Append(expr.Accept(this));
			}
			builder.Append(")");

			return builder.ToString();
		}
	}
}
