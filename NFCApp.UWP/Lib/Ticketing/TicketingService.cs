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
        byte[] _cardID;
        NFCReader _ticketValidator;

        public TicketingService(NFCReader ticketValidator, byte[] cardID) 
        {
            _cardID = cardID;
            _ticketValidator = ticketValidator;
        }

        public void WriteTicket()
        {
            string encryptedTicket = string.Empty;
            SmartTicket ticket = new SmartTicket() // Sample ticket, I need to replace it with the curret object
            {
                Credit = 10.5,
                Type = SmartTicket.TicketType.BIT,
                CurrentValidation = DateTime.Now,
                SessionValidation = DateTime.Now.AddHours(-10),
                SessionExpense = 3.0,
                CardID = new byte[] { 0x04, 0x15, 0x91, 0x8A, 0xCB, 0x42, 0x20 }
            };            
            byte[] encryptedTicketBytes = TicketEncryption.EncryptTicket(ticket, TicketEncryption.GetPaddedIV(_cardID));
            _ticketValidator.WriteTextNDEFMessage(encryptedTicketBytes);
        }

        public SmartTicket ReadTicket()
        {
            return GetConnectedTicket();
        }

        public SmartTicket GetConnectedTicket()
        {
            NDEFPayload payload = _ticketValidator.GetNDEFPayload();
            SmartTicket ticket = TicketEncryption.DecryptTicket(payload.Bytes, _cardID);
            return ticket;
        }
    }
}
