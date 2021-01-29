using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.NDEF
{
    /// <summary>
    /// NDEF Record format
    /// Reference: NFC Data Exchange Format (NDEF), chapter 3.2, pag. 14
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

        public NDEFRecordFlag RecordFlag { get => _flagObject; }
        public NDEFRecordType RecordType { get => _NDEFRecord; }

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
            _hasID = flag.IDLengthBit == NDEFRecordFlag.IDLength.True;
            if (_hasID)
            {
                _idLengthField = (byte)idLength;
                _idField = (byte)id;
            }

            // Setting Flag field bits
            _flagObject = flag;
            if(_flagObject.ShortRecordBit == NDEFRecordFlag.ShortRecord.True && !_isShortRecord)
            {
                _flagObject.ShortRecordBit = 0;
            }
            _flagField = flag.GetByte();
        }

        public NDEFRecord(NDEFRecordType recordType, NDEFRecordFlag flag, bool chunked = false) : this(recordType, flag, 0, 0, chunked) { }

        public NDEFRecord(NDEFRecordType recordType, bool chunked = false) : this(recordType, new NDEFRecordFlag(NDEFRecordFlag.MessageBegin.True, NDEFRecordFlag.MessageEnd.True, NDEFRecordFlag.Chunk.False, NDEFRecordFlag.ShortRecord.True, NDEFRecordFlag.IDLength.False, NDEFRecordFlag.TypeNameFormat.NFCForumWellKnownType), chunked) { }

        public NDEFRecord() { }

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
    /// Class used to set all the bits required in the flag
    /// Reference: NFC Data Exchange Format (NDEF) Technical Specifications, chapters 3.2.1 - 3.2.6, pag. 14 - 16
    /// </summary>
    public class NDEFRecordFlag
    {
        public MessageBegin MessageBeginBit { get; set; }
        public MessageEnd MessageEndBit { get; set; }
        public Chunk ChunkBit { get; set; }
        public ShortRecord ShortRecordBit { get; set; }
        public IDLength IDLengthBit { get; set; }
        public TypeNameFormat TNFBits { get; set; }

        public NDEFRecordFlag(MessageBegin messageBegin, MessageEnd messageEnd, Chunk chunk, ShortRecord shortRecord, IDLength idLength, TypeNameFormat tnf)
        {
            MessageBeginBit = messageBegin;
            MessageEndBit = messageEnd;
            ChunkBit = chunk;
            ShortRecordBit = shortRecord;
            IDLengthBit = idLength;
            TNFBits = tnf;
        }

        public NDEFRecordFlag() { }

        public static NDEFRecordFlag GetNDEFRecordFlagFromByte (byte flagByte)
        {
            NDEFRecordFlag recordFlag = new NDEFRecordFlag();           
            recordFlag.MessageBeginBit = (MessageBegin)(flagByte & (int)MessageBegin.True);
            recordFlag.MessageEndBit = (MessageEnd)(flagByte & (int)MessageEnd.True);
            recordFlag.ChunkBit = (Chunk)(flagByte & (int)Chunk.True);
            recordFlag.ShortRecordBit = (ShortRecord)(flagByte & (int)ShortRecord.True);
            recordFlag.IDLengthBit = (IDLength)(flagByte & (int)IDLength.True);
            recordFlag.TNFBits = (TypeNameFormat)(flagByte & (int)TypeNameFormat.Reserved);
            return recordFlag;
        }

        public byte GetByte()
        {
            return (byte)(0 | (int)MessageBeginBit | (int)MessageEndBit | (int)ChunkBit | (int)ShortRecordBit | (int)IDLengthBit | (int)TNFBits);
        }

        public enum MessageBegin
        {
            True = 0x80,
            False = 0x00
        }

        public enum MessageEnd
        {
            True = 0x40,
            False = 0x00
        }

        public enum Chunk
        {
            True = 0x20,
            False = 0x00
        }

        public enum ShortRecord
        {
            True = 0x10,
            False = 0x00
        }

        public enum IDLength
        {
            True = 0x08,
            False = 0x00
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
}
