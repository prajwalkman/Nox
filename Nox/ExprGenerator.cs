﻿
using System;

namespace Nox {
	public abstract class Expr {
		public abstract R Accept<R>(IVisitor<R> visitor);

		public interface IVisitor<R> {
				R VisitBinaryExpr(Binary expr);
				R VisitGroupingExpr(Grouping expr);
				R VisitLiteralExpr(Literal expr);
				R VisitUnaryExpr(Unary expr);
		}

		public class Binary : Expr {
			public readonly Expr left;
			public readonly Token op;
			public readonly Expr right;
						
			public Binary(Expr left, Token op, Expr right) {
				this.left = left;
				this.op = op;
				this.right = right;
			}

			public override R Accept<R>(IVisitor<R> visitor) {
				return visitor.VisitBinaryExpr(this);
			}
		}

		public class Grouping : Expr {
			public readonly Expr expression;
						
			public Grouping(Expr expression) {
				this.expression = expression;
			}

			public override R Accept<R>(IVisitor<R> visitor) {
				return visitor.VisitGroupingExpr(this);
			}
		}

		public class Literal : Expr {
			public readonly Object value;
						
			public Literal(Object value) {
				this.value = value;
			}

			public override R Accept<R>(IVisitor<R> visitor) {
				return visitor.VisitLiteralExpr(this);
			}
		}

		public class Unary : Expr {
			public readonly Token op;
			public readonly Expr right;
						
			public Unary(Token op, Expr right) {
				this.op = op;
				this.right = right;
			}

			public override R Accept<R>(IVisitor<R> visitor) {
				return visitor.VisitUnaryExpr(this);
			}
		}

	}
}
