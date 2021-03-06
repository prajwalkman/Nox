﻿using System;
using System.Linq;
using System.Collections.Generic;
namespace Nox {
	public struct VoidT {
		public override string ToString() {
			throw new Interpreter.RuntimeError(
				new Token(),
				string.Format("Stmt visitor returns should not be used."));
		}
	} // Workaound for IVisitor<void>

	public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<VoidT> {

		private Environment currentEnv = new Environment();
		public static readonly VoidT VoidT;

		public void Interpret(Expr expression) {
			try {
				object result = Evaluate(expression);
				Console.WriteLine(Stringify(result));
			} catch (RuntimeError e) {
				Nox.RuntimeError(e);
			}
		}

		public void Interpret(Stmt statement) {
			try {
				statement.Accept(this);
			} catch (RuntimeError e) {
				Nox.RuntimeError(e);
			}
		}

		public void ExecuteBlock(List<Stmt> statements, Environment environment) {
			Environment prevEnv = currentEnv;
			currentEnv = environment;
			foreach (Stmt statement in statements) {
				statement.Accept(this);
			}
			currentEnv = prevEnv;
		}

		#region StmtVisitors

		public VoidT VisitBlockStmt(Stmt.Block stmt) {
			ExecuteBlock(stmt.statements, new Environment(currentEnv));
			return VoidT;
		}

		public VoidT VisitClassStmt(Stmt.Class stmt) {
			return VoidT;
		}

		public VoidT VisitExpressionStmt(Stmt.Expression stmt) {
			Evaluate(stmt.expression);
			return VoidT;
		}

		public VoidT VisitFunctionStmt(Stmt.Function stmt) {
			currentEnv.Define(stmt.name, null);
			NoxFunction function = new NoxFunction(stmt, currentEnv, false);

			currentEnv.Assign(stmt.name, function);

			return VoidT;
		}

		public VoidT VisitIfStmt(Stmt.If stmt) {
			object condition = Evaluate(stmt.condition);
			if (IsTrue(condition)) {
				stmt.thenBranch.Accept(this);
			} else {
				stmt.elseBranch?.Accept(this);
			}
			return VoidT;
		}

		public VoidT VisitPrintStmt(Stmt.Print stmt) {
			Console.WriteLine(Evaluate(stmt.expression));
			return VoidT;
		}

		public VoidT VisitReturnStmt(Stmt.Return stmt) {
			object returnValue = null;
			if (stmt.value != null) {
				returnValue = Evaluate(stmt.value);
			}
			throw new ReturnException(returnValue);
			//return VoidT;
		}

		public VoidT VisitVarStmt(Stmt.Var stmt) {
			if (currentEnv.IsDeclared(stmt.name)) {
				throw new RuntimeError(
					stmt.name,
					string.Format("var '{0}' shadows existing var.", stmt.name.lexeme));
			}

			object val = null;
			if (stmt.initializer != null) {
				val = Evaluate(stmt.initializer);
			}
			currentEnv.Define(stmt.name, val);
			return VoidT;
		}

		public VoidT VisitWhileStmt(Stmt.While stmt) {
			while (IsTrue(Evaluate(stmt.condition))) {
				stmt.body.Accept(this);
			}
			return VoidT;
		}

		#endregion StmtVisitors

		#region ExprVisitors

		public object VisitAssignExpr(Expr.Assign expr) {
			object val = Evaluate(expr.value);
			currentEnv.Assign(expr.name, val);
			return val;
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

		public object VisitCallExpr(Expr.Call expr) {
			object callee = Evaluate(expr.callee);

			if (!(callee is ICallable)) {
				throw new RuntimeError(expr.paren, "Can only call functions.");
			}

			List<object> arguments = new List<object>();

			foreach (Expr argument in expr.arguments) {
				arguments.Add(Evaluate(argument));
			}

			ICallable function = (ICallable)callee;

			return function.Call(this, arguments);
		}

		public object VisitGetExpr(Expr.Get expr) {
			throw new NotImplementedException();
		}

		public object VisitGroupingExpr(Expr.Grouping expr) {
			return Evaluate(expr.expression);
		}

		public object VisitLiteralExpr(Expr.Literal expr) {
			return expr.value;
		}

		public object VisitLogicalExpr(Expr.Logical expr) {
			throw new NotImplementedException();
		}

		public object VisitSetExpr(Expr.Set expr) {
			throw new NotImplementedException();
		}

		public object VisitThisExpr(Expr.This expr) {
			throw new NotImplementedException();
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

		public object VisitVariableExpr(Expr.Variable expr) {
			object val = currentEnv.Get(expr.name);
			if (val == null) {
				throw new RuntimeError(
					expr.name, 
					string.Format("var '{0}' was declared but not defined.", expr.name.lexeme));
			}
			return currentEnv.Get(expr.name);
		}

		#endregion ExprVisitors

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
				if (text.EndsWith(".0", StringComparison.OrdinalIgnoreCase)) {
					text = text.Substring(0, text.Length - 2);
				}
				return text;
			}

			return obj.ToString();
		}

	}
}
