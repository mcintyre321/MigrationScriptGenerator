namespace SampleProject.Entities
{
	public class OrderItem
	{
		public virtual int Id{get;set;}
		public virtual Product Product { get; set; }
		public virtual decimal Cost { get; set; }
	}
}