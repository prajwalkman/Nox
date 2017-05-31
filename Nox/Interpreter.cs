using System;
using System.Linq;
namespace Nox {
	public class Interpreter : Expr.IVisitor<object> {

		public void Interpret(Expr expression) {
			try {
				object result = Evaluate(expression);
				Console.WriteLine(Stringify(result));
			} catch (RuntimeError e) {
				Nox.RuntimeError(e);
			}
		}

		public object VisitBinaryExpr(Expr.Binary expr) {
			object left = Evaluate(expr.left);
			object right = Evaluate(expr.right);

			switch(expr.op.type) {
				case TokenType.PLUS:
					if (left is double && right is double) {
						return (double)left + (double)right;
					}
					if (left is string && right is string) {
						return (string)left + (string)right;
					}
					if (left is string && right is double) {
						return (string)left + right;
					}
					throw new RuntimeError(expr.op, "Invalid addition operands");
				case TokenType.MINUS:
					CheckNumberedOperands(expr.op, left, right);
					return (double)left - (double)right;
				case TokenType.SLASH:
					CheckNumberedOperands(expr.op, left, right);
#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
					if ((double)right == 0.0) throw new RuntimeError(expr.op, "RHS Operand cannot be zero");
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
					return (double)left / (double)right;
				case TokenType.STAR:
					CheckNumberedOperands(expr.op, left, right);
					return (double)left * (double)right;

				case TokenType.GREATER:
					CheckNumberedOperands(expr.op, left, right);
					return (double)left > (double)right;
				case TokenType.GREATER_EQUAL:
					CheckNumberedOperands(expr.op, left, right);
					return (double)left >= (double)right;
				case TokenType.LESS:
					CheckNumberedOperands(expr.op, left, right);
					return (double)left < (double)right;
				case TokenType.LESS_EQUAL:
					CheckNumberedOperands(expr.op, left, right);
					return (double)left <= (double)right;

				case TokenType.BANG_EQUAL:
					return !IsEqual(left, right);
				case TokenType.EQUAL_EQUAL:
					return IsEqual(left, right);
			}

			return null;
		}

		public object VisitGroupingExpr(Expr.Grouping expr) {
			return Evaluate(expr.expression);
		}

		public object VisitLiteralExpr(Expr.Literal expr) {
			return expr.value;
		}

		public object VisitUnaryExpr(Expr.Unary expr) {
			object right = Evaluate(expr.right);

			switch(expr.op.type) {
				case TokenType.MINUS:
					CheckNumberedOperands(expr.op, right);
					return -(double)right;
				case TokenType.BANG:
					return !IsTrue(right);
			}

			return null;
		}

		private object Evaluate(Expr expression) {
			return expression.Accept(this);
		}

		private bool IsTrue(object obj) {
			if (obj == null) return false;
			if (obj is bool) return (bool)obj;
			return true;
		}

		private bool IsEqual(object a, object b) {
			if (a == null && b == null) return true;
			if (a == null) return false;
			return a.Equals(b);
		}

		public class RuntimeError : Exception {
			public readonly Token token;

			public RuntimeError(Token token, string msg) : base(msg) {
				this.token = token;
			}
		}

		private void CheckNumberedOperands(Token op, params object[] operands) {
			if (operands.All(o => o is double)) return;
			throw new RuntimeError(op, "Operand(s) must be a number.");
		}

		private string Stringify(object obj) {
			if (obj == null) return "nil";

			if (obj is double) {
				string text = obj.ToString();
				if (text.EndsWith(".0")) {
					text = text.Substring(0, text.Length - 2);
				}
				return text;
			}

			return obj.ToString();
		}

	}
}
