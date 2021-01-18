using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC
{
    public class NFCOperation
    {
        private Command _controllerCommand;
        private Command _readerCommand;
        private Command _cardCommand;

        public int Status { get; set; }
        public byte[] ResponseBuffer { get; set; }
        public byte[] ResponsePayload { get; set; }

        public void ElaborateResponse() 
        {

        }
    }

    public class Command
    {
        public byte[] Command { get; set; }
        public byte[] ResponseHeader { get; set; }
        protected NFCCommandStatus CommandStatus { get; set; }

        public byte[] ElaborateResponse(byte[] responseBuffer)
        {
            byte[] strippedBuffer = new byte[] { };
            return strippedBuffer;
        }
    }

    public enum NFCCommandStatus
    {
        Success,
        Error
    }
}
