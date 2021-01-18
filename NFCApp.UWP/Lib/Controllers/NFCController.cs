using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.Controllers
{
    public abstract class NFCController
    {
        protected abstract NFCCommand Get_DataExchangeCommand(byte[] payload);        

        public NFCCommand GetDataExchangeCommand(byte[] payload)
        {
            return Get_DataExchangeCommand(payload);
        }

        public NFCCommand GetDataExchangeCommand()
        {
            return this.GetDataExchangeCommand(new byte[] { });
        }                
    }
}
