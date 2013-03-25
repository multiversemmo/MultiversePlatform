using System;
using System.Collections.Generic;
using System.Text;

namespace Axiom.Core {
    public abstract class FactoryObj<T> {
        public abstract T CreateInstance(string name);
        public abstract void DestroyInstance(T obj);

        public abstract string Type {
            get;
        }
    }
}
