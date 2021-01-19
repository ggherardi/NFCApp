using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC
{
    public class NFCOperation
    {
        private readonly NFCCommand _readerCommand;
        private readonly NFCCommand _controllerCommand;
        private readonly NFCCommand _cardCommand;
        private byte[] _wrappedCommand;

        public byte[] WrappedCommand { get => _wrappedCommand; }
        public byte[] ResponseBuffer { get; set; }
        public byte[] ResponsePayload { get; set; }
        public int Status { get; set; }

        public NFCOperation(NFCCommand readerCommand, NFCCommand controllerCommand, NFCCommand cardCommand, byte[] wrappedCommand)
        {
            _readerCommand = readerCommand;
            _controllerCommand = controllerCommand;
            _cardCommand = cardCommand;
            _wrappedCommand = wrappedCommand;
        }

        public NFCOperation(NFCCommand readerCommand) : this(readerCommand, null, null, readerCommand.Bytes) { }

        public NFCOperation() { }

        public void ElaborateResponse()
        {
            if(_readerCommand != null)
            {
                ResponsePayload = _readerCommand.ExtractPayload(ResponseBuffer);
            }            
            if(_controllerCommand != null)
            {
                ResponsePayload = _controllerCommand.ExtractPayload(ResponsePayload);
            }
            if (_cardCommand != null)
            {
                ResponsePayload = _cardCommand.ExtractPayload(ResponsePayload);
            }
        }
    }

}
