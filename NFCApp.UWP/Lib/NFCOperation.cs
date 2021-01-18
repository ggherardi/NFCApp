using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC
{
    public class NFCOperation
    {
        private readonly NFCCommand _controllerCommand;
        private readonly NFCCommand _readerCommand;
        private readonly NFCCommand _cardCommand;

        public int Status { get; set; }
        public byte[] ResponseBuffer { get; set; }
        public byte[] ResponsePayload { get; set; }

        public NFCOperation(NFCCommand readerCommand, NFCCommand cardCommand, NFCCommand controllerCommand) 
        {
            _controllerCommand = controllerCommand;
            _readerCommand = readerCommand;
            _cardCommand = cardCommand;
        }

        public NFCOperation(NFCCommand readerCommand, NFCCommand cardCommand) : this(readerCommand, cardCommand, null) { }

        public NFCOperation(NFCCommand readerCommand) : this(readerCommand, null, null) { }

        public NFCOperation() { }

        public void ElaborateResponse()
        {
            if(_controllerCommand != null)
            {
                ResponsePayload = _controllerCommand.ExtractPayload(ResponseBuffer);
            }            
            if(_readerCommand != null)
            {
                ResponsePayload = _readerCommand.ExtractPayload(ResponsePayload);
            }
            if (_cardCommand != null)
            {
                ResponsePayload = _cardCommand.ExtractPayload(ResponsePayload);
            }
        }
    }

}
