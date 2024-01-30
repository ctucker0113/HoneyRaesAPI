using System;
namespace HoneyRaesAPI.Models
{
	public class ServiceTicket
	{
		public int ID { get; set; }
		public int CustomerID { get; set; }
		public int? EmployeeID { get; set; }
		public string Description { get; set; }
		public bool Emergency { get; set; }
		public DateTime? DateCompleted { get; set; }
		public Employee Employee { get; set; }
		public Customer Customer { get; set; }
	}
}

