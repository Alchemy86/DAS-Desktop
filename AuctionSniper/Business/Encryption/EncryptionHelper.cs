namespace AuctionSniper.Business.Encryption
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Security.Cryptography;
    using System.IO;

    public class EncryptionHelper
    {
        public static EncryptionHelper Instance = new EncryptionHelper();
        // Fields
        private ICryptoTransform DecryptorTransform;
        private ICryptoTransform EncryptorTransform;
        private byte[] Key = new byte[] {
        6, 0xd9, 0x13, 11, 0x42, 0x1a, 0x55, 0x2d, 12, 2, 0x1b, 0xa2, 0x20, 0x70, 0x79, 0xd1,
        0xde, 0x18, 0xaf, 0x90, 0x70, 0xe7, 0xc4, 0xea, 0x18, 0x1a, 0x11, 7, 0x83, 1, 0x35, 2
        };

        private UTF8Encoding UTFEncoder;
        private byte[] Vector = new byte[] { 0xd3, 0x40, 0xbf, 0x17, 0x17, 3, 0x7a, 0x77, 0xdf, 0x79, 0xfc, 0x4c, 0x4f, 0x20, 0x36, 0x9c };

        // Methods
        public EncryptionHelper()
        {
            RijndaelManaged managed = new RijndaelManaged();
            this.EncryptorTransform = managed.CreateEncryptor(this.Key, this.Vector);
            this.DecryptorTransform = managed.CreateDecryptor(this.Key, this.Vector);
            this.UTFEncoder = new UTF8Encoding();
        }

        public string ByteArrToString(byte[] byteArr)
        {
            string str = "";
            for (int i = 0; i <= byteArr.GetUpperBound(0); i++)
            {
                byte num = byteArr[i];
                if (num < 10)
                {
                    str = str + "00" + num.ToString();
                }
                else if (num < 100)
                {
                    str = str + "0" + num.ToString();
                }
                else
                {
                    str = str + num.ToString();
                }
            }
            return str;
        }

        public string Decrypt(byte[] EncryptedValue)
        {
            MemoryStream stream = new MemoryStream();
            CryptoStream stream2 = new CryptoStream(stream, this.DecryptorTransform, CryptoStreamMode.Write);
            stream2.Write(EncryptedValue, 0, EncryptedValue.Length);
            stream2.FlushFinalBlock();
            stream.Position = 0L;
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Close();
            return this.UTFEncoder.GetString(buffer);
        }

        public string DecryptString(string EncryptedString)
        {
            if (string.IsNullOrEmpty(EncryptedString))
            {
                return "";
            }
            bool flag = true;
            for (int i = 0; i < EncryptedString.Length; i++)
            {
                if (!char.IsDigit(EncryptedString, i))
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                return this.Decrypt(this.StrToByteArray(EncryptedString));
            }
            return EncryptedString;
        }

        public byte[] Encrypt(string TextValue)
        {
            byte[] bytes = this.UTFEncoder.GetBytes(TextValue);
            MemoryStream stream = new MemoryStream();
            CryptoStream stream2 = new CryptoStream(stream, this.EncryptorTransform, CryptoStreamMode.Write);
            stream2.Write(bytes, 0, bytes.Length);
            stream2.FlushFinalBlock();
            stream.Position = 0L;
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream2.Close();
            stream.Close();
            return buffer;
        }

        public string EncryptToString(string TextValue)
        {
            return this.ByteArrToString(this.Encrypt(TextValue));
        }

        public static byte[] GenerateEncryptionKey()
        {
            RijndaelManaged managed = new RijndaelManaged();
            managed.GenerateKey();
            return managed.Key;
        }

        public static byte[] GenerateEncryptionVector()
        {
            RijndaelManaged managed = new RijndaelManaged();
            managed.GenerateIV();
            return managed.IV;
        }

        public bool IsEncrypted(string encryptedString)
        {
            for (int i = 0; i < encryptedString.Length; i++)
            {
                if (!char.IsDigit(encryptedString, i))
                {
                    return false;
                }
            }
            return true;
        }

        public byte[] StrToByteArray(string str)
        {
            if (str.Length == 0)
            {
                throw new Exception("Invalid string value in StrToByteArray");
            }
            byte[] buffer = new byte[str.Length / 3];
            int startIndex = 0;
            int num3 = 0;
            do
            {
                byte num = byte.Parse(str.Substring(startIndex, 3));
                buffer[num3++] = num;
                startIndex += 3;
            }
            while (startIndex < str.Length);
            return buffer;
        }
    }
}
