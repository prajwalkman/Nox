﻿using System;
using System.IO;
using System.Collections.Generic;

using Nox.Frontend;

namespace Nox {
	public static class Nox {
		private static bool hadError = false;
		private static bool hadRuntimeError = false;
		private static bool replExitRequested = false;

		private static bool astPrinterMode = true;

		private static Interpreter interpreter = new Interpreter();

		private static void Run(string source) {
			Lexer lexer = new Lexer(source);
			List<Token> tokens = lexer.ScanTokens();
			Parser parser = new Parser(tokens);
			List<Stmt> statements = parser.Parse();

			if (!hadError) {
				foreach (Stmt statement in statements) {
					if (astPrinterMode) {
						Console.WriteLine(new Debug.AstPrinter().Print(statement));
					} else {
						interpreter.Interpret(statement);
					}
				}
			}
		}

		public static int RunFile(string path) {
			string code = File.ReadAllText(path);
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
			string msg = string.Format("{0}\n[line {1}|{2}]", error, error.token.line, error.token.column);
			Console.WriteLine(msg);
			hadRuntimeError = true;
		}

		public static void Error(int line, int column, string msg) {
			Report(line, column, "", msg);
		}

		public static void Error(Token token, string msg) {
			if (token.type == TokenType.EOF) {
				Report(token.line, token.column, "at end", msg);
			} else {
				Report(token.line, token.column, string.Format("at '{0}'", token.lexeme), msg);
			}
		}

		private static void Report(int line, int column, string context, string msg) {
			string report = string.Format("[line {0}|{1}] Error {2}: {3}", line, column, context, msg);
			Console.WriteLine(report);
			hadError = true;
		}

	}
}
