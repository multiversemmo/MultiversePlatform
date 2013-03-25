using System;
using System.Reflection;
using System.IO;

namespace Axiom.Core
{
    /// <summary>
    /// Used by configuration classes to store assembly/class names and instantiate
    /// objects from them.
    /// </summary>
    public class ObjectCreator
    {
		public Type type;
        public string assemblyName;
        public string className;

		
		public ObjectCreator(Type type) 
		{
			this.type = type;
		}
        public ObjectCreator(string assemblyName, string className) {
            this.assemblyName = assemblyName;
            this.className = className;
        }
        public Assembly GetAssembly() {
			if(type != null)
				return type.Assembly;
            string assemblyFile = Path.Combine(Environment.CurrentDirectory, assemblyName);

            // load the requested assembly
            return Assembly.LoadFrom(assemblyFile);
        }
		public new Type GetType() 
		{
			if(type != null)
				return type;
			if(className == null)
				throw new InvalidOperationException("Cannot get the type from an assembly when the class name is null.");
            return GetAssembly().GetType(className);
        }
        public object CreateInstance() {
            return Activator.CreateInstance(GetType());
        }
    }
}
