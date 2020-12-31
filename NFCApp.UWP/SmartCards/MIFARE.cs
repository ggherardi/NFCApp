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
        /// Command: Class, INS, P1, P2, Le        
        /// </summary>
        private readonly byte[] GetUIDCommand = { 0xFF, 0xCA, 0x00, 0x00, 0x00 };
        private readonly byte[] LoadAuthenticationKeysCommand = { 0xFF, 0x82, 0x00, 0x00, 0x00 };

        public MIFARE(int hCard) : base(hCard) { }

        public override byte[] Get_GetUIDCommand()
        {
            return GetUIDCommand;
        }

        public override byte[] Get_LoadAuthenticationKeysCommand()
        {
            return LoadAuthenticationKeysCommand;
        }

        public override byte[] Get_ReadBinaryBlocksCommand()
        {
            throw new NotImplementedException();
        }
    }
}
