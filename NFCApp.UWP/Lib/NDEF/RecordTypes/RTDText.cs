﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.NDEF
{
    public class RTDText : NDEFRecordType
    {
        public override int TypeIdentifier { get => 0x54; } // Record type "T", Text
        public override int TypeLength { get => 0x01; }

        private int _flag = 0x00;
        private byte[] _language;
        private byte[] _text;

        public static Language EnglishLanguage = new Language() { Code = new byte[] { 0x65, 0x6E }, Length = 2 };
        public static Language ItalianLanguage = new Language() { Code = new byte[] { 0x69, 0x74 }, Length = 2 };

        public RTDText(string textContent, Language language, RTDTextFlag flag)
        {
            _flag = flag.GetByte();
            _language = language.Code;
            _text = Encoding.Default.GetBytes(textContent);
        }

        public RTDText(string textContent, Language language) : this(textContent, language, new RTDTextFlag(RTDTextFlag.LanguageEncoding.UTF8, language.Length)) { }

        public RTDText(string textContent) : this(textContent, EnglishLanguage) { }

        public RTDText() { }

        public override byte[] GetBytes()
        {
            byte[] rtdTextBytes = new byte[] { (byte)_flag, _language[0], _language[1] };
            return rtdTextBytes.Concat(_text).ToArray();
        }

        public override void BuildRecordFromBytes(byte[] bytes)
        {
            _flag = bytes[0];
            _language = new byte[] { bytes[1], bytes[2] };
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

    public class RTDTextFlag
    {
        public LanguageEncoding LanguageFormatBit { get; set; }
        public int LanguageCodeLength { get; set; }

        public RTDTextFlag(LanguageEncoding format, int languageCodeLength)
        {
            LanguageFormatBit = format;
            LanguageCodeLength = languageCodeLength.Clamp(0, 63);
        }

        public enum LanguageEncoding
        {
            UTF8 = 0x00,
            UTF16 = 0x80
        }

        public RTDTextFlag() { }

        public byte GetByte()
        {
            return (byte)(0 | (int)LanguageFormatBit | LanguageCodeLength);
        }
    }
}