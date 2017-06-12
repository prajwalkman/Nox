using System;
using System.Collections.Generic;
namespace Nox {
	public class Environment {
		private Dictionary<string, object> values = new Dictionary<string, object>();
		private readonly Environment parentEnv;

		public Environment() {
			parentEnv = null;
		}

		public Environment(Environment enclosing) {
			this.parentEnv = enclosing;
		}

		public void Bind(NoxInstance instance) {
			values.Add("this", instance);
		}

		public void Define(Token ident, object val) {
			string name = ident.lexeme;
			if (values.ContainsKey(name)) {
				throw new Interpreter.RuntimeError(
					ident,
					string.Format("Previously defined variable {0}", name));
			}
			values[name] = val;
		}

		public void Assign(Token ident, object val) {
			string name = ident.lexeme;
			if (values.ContainsKey(name)) {
				values[name] = val;
				return;
			}

			if (parentEnv != null) {
				parentEnv.Assign(ident, val);
				return;
			}

			throw new Interpreter.RuntimeError(
				ident,
				string.Format("Undefine variable {0}", name));
		}

		public bool IsDeclared(Token ident) {
			string name = ident.lexeme;
			if (values.ContainsKey(name)) {
				return true;
			}

			if (parentEnv != null) {
				return parentEnv.IsDeclared(ident);
			}

			return false;
		}

		public object Get(Token ident) {
			string name = ident.lexeme;
			if (values.ContainsKey(name)) {
				return values[name];
			}

			if (parentEnv != null) {
				return parentEnv.Get(ident);
			}

			throw new Interpreter.RuntimeError(
				ident,
				string.Format("Undefined variable {0}", name));
		}

	}
}
