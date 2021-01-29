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
                bytesReadToSkip += message.Length > 255 ? 3 : 1;
                if (message.Length > 0)
                {
                    NDEFRecordFlag flag = NDEFRecordFlag.GetNDEFRecordFlagFromByte(bytes.Skip(bytesReadToSkip).First());
                    bool hasId = (flag.GetByte() & (int)NDEFRecordFlag.IDLength.True) == (int)NDEFRecordFlag.IDLength.True;
                    bytesReadToSkip += hasId ? 4 : 3;
                    NDEFRecordType type = GetNDEFRecordTypeWithTypeIdentifier(bytes[bytesReadToSkip]);
                    NDEFRecord record = new NDEFRecord(type, flag);
                    // WIP, I need to decide what item to return in order to keep reading the bytes from the payload!
                }
            }
            return message;
        }

        private static NDEFRecordType GetNDEFRecordTypeWithTypeIdentifier(int typeIdentifier)
        {
            NDEFRecordType type = null;
            IEnumerable<Type> recordTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(NDEFRecordType)));
            PropertyInfo typeIdentifierProperty = typeof(NDEFRecordType).GetProperties().Where(p => p.Name == "TypeIdentifier").FirstOrDefault();
            foreach(Type recordType in recordTypes)
            {
                type = Activator.CreateInstance(recordType) as NDEFRecordType;
                if (type != null && (int)typeIdentifierProperty.GetValue(type) == typeIdentifier)
                {
                    return type;
                }
            }
            return type;
        }

        public bool ReadByesIntoMessage(byte[] bytes)
        {
            bool keepReading = true;

            return keepReading;
        }        

        public override byte[] GetFormattedBlock()
        {
            byte[] tlvBlock = new byte[] { TagByte };
            return tlvBlock.Concat(LengthBytes).Concat(ValueBytes).Concat(new Terminator().GetFormattedBlock()).ToArray();
        }
    }
}
