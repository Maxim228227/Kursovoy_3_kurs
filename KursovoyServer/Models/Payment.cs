using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovoyServer.Models
{
    public class Payment
    {
        public int PaymentID { get; set; }
        public int StudentID { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
