using System;
namespace HoneyRaesAPI.Models
{
	public class Customer
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public string Address { get; set; }
        public List<ServiceTicket> ServiceTickets { get; set; }
    }
}

