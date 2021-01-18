using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC
{
    public class NFCCommand
    {
        public virtual byte[] Bytes { get; set; }
        public virtual byte[] ResponseHeaderBytes { get; set; }
        public virtual NFCCommandStatus CommandStatus { get; set; }
        public virtual Func<byte[], byte[]> ExtractPayload { get => (responseBuffer) => { return responseBuffer; }; set => ExtractPayload = value; }

        public NFCCommand(byte[] commandBytes, byte[] responseHeaderBytes)
        {
            Bytes = commandBytes;
            ResponseHeaderBytes = responseHeaderBytes;
            CommandStatus = NFCCommandStatus.Ready;
        }

        public NFCCommand(NFCCommand command) : this(command.Bytes, command.ResponseHeaderBytes) { }

        public NFCCommand() : this(new byte[] { }, new byte[] { }) { }

        public void ConcatBytesToCommand(byte[] bytesToAdd)
        {
            Bytes = Bytes.Concat(bytesToAdd).ToArray();
        }
    }

    public enum NFCCommandStatus
    {
        Success,
        Error,
        Ready
    }
}
