using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCApp.UWP.SmartCards
{
    public abstract class Card
    {
        private Winscard.SCARD_IO_REQUEST _standardRequest;
        private IntPtr _cardNumber;
        public int CardNumber { get { return _cardNumber.ToInt32(); } }

        public Card(IntPtr hCard) 
        {
            _cardNumber = hCard;
            _standardRequest = new Winscard.SCARD_IO_REQUEST() { dwProtocol = Winscard.SCARD_PROTOCOL_T1, cbPciLength = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Winscard.SCARD_IO_REQUEST)) };
        }

        protected abstract byte[] Get_GetUIDCommand();
        protected abstract byte[] Get_LoadAuthenticationKeysCommand();
        protected abstract byte[] Get_ReadBinaryBlocksCommand(byte startingBlock, int length);
        protected abstract byte[] Get_ReadValueBlockCommand(byte block);

        public CardResponse Transmit(byte[] command)
        {
            CardResponse response = new CardResponse();
            byte[] responseBuffer = new byte[255];
            int responseLength = responseBuffer.Length;
            response.Status = Winscard.SCardTransmit(CardNumber, ref _standardRequest, ref command[0], command.Length, ref _standardRequest, ref responseBuffer[0], ref responseLength);
            response.ResponseBuffer = responseBuffer;
            return response;
        }

        public CardResponse GetGuid()
        {
            return Transmit(Get_GetUIDCommand());
        }

        public CardResponse ReadBlocks(byte startingBlock, int length = 16)
        {
            return Transmit(Get_ReadBinaryBlocksCommand(startingBlock, length));
        }

        public CardResponse ReadValue(byte block)
        {
            return Transmit(Get_ReadValueBlockCommand(block));
        }
    }

    public class CardResponse
    {
        public int Status { get; set; }
        public byte[] ResponseBuffer { get; set; }
    }
}
