using System;
using System.IO;
using System.Collections.Generic;

using Nox.Frontend;

namespace Nox {
	public static class Nox {
		private static bool hadError = false;
		private static bool replExitRequested = false;

		private static void Run(string Source) {
			Scanner scanner = new Scanner(Source);
			List<Token> tokens = scanner.ScanTokens();
			Parser parser = new Parser(tokens);
			Expr expression = parser.Parse();

			if (!hadError) {
				Console.WriteLine(new Debug.AstPrinter().Print(expression));
			}
		}

		public static int RunFile(string Path) {
			string code = File.ReadAllText(Path);
			Run(code);

			if (hadError) {
				return 65;
			}
			return 0;
		}

		public static void RunPrompt() {
			while (!replExitRequested) {
				hadError = false;
				Console.Write("> ");
				Run(Console.ReadLine());
			}
		}

		public static void Error(int Line, string Message) {
			Report(Line, "", Message);
		}

		public static void Error(Token token, string msg) {
			if (token.type == TokenType.EOF) {
				Report(token.line, "at end", msg);
			} else {
				Report(token.line, string.Format("at '{0}'", token.lexeme), msg);
			}
		}

		private static void Report(int Line, string Where, string Message) {
			string msg = string.Format("[line {0}] Error {1}: {2}", Line, Where, Message);
			Console.WriteLine(msg);
			hadError = true;
		}

	}
}
