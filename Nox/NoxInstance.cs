using System;
using System.Collections.Generic;
namespace Nox {
	public class NoxInstance {
		private readonly NoxClass klass;
		private Dictionary<string, object> fields = new Dictionary<string, object>();

		public NoxInstance(NoxClass klass) {
			this.klass = klass;
		}

		public object GetProperty(Token name) {
			if (fields.ContainsKey(name.lexeme)) {
				return fields[name.lexeme];
			}

			NoxFunction method = klass.FindMethod(this, name.lexeme);
			if (method != null) return method;

			throw new Interpreter.RuntimeError(name, 
               string.Format("Property {0} not defined.", name.lexeme));
		}

		public override string ToString() {
			return klass.name + " [NoxInstance]";
		}
	}
}
