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
        public byte[] ResponsePayloadBuffer { get; set; }
        public string ResponsePayloadText { get; set; }
        public int Status { get; set; }
        private NFCOperationType OperationType { get; set; } 

        public enum NFCOperationType
        {
            ReaderOperation,
            CardOperation
        }

        public NFCOperation(NFCCommand readerCommand, NFCCommand controllerCommand, NFCCommand cardCommand, byte[] wrappedCommand, NFCOperationType operationType = NFCOperationType.CardOperation )
        {
            _readerCommand = readerCommand;
            _controllerCommand = controllerCommand;
            _cardCommand = cardCommand;
            _wrappedCommand = wrappedCommand;
            OperationType = NFCOperationType.CardOperation;
        }

        public NFCOperation(NFCCommand readerCommand) : this(readerCommand, null, null, readerCommand.Bytes, NFCOperationType.ReaderOperation) { }

        public NFCOperation() { }

        public void ElaborateResponse()
        {
            NFCPayload readerPayload = null;
            NFCPayload controllerPayload = null;
            NFCPayload cardPayload = null;
            if (_readerCommand != null)
            {
                readerPayload = _readerCommand.ExtractPayload(ResponseBuffer);
                _readerCommand.Payload = readerPayload;
                if (OperationType == NFCOperationType.ReaderOperation)
                {
                    ResponsePayloadBuffer = readerPayload.PayloadBytes;
                    ResponsePayloadText = readerPayload.PayloadText;
                }
            }            
            if(_controllerCommand != null)
            {
                controllerPayload = _controllerCommand.ExtractPayload(readerPayload.PayloadBytes);
                _controllerCommand.Payload = controllerPayload;
            }
            if (_cardCommand != null)
            {
                cardPayload = _cardCommand.ExtractPayload(controllerPayload.PayloadBytes);
                _cardCommand.Payload = cardPayload;
                if (OperationType == NFCOperationType.CardOperation)
                {
                    ResponsePayloadBuffer = cardPayload.PayloadBytes;
                    ResponsePayloadText = cardPayload.PayloadText;
                }
            }
        }
    }

    public class NFCPayload
    {
        public byte[] PayloadBytes { get; set; }
        public string PayloadText { get; set; }

        public NFCPayload(byte[] payload, string payloadText)
        {
            PayloadBytes = payload;
            PayloadText = payloadText;
        }

        public NFCPayload(byte[] payload) : this(payload, string.Empty) { }

        public NFCPayload() : this(new byte[] { }) { }
    }
}
