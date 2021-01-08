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
        /// Class: 0xFF (1 byte) - Instruction: 0xCA (1 byte) - P1: 0x00 (1 byte) - P2: 0x00 (1 byte) - Le: 0x00 (1 byte)
        /// </summary>
        private byte[] GetUIDCommand { get => new byte[] { 0xFF, 0xCA, 0x00, 0x00, 0x00 }; }
        /// <summary>
        /// There should be no need for this for MIFARE UltraLight, I'll manage the ATAC ticket protection by crypting the data
        /// </summary>
        private byte[] LoadAuthenticationKeysCommand { get => new byte[] { 0xFF, 0x82, 0x00, 0x00, 0x00 }; }
        /// <summary>
        /// Class: 0xFF (1 byte) - Instruction: 0xB0 (1 byte) - P1: 0x00 (1 byte) - P2: {blockNumber} (1 byte) - Le: {numberOfBytesToRead: max 16 bytes} (1 byte)
        /// </summary>
        private byte[] ReadBinaryBlocksCommand { get => new byte[] { 0xFF, 0xB0, 0x00, 0x00, 0x00 }; }
        /// <summary>
        /// Class: 0xFF (1 byte) - Instruction: 0xB1 (1 byte) - P1: 0x00 (1 byte) - P2: {blockNumber} (1 byte) - Le: 0x04 (1 byte)
        /// </summary>
        private byte[] ReadValueBlockCommand { get => new byte[] { 0xFF, 0xB1, 0x00, 0x00, 0x04 }; }
        /// <summary>
        /// Class: 0xFF (1 byte) - Instruction: 0xD6 (1 byte) - P1: 0x00 (1 byte) - P2: {blockNumber} (1 byte) - Lc: {numberOfBytesToUpdate} (1 byte) - Data In: {dataIn} (4 bytes)
        /// </summary>
        private byte[] UpdateBinaryBlockCommand { get => new byte[] { 0xFF, 0xD6, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; }

        public MIFARE(IntPtr hCard) : base(hCard) { }

        protected override int MaxWritableBlocks { get => 4; set => MaxWritableBlocks = value; }
        protected override int MaxReadableBytes { get => 16; set => MaxReadableBytes = value; }

        protected override byte[] Get_GetUIDCommand()
        {
            return GetUIDCommand;
        }

        protected override byte[] Get_LoadAuthenticationKeysCommand()
        {
            return LoadAuthenticationKeysCommand;
        }

        protected override byte[] Get_ReadBinaryBlocksCommand(byte blockNumber, int numberOfBytesToRead)
        {            
            byte[] command = ReadBinaryBlocksCommand;
            command.SetValue(blockNumber, 3);
            command.SetValue((byte)numberOfBytesToRead, 4);
            return command;
        }

        protected override byte[] Get_ReadValueBlockCommand(byte blockNumber)
        {
            byte[] command = ReadValueBlockCommand;
            command.SetValue(blockNumber, 3);            
            return command;
        }

        protected override byte[] Get_UpdateBinaryBlockCommand(byte blockNumber, byte[] dataIn, int numberOfBytesToUpdate = 4)
        {
            byte[] command = UpdateBinaryBlockCommand;
            command.SetValue(blockNumber, 3);
            command.SetValue((byte)numberOfBytesToUpdate, 4);
            for(int i = 0; i < dataIn.Length; i++)
            {
                command.SetValue(dataIn[i], i + 5);
            }
            return command;
        }
    }
}
