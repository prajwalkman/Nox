using System;
namespace Nox {
	// Shh, it's ok. Sleep now. The horror will pass. Lord Bytecode will save us.
	public class ReturnException : Exception {
		public readonly object returnValue;
		public ReturnException(object returnValue) : base(null, null) {
			this.returnValue = returnValue;
		}
	}
}
