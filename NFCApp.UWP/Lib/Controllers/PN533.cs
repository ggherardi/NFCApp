using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.Controllers
{
    public class PN533 : NFCController
    {
        /// <summary>
        /// Input | Class: 0xD4 (1 byte) - Instruction: 0x42 (1 byte) - Data out [] (n bytes)
        /// Response | 0xD5 (1 byte), 0x43 (1 byte), Status (1 byte) (errors: UM0801-03 (PN533 User Manual), chapter 8.1. Error handling, pag. 50), DataIn[] (array of raw data)
        /// In order to send a direct transmit to the card, one should a combination of commands, i.e.: _reader.DirectTransmitCommand(payload:{_controller.ModuleDataExchangeCommand(DataOut:{_card.PWD_AUTH(value:{password})})})
        /// Reference: UM0801-03 (PN533 User Manual), chapter 8.4.9. InCommunicateThru, pag. 109
        /// </summary>
        private readonly PN533Command _dataExchangeCommand = new PN533Command()
        {
            Bytes = new byte[] { 0xD4, 0x42 },
            ResponseHeaderBytes = new byte[] { 0xD5, 0x43, }
        };

        protected override NFCCommand Get_DataExchangeCommand(byte[] payload)
        {
            PN533Command command = new PN533Command(_dataExchangeCommand);
            command.ConcatBytesToCommand(payload);
            return command;
        }

        public enum ErrorMessages
        {
            [Description("Time Out, the target has not answered")]
            TimeOut = 0x01,
            [Description("A CRC Error has been detected by the CIU")]
            CRCError = 0x02,
            [Description("A Parity error has been detected by the CIU")]
            PairtyError = 0x03,
            [Description("Time Out, the target has not answered")]
            TimeOut = 0x04,
            [Description("Time Out, the target has not answered")]
            TimeOut = 0x05,
            [Description("Time Out, the target has not answered")]
            TimeOut = 0x06,
            [Description("Time Out, the target has not answered")]
            TimeOut = 0x07,
            [Description("Time Out, the target has not answered")]
            TimeOut = 0x09
        }
    }

    public class PN533Command : NFCCommand
    {
        public override Func<byte[], byte[]> ExtractPayload 
        {
            get => _extractPayload; 
            set => base.ExtractPayload = value; 
        }

        public PN533Command(PN533Command command) : base(command) { }

        public PN533Command() : base() { }

        private byte[] _extractPayload(byte[] responseBuffer)
        {
            if(responseBuffer[0] == ResponseHeaderBytes[0] && responseBuffer[1] == ResponseHeaderBytes[1])
            {
                int status = responseBuffer[2];
            }
            return base.ExtractPayload(responseBuffer);
        }
    }
}
