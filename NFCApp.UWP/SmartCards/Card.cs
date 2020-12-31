using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCApp.UWP.SmartCards
{
    public abstract class Card
    {
        public int CardNumber { get; }
        public Card(int hCard) { CardNumber = hCard; }
        
        public abstract byte[] Get_GetUIDCommand();
        public abstract byte[] Get_LoadAuthenticationKeysCommand();
        public abstract byte[] Get_ReadBinaryBlocksCommand();
    }
}
