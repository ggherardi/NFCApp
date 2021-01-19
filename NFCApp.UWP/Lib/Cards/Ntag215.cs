using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.Cards
{
    public class Ntag215 : NFCCard
    {
        #region Properties
        public override int MaxWritableBlocks { get => 4; protected set => MaxWritableBlocks = value; }
        public override int MaxReadableBytes { get => 16; protected set => MaxReadableBytes = value; }
        #endregion

        #region Commands
        /// <summary>
        /// Cmd: 0x1B (1 byte) - Pwd: {password} (4 bytes)
        /// Reference: NTAG213/215/216, chapter: 10.7. PWD_AUTH, pag. 46
        /// </summary>
        private Ntag215Command PWD_AUTH = new Ntag215Command()
        {
            Bytes = new byte[] { 0x1B, 0x00, 0x00, 0x00, 0x00 }
        };

        /// <summary>
        /// Input | Cmd: 0x60 (1 byte)
        /// Response | 0x00 (1 byte), {vendorID} (1 byte), {productType} (1 byte), {productSubtype} (1 byte), {majorProductVersion} (1 byte), {minorProductVersion} (1 byte), {storageSize} (1 byte), {procotolType} (1 byte)
        /// Reference: NTAG213/215/216, chapter: 10.1. GET_VERSION, pag. 34
        /// </summary>
        private readonly Lazy<Ntag215Command> _lazyGET_VERSION = new Lazy<Ntag215Command>(() =>
        {
            Ntag215Command command = new Ntag215Command()
            {
                Bytes = new byte[] { 0x60 },
                Response = new NFCCommandResponse() { HeaderBytes = new byte[] { 0x00 }, MinBufferLength = 8 }
            };
            command.ExtractPayload = (responseBuffer) =>
            {
                byte[] payloadBytes = new byte[responseBuffer.Length];
                byte storageSizeByte = responseBuffer[6];
                string cardType = string.Empty;

                if (responseBuffer[0] != command.Response.HeaderBytes[0])
                {
                    command.Response.SetCommandStatus(NFCCommandStatus.Status.HeaderMismatch);
                }
                Array.Copy(responseBuffer, 1, payloadBytes, 0, responseBuffer.Length - 1);

                switch (storageSizeByte)
                {
                    case 0x0F:
                        cardType = "NTAG213";
                        break;
                    case 0x11:
                        cardType = "NTAG215";
                        break;
                    case 0x13:
                        cardType = "NTAG216";
                        break;
                    default: break;
                }

                return new NFCPayload(payloadBytes, cardType);
            };
            return command;
        });
        private Ntag215Command GET_VERSION { get => _lazyGET_VERSION.Value; }
        #endregion

        #region Constructors
        public Ntag215(IntPtr hCard) : base(hCard) { }

        public Ntag215() : base() { }
        #endregion

        #region Implementation
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
            Ntag215Command get_version = GET_VERSION;
            return get_version;
        }
        #endregion
    }

    public class Ntag215Command : NFCCommand
    {
        public Ntag215Command(Ntag215Command commandToClone) : base(commandToClone) { }
        public Ntag215Command() : base() { }
    }
}
