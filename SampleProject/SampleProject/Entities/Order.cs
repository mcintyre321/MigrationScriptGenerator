using System;
using System.Collections.Generic;

namespace SampleProject.Entities
{
	public class Order
	{
		public virtual int Id { get; set; }
		public virtual DateTime Placed { get; set; }
		public virtual IList<OrderItem> Items {get;set;}
	}
}