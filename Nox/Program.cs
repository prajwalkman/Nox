using System;

namespace Nox {
	class Program {
		static int Main (string[] args) {
			Console.WriteLine("Nox interpreter");

			if (args.Length > 1) {
				Console.WriteLine("Usage: nox [script]");
				return 1;
			}

			if (args.Length == 1) {
				return Nox.RunFile(args[0]);
			}

			Nox.RunPrompt();

			return 0;
		}

		static void AstPrinterTest() {
			Expr expression = new Expr.Binary(
				new Expr.Unary(
					new Token(TokenType.MINUS, "-", null, 1, 0),
					new Expr.Literal(123)
				),
				new Token(TokenType.STAR, "*", null, 1, 0),
				new Expr.Grouping(
					new Expr.Literal(45.67)
				)
			);

			Console.WriteLine(new Debug.AstPrinter().Print(expression));
		}
	}
}
