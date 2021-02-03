using CSharp.NFC;
using CSharp.NFC.NDEF;
using CSharp.NFC.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ticketing.Encryption;

namespace Ticketing
{
    public class TicketingService
    {
        NFCReader _ticketValidator;

        public TicketingService(NFCReader ticketValidator) 
        {
            _ticketValidator = ticketValidator;
        }

        public SmartTicket GetConnectedTicket()
        {
            SmartTicket ticket = null;
            NDEFPayload payload = _ticketValidator.GetNDEFPayload();
            ticket = TicketEncryption.DecryptTicket(payload.Bytes);
            return ticket;
        }
    }
}
