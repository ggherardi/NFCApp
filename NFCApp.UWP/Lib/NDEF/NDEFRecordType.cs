using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.NDEF
{
    /// <summary>
    /// Abstract class used as template for all the Record Types
    /// Reference: NFC Record Type Definition (RTD) - Technical Specifications
    /// </summary>
    public abstract class NDEFRecordType
    {
        public abstract int TypeIdentifier { get; }
        public abstract int TypeLength { get; }
        public abstract byte[] GetBytes();       
    }    
}
