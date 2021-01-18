using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC
{
    public class NFCOperation
    {
        private NFCCommand _controllerCommand;
        private NFCCommand _readerCommand;
        private NFCCommand _cardCommand;

        public int Status { get; set; }
        public byte[] ResponseBuffer { get; set; }
        public byte[] ResponsePayload { get; set; }

        public void ElaborateResponse()
        {

        }
    }

}
