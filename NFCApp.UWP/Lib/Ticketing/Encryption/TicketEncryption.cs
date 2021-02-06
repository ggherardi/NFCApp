using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ticketing.Encryption
{
    /// <summary>
    /// This class decrpyts and encrypts the tickets. It should be placed on the server in order to keep the key more safe.
    /// </summary>
    public class TicketEncryption
    {
        // Just a temporary sample key
        private static readonly byte[] AesKey = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };

        public static byte[] EncryptTicket(SmartTicket ticket, byte[] cardIV)
        {
            byte[] encryptedTicketBytes = new byte[] { };
            string jsonTicket = JsonSerializer.Serialize(ticket);
            Aes aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            //byte[] iv = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };
            using (MemoryStream ms = new MemoryStream())
            {
                CryptoStream cstream = new CryptoStream(ms, aes.CreateEncryptor(TicketEncryption.AesKey, cardIV), CryptoStreamMode.Write);
                using (StreamWriter writer = new StreamWriter(cstream))
                {
                    writer.Write(jsonTicket);
                }
                encryptedTicketBytes = ms.ToArray();
            }
            return encryptedTicketBytes;
        }

        public static SmartTicket DecryptTicket(byte[] encryptedBytes, byte[] cardIV)
        {
            SmartTicket ticket = null;
            using (MemoryStream ms = new MemoryStream(encryptedBytes))
            {
                Aes aes = Aes.Create();
                aes.Mode = CipherMode.CBC;
                //byte[] iv = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };
                using (CryptoStream cstream = new CryptoStream(ms, aes.CreateDecryptor(TicketEncryption.AesKey, cardIV), CryptoStreamMode.Read))
                {
                    using (StreamReader reader = new StreamReader(cstream))
                    {
                        ticket = JsonSerializer.Deserialize<SmartTicket>(reader.ReadToEnd());
                    }
                }
            }
            return ticket;
        }

        public static byte[] GetPaddedIV(byte[] baseBytpes)
        {
            if(baseBytpes.Length > 16)
            {
                throw new Exception($"The length of the base byte buffer ({baseBytpes.Length}) used for IV is too big.");
            }
            byte[] ivBytes = new byte[16];
            Array.Copy(baseBytpes, ivBytes, baseBytpes.Length);
            return ivBytes;
        }
    }
}
