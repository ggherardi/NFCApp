using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCApp.UWP.SmartCards
{
    public interface IReaderSignalControl
    {
        byte[] GetErrorSignalCommand();
        byte[] GetSuccessSignalCommand();
        uint GetControlCode();
    }
}
