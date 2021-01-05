using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCApp.UWP.SmartCards
{
    public class MIFARE : Card
    {
        /// <summary>
        /// Commands: Class, INS, P1, P2, Le, Data, DataLength        
        /// </summary>
        private readonly byte[] GetUIDCommand = { 0xFF, 0xCA, 0x00, 0x00, 0x00 };
        private readonly byte[] LoadAuthenticationKeysCommand = { 0xFF, 0x82, 0x00, 0x00, 0x00 };
        private readonly byte[] ReadBinaryBlocksCommand = { 0xFF, 0xB0, 0x00, 0x00, 0x00 };
        private readonly byte[] ReadValueBlockCommand = { 0xFF, 0xB1, 0x00, 0x00, 0x04 };

        public MIFARE(IntPtr hCard) : base(hCard) { }

        protected override byte[] Get_GetUIDCommand()
        {
            return GetUIDCommand;
        }

        protected override byte[] Get_LoadAuthenticationKeysCommand()
        {
            return LoadAuthenticationKeysCommand;
        }

        protected override byte[] Get_ReadBinaryBlocksCommand(byte startingBlock, int length)
        {
            byte[] command = ReadBinaryBlocksCommand;
            command.SetValue(startingBlock, 3);
            command.SetValue((byte)length, 4);
            return command;
        }

        protected override byte[] Get_ReadValueBlockCommand(byte block)
        {
            byte[] command = ReadValueBlockCommand;
            command.SetValue(block, 3);            
            return command;
        }
    }
}
