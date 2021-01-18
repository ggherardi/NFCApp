using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.Controllers
{
    public abstract class NFCController
    {
        protected abstract byte[] Get_DataExchangeCommand(byte[] payload);

        public byte[] GetDataExchangeCommand(byte[] payload)
        {
            return Get_DataExchangeCommand(payload);
        }

        public byte[] GetDataExchangeCommand()
        {
            return this.GetDataExchangeCommand(new byte[] { });
        }        
    }
}
