using System;
using System.Collections.Generic;
namespace Nox {
	public class NoxClass : ICallable {
		public readonly string name;
		private readonly List<NoxClass> parentClasses;
		private readonly Dictionary<string, NoxFunction> methods;
		
		public NoxClass(string name, List<NoxClass> parentClasses, 
		                Dictionary<string, NoxFunction> methods, List<Stmt.Var> fields) {
			this.name = name;
			this.parentClasses = parentClasses;
			this.methods = methods;
		}

		public NoxFunction FindMethod(NoxInstance instance, string name) {
			if (methods.ContainsKey(name)) {
				return methods[name].Bind(instance);
			}

			foreach (NoxClass klass in parentClasses) {
				NoxFunction method = klass.FindMethod(instance, name);
				if (method != null) return method;
			}

			return null;
		}

		public int requiredArguments {
			get {
				return 0;
			}
		}

		public object Call(Interpreter interpreter, List<object> arguments) {
			return null;
		}
	}
}
