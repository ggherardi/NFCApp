using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.Cards
{
    public class Ntag215 : NFCCard
    {
        /// <summary>
        /// Cmd: 0x1B (1 byte) - Pwd: {password} (4 bytes)
        /// Reference: NTAG213/215/216, chapter: 10.7. PWD_AUTH, pag. 46
        /// </summary>
        private Ntag215Command PWD_AUTH = new Ntag215Command()
        {
            Bytes = new byte[] { 0x1B, 0x00, 0x00, 0x00, 0x00 }
        };

        /// <summary>
        /// Cmd: 0x60 (1 byte)
        /// Reference: NTAG213/215/216, chapter: 10.1. GET_VERSION, pag. 34
        /// </summary>
        private Ntag215Command GET_VERSION = new Ntag215Command()
        {
            Bytes = new byte[] { 0x60 }
        };

        public Ntag215(IntPtr hCard) : base(hCard) { }

        public Ntag215() : base() { }

        public override int MaxWritableBlocks { get => 4; protected set => MaxWritableBlocks = value; }
        public override int MaxReadableBytes { get => 16; protected set => MaxReadableBytes = value; }

        protected override NFCCommand Get_PWD_AUTH_Command(string password)
        {
            Ntag215Command command = new Ntag215Command(PWD_AUTH);
            byte[] passwordBytes = Encoding.Default.GetBytes(password);
            for(int i = 0; i < 4; i++)
            {
                command.Bytes.SetValue(passwordBytes[i], i);
            }            
            return command;
        }

        protected override NFCCommand Get_GET_VERSION_Command()
        {
            return new NFCCommand(GET_VERSION);
        }
    }

    public class Ntag215Command : NFCCommand
    {
        public Ntag215Command(Ntag215Command commandToClone) : base(commandToClone) { }
        public Ntag215Command() : base() { }
    }
}
