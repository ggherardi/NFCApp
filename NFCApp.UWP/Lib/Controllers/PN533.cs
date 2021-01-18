using System;
using System.Collections.Generic;
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
        //private byte[] DataExchangeCommand { get => new byte[] { 0xD4, 0x42 }; }
        private PN533Command _dataExchange = new PN533Command()
        {
            Command = new byte[] { 0xD4, 0x42 },
            ResponseHeader = new byte[] { 0xD5, 0x43, }
        };

        protected override byte[] Get_DataExchangeCommand(byte[] payload)
        {
            byte[] command = _dataExchange.Command;
            return command.Concat(payload).ToArray();
        }
    }

    public class PN533Command : Command
    {
        //public byte[] GetPayload(byte[] responseBuffer)
        //{
        //    ResponseHeader
        //}
    }
}
