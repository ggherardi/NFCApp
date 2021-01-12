using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCApp.UWP.SmartCards
{
    public abstract class TLVBlock
    {
        protected byte _tag;
        protected byte[] _length = new byte[] { };
        protected byte[] _value = new byte[] { };

        public abstract byte Tag { get; }
        public virtual byte[] Length { get => _length; protected set => _length = value; }
        public virtual byte[] Value { get => _value; protected set => _value = value; }

        public virtual byte[] GetFormattedBlock()
        {
            return (new byte[] { Tag }).Concat(Length).Concat(Value).ToArray();
        }

        public byte[] GetValueLengthInBytes(int valueLength)
        {
            byte[] valueLengthInBytes = BitConverter.GetBytes(valueLength);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(valueLengthInBytes);
            }
            if (valueLength < 255)
            {
                valueLengthInBytes = new byte[] { (byte)valueLengthInBytes[3] };
            }
            else
            {

                valueLengthInBytes = new byte[] { 0xFF, valueLengthInBytes[2], valueLengthInBytes[3] };
            }
            return valueLengthInBytes;
        }
    }

    public class NDEFMessageTLV : TLVBlock
    {
        public override byte Tag { get => 0x03; }

        public NDEFMessageTLV(string value)
        {
            Value = Encoding.UTF8.GetBytes(value);
            Length = GetValueLengthInBytes(Value.Length);
        }

        public override byte[] GetFormattedBlock()
        {
            byte[] tlvBlock = new byte[] { Tag };
            return tlvBlock.Concat(Length).Concat(Value).Concat(new TerminatorTLV().GetFormattedBlock()).ToArray();
        }
    }

    public class TerminatorTLV : TLVBlock
    {
        public override byte Tag { get => 0xFE; }
    }
}
