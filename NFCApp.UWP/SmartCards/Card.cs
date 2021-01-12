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
        
        protected abstract int MaxWritableBlocks { get; set; }
        protected abstract int MaxReadableBytes { get; set; }

        protected abstract byte[] Get_GetUIDCommand();
        protected abstract byte[] Get_LoadAuthenticationKeysCommand();
        protected abstract byte[] Get_ReadBinaryBlocksCommand(byte startingBlock, int length);
        protected abstract byte[] Get_ReadValueBlockCommand(byte block);
        protected abstract byte[] Get_UpdateBinaryBlockCommand(byte blockNumber, byte[] dataIn, int numberOfBytesToUpdate);

        public CardResponse Transmit(byte[] command, int responseBufferLength = 255)
        {
            CardResponse response = new CardResponse();
            byte[] responseBuffer = new byte[responseBufferLength];
            int responseLength = responseBuffer.Length;
            response.Status = Winscard.SCardTransmit(CardNumber, ref _standardRequest, ref command[0], command.Length, ref _standardRequest, ref responseBuffer[0], ref responseLength);
            response.ResponseBuffer = responseBuffer;
            return response;
        }

        public CardResponse GetGuid()
        {
            return Transmit(Get_GetUIDCommand());
        }

        public CardResponse ReadBlocks(byte startingBlock, int length)
        {
            return Transmit(Get_ReadBinaryBlocksCommand(startingBlock, length));
        }

        public CardResponse ReadBlocks(byte startingBlock)
        {
            return ReadBlocks(startingBlock, MaxReadableBytes);
        }

        public CardResponse ReadValue(byte block)
        {
            return Transmit(Get_ReadValueBlockCommand(block));
        }

        public CardResponse WriteBlocks(byte blockNumber, byte[] dataIn, int numberOfBytesToUpdate)
        {
            return Transmit(Get_UpdateBinaryBlockCommand(blockNumber, dataIn, numberOfBytesToUpdate));
        }

        public CardResponse WriteBlocks(byte blockNumber, byte[] dataIn)
        {            
            return WriteBlocks(blockNumber, dataIn, MaxWritableBlocks);
        }                

        public void WriteNDEFMessage(string value, int startingBlock)
        {
            NDEFMessageTLV message = new NDEFMessageTLV(value);
            byte[] blockBytes = message.GetFormattedBlock();
            int j = 0;
            for (int i = 0; i < blockBytes.Length; i += 4)
            {
                WriteBlocks((byte)(startingBlock + j), blockBytes.Skip(j * MaxWritableBlocks).Take(MaxWritableBlocks).ToArray());
                j++;
            }            
        }

        public void WriteNDEFMessage(string value)
        {
            WriteNDEFMessage(value, 4);
        }

        public CardResponse TestOperation()
        {
            //return Transmit(new byte[] { 0xFF, 0x00, 0x00, 0x00, 0x04, 0xD4, 0x4A, 0x01, 0x00 }); POLLING: pag. 26 API-ACR122USAM-2.01.pdf
            return Transmit(new byte[] { 0xFF, 0x00, 0x00, 0x00, 0x05, 0xD4, 0x40, 0x01, 0x30, 0x04 });
        }
    }
    
    public class CardResponse
    {
        public int Status { get; set; }
        public byte[] ResponseBuffer { get; set; }
    }
}
