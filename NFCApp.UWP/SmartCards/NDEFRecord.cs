using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCApp.UWP.SmartCards
{
    public class NDEFRecord
    {
        private NDEFRecordFlag _flag;
        private readonly int _typeLength;
        private readonly int[] _payloadLength;
        private readonly int _idLength;
        private readonly int _typeIdentifier;
        private readonly int _id;
        private readonly NDEFRecordType _NDEFRecord;

        public NDEFRecord(NDEFRecordType recordType, NDEFRecordFlag flag, int typeLength)
        {

        }

        public NDEFRecord(NDEFRecordType recordType)
        {
            _flag = new NDEFRecordFlag(true, true, false/*check length*/, true/*check length*/, false/*approfondire funzionamento ID*/, TypeNameFormat.NFCForumWellKnownType);
            _typeLength = recordType.TypeLength;
            _typeIdentifier = recordType.TypeIdentifier;
            _NDEFRecord = recordType;            
        }

        public byte[] GetRecordBytes()
        {
            // Creating the record by inserting the Flag (MB, ME, CF, SR, IL, TNF)
            byte[] record = new byte[] { _flag.GetByte() };

            // Adding
            //record.ToArray().

            return record.Concat(_NDEFRecord.GetBytes()).ToArray();
        }
    }

    public class NDEFRecordFlag
    {
        public int MessageBegin { get; set; }
        public int MessageEnd { get; set; }
        public int Chunk { get; set; }
        public int ShortRecord { get; set; }
        public int IDLength { get; set; }
        public int TypeNameFormat { get; set; }

        public NDEFRecordFlag(bool messageBegin, bool messageEnd, bool chunk, bool shortRecord, bool idLength, TypeNameFormat tnf)
        {
            MessageBegin = messageBegin ? 0x80 : 0x00;
            MessageEnd = messageEnd ? 0x40 : 0x00;
            Chunk = chunk ? 0x20 : 0x00;
            ShortRecord = shortRecord ? 0x10 : 0x00;
            IDLength = idLength ? 0x08 : 0x00;
            TypeNameFormat = (int)tnf;
        }

        public byte GetByte()
        {
            return (byte)(MessageBegin | MessageEnd | Chunk | ShortRecord | IDLength | TypeNameFormat);
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
