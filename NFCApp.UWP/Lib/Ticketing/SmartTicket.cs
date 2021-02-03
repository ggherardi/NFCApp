using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ticketing
{
    public class SmartTicket
    {
        public double Credit { get; set; }
        public TicketType Type { get; set; }        
        public DateTime CurrentValidation { get; set; }
        public DateTime SessionValidation { get; set; }
        public double SessionExpense { get;  set; }
        public byte[] CardID { get; set; }

        public enum TicketType
        {
            BIT = 0x00,
            ROMA24 = 0x01,
            ROMA48 = 0x02,
            ROMA72 = 0x03,
            CIS = 0x04
        }
    }
}
