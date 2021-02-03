using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public override byte TagByte { get => 0x03; }
        public NDEFRecord Record { get; set; }
        public int TotalHeaderLength { get; set; }

        public NDEFMessage() { }

        public static NDEFMessage GetTextNDEFMessage(string text)
        {
            RTDText rtdText = new RTDText(text, RTDText.EnglishLanguage);
            NDEFRecord record = new NDEFRecord(rtdText);
            NDEFMessage message = new NDEFMessage();
            message.ValueBytes = record.GetBytes();
            message.LengthBytes = GetValueLengthInBytes(message.ValueBytes.Length);
            return message;
        }

        public static NDEFMessage GetNDEFMessageFromBytes(byte[] bytes)
        {
            int bytesReadToSkip = 1;
            NDEFMessage message = new NDEFMessage();
            if (bytes[0] == message.TagByte)
            {                
                message.Length = TLVBlock.GetLengthFromBytes(bytes.Skip(bytesReadToSkip).Take(3).ToArray());
                bool isLongMessage = message.Length > 255;
                message.LengthBytes = isLongMessage ? bytes.Skip(bytesReadToSkip).Take(3).ToArray() : bytes.Skip(bytesReadToSkip).Take(1).ToArray();
                bytesReadToSkip += isLongMessage ? 3 : 1;
                if (message.Length > 0)
                {
                    NDEFRecord record = new NDEFRecord();
                    NDEFRecordFlag recordFlag = NDEFRecordFlag.GetNDEFRecordFlagFromByte(bytes.Skip(bytesReadToSkip++).First());
                    record.RecordFlag = recordFlag;
                    record.FlagField = recordFlag.GetByte();
                    bool hasId = (record.FlagField & (int)NDEFRecordFlag.IDLength.True) == (int)NDEFRecordFlag.IDLength.True;
                    bool isShortRecord = (record.FlagField & (int)NDEFRecordFlag.ShortRecord.True) == (int)NDEFRecordFlag.ShortRecord.True;
                    record.TypeLengthField = bytes.Skip(bytesReadToSkip++).First();
                    int payloadBytesCount = isShortRecord ? 1 : 4;
                    record.PayloadLengthField = bytes.Skip(bytesReadToSkip).Take(payloadBytesCount).ToArray();
                    bytesReadToSkip += payloadBytesCount;
                    if (hasId)
                    {
                        record.IDLengthField = bytes[bytesReadToSkip++];
                        record.TypeIdentifierField = bytes[bytesReadToSkip++];
                        record.IDField = bytes[bytesReadToSkip++];
                    }
                    else
                    {
                        record.TypeIdentifierField = bytes[bytesReadToSkip++];
                    }                    
                    NDEFRecordType type = NDEFRecordType.GetNDEFRecordTypeWithTypeIdentifier(record.TypeIdentifierField);                    
                    type.BuildRecordFromBytes(bytes.Skip(bytesReadToSkip).Take(type.HeaderLength).ToArray());                    
                    record.RecordContent = type;
                    message.TotalHeaderLength = bytesReadToSkip += type.HeaderLength;
                    message.Record = record;                    
                }
            }
            else
            {
                // No NDEF Message found
            }
            return message;
        }

        public bool ReadByesIntoMessage(byte[] bytes)
        {
            bool keepReading = true;
            int maxIndexToCopy = bytes.Length;
            int terminatorIndex = bytes.ToList().FindIndex(b => b == new Terminator().TagByte);
            if(terminatorIndex != -1)
            {
                keepReading = false;
                maxIndexToCopy = terminatorIndex;
            }
            this.Record.RecordContent.AddTextToPayload(bytes.Take(maxIndexToCopy).ToArray());
            return keepReading;
        }                

        public override byte[] GetFormattedBlock()
        {
            byte[] tlvBlock = new byte[] { TagByte };
            return tlvBlock.Concat(LengthBytes).Concat(ValueBytes).Concat(new Terminator().GetFormattedBlock()).ToArray();
        }
    }
}
