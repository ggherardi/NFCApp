using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.Cards
{
    public abstract class NFCCard
    {
        protected IntPtr _cardNumber;
        public int CardNumber { get { return _cardNumber.ToInt32(); } }

        public NFCCard(IntPtr hCard)
        {
            _cardNumber = hCard;            
        }

        public NFCCard() { }

        protected abstract byte[] Get_PWD_AUTH_Command(string password);
        protected abstract byte[] Get_GET_VERSION_Command();

        public abstract int MaxWritableBlocks { get; protected set; }
        public abstract int MaxReadableBytes { get; protected set; }

        public byte[] GetPasswordAuthenticationCommand(string password)
        {
            return Get_PWD_AUTH_Command(password);
        }

        public byte[] GetGetVersionCommand()
        {
            return Get_GET_VERSION_Command();
        }

        public static class CardHelper
        {
            public static T GetCard<T>(int cardNumber) where T : NFCCard, new()
            {
                T nfcReader = new T();
                nfcReader._cardNumber = new IntPtr(cardNumber);
                return nfcReader;
            }
        }        
    }

}
