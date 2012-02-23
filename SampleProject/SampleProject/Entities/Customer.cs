using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SampleProject.Entities
{
	public class Customer
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual Address Address { get; set; }
		public virtual IList<Order> Orders { get; set; }
	}
}
