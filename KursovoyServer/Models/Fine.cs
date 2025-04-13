using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovoyServer.Models
{
    public class Fine
    {
        public int FineID { get; set; }
        public int StudentID { get; set; }
        public string Reason { get; set; }
        public string Discipline { get; set; }
        public decimal Amount { get; set; }
    }
}
