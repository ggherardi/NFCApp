using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCApp.UWP.SmartCards
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

        public abstract int MaxWritableBlocks { get; protected set; }
        public abstract int MaxReadableBytes { get; protected set; }

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
