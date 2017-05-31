using System;
using System.IO;
using System.Collections.Generic;

using Nox.Frontend;

namespace Nox {
	public static class Nox {
		private static bool hadError = false;
		private static bool hadRuntimeError = false;
		private static bool replExitRequested = false;

		private static Interpreter interpreter = new Interpreter();

		private static void Run(string Source) {
			Scanner scanner = new Scanner(Source);
			List<Token> tokens = scanner.ScanTokens();
			Parser parser = new Parser(tokens);
			Expr expression = parser.Parse();

			if (!hadError) {
				//Console.WriteLine(new Debug.AstPrinter().Print(expression));
				interpreter.Interpret(expression);
			}
		}

		public static int RunFile(string Path) {
			string code = File.ReadAllText(Path);
			Run(code);

			if (hadError) {
				return 65;
			}
			if (hadRuntimeError) {
				return 70;
			}
			return 0;
		}

		public static void RunPrompt() {
			while (!replExitRequested) {
				hadError = false;
				Console.Write("> ");
				string input = Console.ReadLine();
				// Handle edge case : input[0] == '\0'
				if (input[0] == '\0') {
					input = input.Substring(1, input.Length - 1);
				}
				Run(input);
			}
		}

		public static void RuntimeError(Interpreter.RuntimeError error) {
			string msg = string.Format("{0}\n[line {1}]", error, error.token.line);
			Console.WriteLine(msg);
			hadRuntimeError = true;
		}

		public static void Error(int Line, int Column, string Message) {
			Report(Line, Column, "", Message);
		}

		public static void Error(Token token, string msg) {
			if (token.type == TokenType.EOF) {
				Report(token.line, token.column, "at end", msg);
			} else {
				Report(token.line, token.column, string.Format("at '{0}'", token.lexeme), msg);
			}
		}

		private static void Report(int Line, int Column, string Where, string Message) {
			string msg = string.Format("[line {0}|{1}] Error {2}: {3}", Line, Column, Where, Message);
			Console.WriteLine(msg);
			hadError = true;
		}

	}
}
