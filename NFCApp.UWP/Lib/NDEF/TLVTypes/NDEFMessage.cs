using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.NDEF
{

    /// <summary>
    /// NDEFMessage Tag Length Value Block
    /// Reference:  NFCForum-Type-2-Tag_1.1 specifications, chapter 2.3.4 NDEF Message TLV, pag. 12
    /// </summary>
    public class NDEFMessage : TLVBlock
    {
        /// <summary>
        /// Tag Field Value byte for "NDEF Message TLV"        
        /// </summary>
        public override byte Tag { get => 0x03; }

        public NDEFMessage() { }

        public static NDEFMessage GetTextNDEFMessage(string text)
        {
            RTDText rtdText = new RTDText(text, RTDText.EnglishLanguage, RTDText.TextEncoding.UTF8);
            NDEFRecord record = new NDEFRecord(rtdText);
            NDEFMessage message = new NDEFMessage();
            message.Value = record.GetBytes();
            message.Length = GetValueLengthInBytes(message.Value.Length);
            return message;
        }

        //public 

        public override byte[] GetFormattedBlock()
        {
            byte[] tlvBlock = new byte[] { Tag };
            return tlvBlock.Concat(Length).Concat(Value).Concat(new Terminator().GetFormattedBlock()).ToArray();
        }
    }
}
