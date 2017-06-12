using System;
using System.Collections.Generic;
using System.Linq;

namespace Nox {
	public class NoxFunction : ICallable {
		private readonly Stmt.Function declaration;
		private readonly Environment closure;
		private readonly bool isInitializer;

		public NoxFunction(Stmt.Function declaration, Environment closure, bool isInitializer) {
			this.declaration = declaration;
			this.closure = closure;
			this.isInitializer = isInitializer;
		}

		public override string ToString() {
			return declaration.name.lexeme;
		}

		public int requiredArguments {
			get {
				return declaration.parameters.Count;
			}
		}

		public NoxFunction Bind(NoxInstance instance) {
			Environment env = new Environment(closure);
			env.Bind(instance);
			return new NoxFunction(declaration, env, isInitializer);
		}

		public object Call(Interpreter interpreter, List<object> arguments) {
			object result = null;

			try {
				Environment environment = new Environment(closure);

				IEnumerable<KeyValuePair<Token, object>> parameters = 
					declaration.parameters.Zip(arguments,
						(a, b) => new KeyValuePair<Token, object>(a, b));

				foreach (var pair in parameters) {
					environment.Define(pair.Key, pair.Value);
				}

				interpreter.ExecuteBlock(declaration.body, environment);

			} catch (ReturnException e) {
				result = e.returnValue;
			}

			return result;
		}

	}
}
