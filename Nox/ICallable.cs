using System;
using System.Collections.Generic;
namespace Nox {
	public interface ICallable {
		int requiredArguments { get; }

		object Call(Interpreter interpreter, List<object> arguments);
	}
}
