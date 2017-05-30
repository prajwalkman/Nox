using System;
using System.IO;
using System.Collections.Generic;

using Nox.Frontend;

namespace Nox {
	public static class Nox {
		private static bool hadError = false;

		private static void Run(string Source) {
			Scanner scanner = new Scanner(Source);
			List<Token> tokens = scanner.ScanTokens();

			foreach (Token token in tokens) {
				Console.WriteLine(token);
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
			for (;;) {
				Console.Write("> ");
				Run(Console.ReadLine());
			}
			throw new NotSupportedException();
		}

		public static void Error(int Line, string Message) {
			Report(Line, "", Message);
		}

		private static void Report(int Line, string Where, string Message) {
			string msg = string.Format("[line {0}] Error {1}: {2}", Line, Where, Message);
			hadError = true;
		}

	}
}
