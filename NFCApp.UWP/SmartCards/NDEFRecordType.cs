using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCApp.UWP.SmartCards
{
    public abstract class NDEFRecordType
    {
        public abstract int TypeIdentifier { get; }
        public abstract int TypeLength { get; }
        public abstract byte[] GetBytes();       
    }

    public class RTDText : NDEFRecordType
    {
        public override int TypeIdentifier { get => 0x54; } // Character "T"
        public override int TypeLength { get => 0x01; }

        private readonly int _flag = 0x00;
        private readonly byte[] _language;
        private readonly byte[] _text;

        public static Language EnglishLanguage = new Language() { Code = new byte[] { 0x65, 0x6E }, Length = 2 };
        public static Language ItalianLanguage = new Language() { Code = new byte[] { 0x69, 0x74 }, Length = 2 };


        public RTDText(string textContent, Language language, TextEncoding textEncoding)
        {
            _flag = _flag | (int)textEncoding | language.Length;
            _language = language.Code;
            _text = Encoding.Default.GetBytes(textContent);
        }

        public RTDText(string textContent, Language language) : this(textContent, language, TextEncoding.UTF8) { }

        public RTDText(string textContent) : this(textContent, EnglishLanguage, TextEncoding.UTF8) { }

        public RTDText() : this(string.Empty, EnglishLanguage, TextEncoding.UTF8) { }

        public override byte[] GetBytes()
        {
            byte[] rtdTextBytes = new byte[] { (byte)_flag, _language[0], _language[1] };
            return rtdTextBytes.Concat(_text).ToArray();
        }

        public enum TextEncoding
        {
            UTF8 = 0x00,
            UTF16 = 0x80
        }

        public class Language
        {
            public byte[] Code { get; set; }
            public int Length { get; set; }
        }
    }
}
