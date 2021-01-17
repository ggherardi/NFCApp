using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.NDEF
{
    /// <summary>
    /// Single Record as specified in the "NFC Data Exchange Format (NDEF)" TS, chapter 3.2
    /// </summary>
    public class NDEFRecord
    {
        // Flag field: (MB, ME, CF, SR, IL, TNF)
        private readonly NDEFRecordFlag _flagObject;
        private readonly byte _flagField;
        private readonly byte _typeLengthField;
        private readonly byte[] _payloadLengthField;
        private readonly byte _idLengthField;
        private readonly byte _typeIdentifierField;
        private readonly byte _idField;
        private readonly NDEFRecordType _NDEFRecord;
        private readonly byte[] _NDEFRecordPayloadBytes;

        private bool _isShortRecord;
        private bool _hasID;

        public NDEFRecord(NDEFRecordType recordType, NDEFRecordFlag flag, int idLength, int id, bool chunk)
        {
            // Add management for chunked record, not required right now
            // Getting NDEFRecordType byte
            _NDEFRecord = recordType;
            _NDEFRecordPayloadBytes = _NDEFRecord.GetBytes();

            // Determining payload length byte/bytes
            byte[] payloadLength = BitConverter.GetBytes(_NDEFRecordPayloadBytes.Length);
            _isShortRecord = _NDEFRecordPayloadBytes.Length <= 255;
            int numberOfPayloadLengthFields = _isShortRecord ? 1 : 4;
            _payloadLengthField = _isShortRecord ? new byte[numberOfPayloadLengthFields] : new byte[numberOfPayloadLengthFields];            
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(payloadLength);
            }
            for (int i = 0; i < numberOfPayloadLengthFields; i++)
            {
                int j = 3;
                _payloadLengthField[i] = payloadLength[j--];
            }

            // Setting Type Length and Type bytes
            _typeLengthField = (byte)_NDEFRecord.TypeLength;
            _typeIdentifierField = (byte)_NDEFRecord.TypeIdentifier;

            // Setting ID Length and ID bytes only if the flag has IL field = 1
            _hasID = flag.IDLength == 1;
            if (_hasID)
            {
                _idLengthField = (byte)idLength;
                _idField = (byte)id;
            }

            // Setting Flag field bits
            _flagObject = flag;
            if(_flagObject.ShortRecord == 1 && !_isShortRecord)
            {
                _flagObject.ShortRecord = 0;
            }
            _flagField = flag.GetByte();
        }

        public NDEFRecord(NDEFRecordType recordType, NDEFRecordFlag flag, bool chunked = false) : this(recordType, flag, 0, 0, chunked) { }

        public NDEFRecord(NDEFRecordType recordType, bool chunked = false) : this(recordType, new NDEFRecordFlag() { MessageBegin = 1, MessageEnd = 1, Chunk = 0, ShortRecord = 1, IDLength = 0, TypeNameFormat = TypeNameFormat.NFCForumWellKnownType}, 0, 0, chunked) { }

        public byte[] GetBytes()
        {
            List<byte> record = new List<byte>();
            record.Add(_flagField);
            record.Add(_typeLengthField);
            record.AddRange(_payloadLengthField);
            if (_hasID)
            {
                record.Add(_idLengthField);
            }
            record.Add(_typeIdentifierField);
            if (_hasID)
            {
                record.Add(_idField);
            }
            record.AddRange(_NDEFRecordPayloadBytes);
            return record.ToArray();
        }
    }

    /// <summary>
    /// Class used to set all the bits required in the flag as specified in the "NFC Data Exchange Format (NDEF)" TS, chapters 3.2.1 - 3.2.6
    /// </summary>
    public class NDEFRecordFlag
    {
        public int MessageBegin { get; set; }
        public int MessageEnd { get; set; }
        public int Chunk { get; set; }
        public int ShortRecord { get; set; }
        public int IDLength { get; set; }
        public TypeNameFormat TypeNameFormat { get; set; }

        public NDEFRecordFlag(bool messageBegin, bool messageEnd, bool chunk, bool shortRecord, bool idLength, TypeNameFormat tnf)
        {
            MessageBegin = messageBegin ? 0x80 : 0x00;
            MessageEnd = messageEnd ? 0x40 : 0x00;
            Chunk = chunk ? 0x20 : 0x00;
            ShortRecord = shortRecord ? 0x10 : 0x00;
            IDLength = idLength ? 0x08 : 0x00;
            TypeNameFormat = tnf;
        }

        public NDEFRecordFlag() { }

        public byte GetByte()
        {
            return (byte)(((((((((((0 | MessageBegin) << 1) | MessageEnd) << 1) | Chunk) << 1) | ShortRecord) << 1) | IDLength) << 3) | (int)TypeNameFormat);
        }
    }
    public enum TypeNameFormat
    {
        Empty = 0x00,
        NFCForumWellKnownType = 0x01,
        MediaType = 0x02,
        AbsoluteURI = 0x03,
        NFCForumExternalType = 0x04,
        Unknown = 0x05,
        Unchanged = 0x06,        
        Reserved = 0x07
    }
}
