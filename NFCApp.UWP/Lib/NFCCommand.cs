using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC
{
    public class NFCCommand
    {
        private Func<byte[], NFCPayload> _extractPayload = (responseBuffer) => { return new NFCPayload(responseBuffer); };
        public virtual byte[] Bytes { get; set; }
        public virtual byte[] ResponseHeaderBytes { get; set; }
        public virtual NFCCommandStatus CommandStatus { get; set; }
        public NFCPayload Payload { get; set; }
        public Func<byte[], NFCPayload> ExtractPayload { get => _extractPayload; set => _extractPayload = value; }

        public NFCCommand(byte[] commandBytes, byte[] responseHeaderBytes)
        {
            Bytes = commandBytes;
            ResponseHeaderBytes = responseHeaderBytes;
            CommandStatus = new NFCCommandStatus() { };
        }

        public NFCCommand(NFCCommand commandToClone) : this(commandToClone.Bytes, commandToClone.ResponseHeaderBytes) { }

        public NFCCommand() : this(new byte[] { }, new byte[] { }) { }

        public void ConcatBytesToCommand(byte[] bytesToAdd)
        {
            Bytes = Bytes.Concat(bytesToAdd).ToArray();
        }
    }

    public class NFCCommandStatus
    {
        public string Result { get; set; }
        public string Message { get; set; }

        public enum Status
        {
            [Description("Operation successful")]
            Success = 0x00,
            [Description("Header mismatch in response buffer")]
            HeaderMismatch = 0x01,
            [Description("The command encountered an error")]
            GenericError = 0x02
        }
    }
}