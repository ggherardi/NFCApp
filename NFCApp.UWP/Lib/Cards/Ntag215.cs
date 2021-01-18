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
        private byte[] PWD_AUTH { get => new byte[] { 0x1B, 0x00, 0x00, 0x00, 0x00 }; }

        /// <summary>
        /// Cmd: 0x60 (1 byte)
        /// Reference: NTAG213/215/216, chapter: 10.1. GET_VERSION, pag. 34
        /// </summary>
        private byte[] GET_VERSION { get => new byte[] { 0x60 }; }

        public Ntag215(IntPtr hCard) : base(hCard) { }

        public Ntag215() : base() { }

        public override int MaxWritableBlocks { get => 4; protected set => MaxWritableBlocks = value; }
        public override int MaxReadableBytes { get => 16; protected set => MaxReadableBytes = value; }

        protected override byte[] Get_PWD_AUTH_Command(string password)
        {
            byte[] command = PWD_AUTH;
            byte[] passwordBytes = Encoding.Default.GetBytes(password);
            for(int i = 0; i < 4; i++)
            {
                command.SetValue(passwordBytes[i], i);
            }            
            return command;
        }

        protected override byte[] Get_GET_VERSION_Command()
        {
            return GET_VERSION;
        }
    }
}
