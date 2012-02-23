namespace SampleProject.Entities
{
	public class Address
	{
		public virtual int Id { get; set; }
		public virtual string HouseNumberOrName { get; set; }
		public virtual string Street { get; set; }
		public virtual string TownOrArea { get; set; }
		public virtual string Country { get; set; }
	}
}