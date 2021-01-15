using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCApp.UWP.SmartCards
{
    public class Ntag215 : NFCCard
    {
        public Ntag215(IntPtr hCard) : base(hCard) { }

        public Ntag215() : base() { }

        public override int MaxWritableBlocks { get => 4; protected set => MaxWritableBlocks = value; }
        public override int MaxReadableBytes { get => 16; protected set => MaxReadableBytes = value; }
    }
}
