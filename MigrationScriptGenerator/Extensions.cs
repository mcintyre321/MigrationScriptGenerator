using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace DatabaseMigrationManager
{
	public static class Extensions
	{
		public static IEnumerable<string> Enumerate(this StringCollection collection)
		{
			foreach(var s in collection)
			{
				yield return s;
			}
		}
		public static void Raise<T>(this EventHandler<T> handler, object sender, T args)
			where T : EventArgs
		{
			EventHandler<T> evt = handler;
			if (evt != null) evt(sender, args);
		}
	}
}