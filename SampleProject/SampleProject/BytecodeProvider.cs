using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Bytecode;
using NHibernate.ByteCode.Castle;
using NHibernate.Bytecode.Lightweight;
using NHibernate.Properties;

namespace SampleProject
{
	public class BytecodeProvider : IBytecodeProvider
	{


		#region IBytecodeProvider Members

		public IReflectionOptimizer GetReflectionOptimizer(Type clazz, IGetter[] getters, ISetter[] setters)
		{
			return new ReflectionOptimizer(clazz, getters, setters);
		}

		public IProxyFactoryFactory ProxyFactoryFactory
		{
			get { return new ProxyFactoryFactory(); }
		}

		#endregion
	}
		
}
