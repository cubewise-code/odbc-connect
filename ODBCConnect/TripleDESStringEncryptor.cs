using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Cubewise.Query
{
    internal class TripleDESStringEncryptor
    {
        private byte[] _key;
        private byte[] _iv;
        private TripleDESCryptoServiceProvider _provider;

        public TripleDESStringEncryptor()
        {
            _key = System.Text.Encoding.ASCII.GetBytes("QUERY_MY_TM1_ADKOPAAAWJK");
            _iv = System.Text.Encoding.ASCII.GetBytes("USAZBGAW");
            _provider = new TripleDESCryptoServiceProvider();
        }

        #region IStringEncryptor Members

        public string EncryptString(string plainText)
        {
        	if (plainText == null)
            {
                return null;
            }
        	
        	using (MemoryStream stream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(stream, _provider.CreateEncryptor(_key, _iv), CryptoStreamMode.Write))
                {
                	byte[] input = UTF8Encoding.UTF8.GetBytes(plainText);
                    cryptoStream.Write(input, 0, input.Length);
                    cryptoStream.FlushFinalBlock();

                    return Convert.ToBase64String(stream.ToArray());
                }
            }
        	            
        }

        public string DecryptString(string encryptedText)
        {
            if (encryptedText == null)
            {
                return null;
            }
            using (MemoryStream stream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(stream, _provider.CreateDecryptor(_key, _iv), CryptoStreamMode.Write))
                {
                    byte[] input = Convert.FromBase64String(encryptedText);
                    cryptoStream.Write(input, 0, input.Length);
                    cryptoStream.FlushFinalBlock();

                    return UTF8Encoding.UTF8.GetString(stream.ToArray());
                }
            }
        }

        #endregion

        private string Transform(string text, ICryptoTransform transform)
        {
            if (text == null)
            {
                return null;
            }
            using (MemoryStream stream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Write))
                {
                    byte[] input = Convert.FromBase64String(text);
                    cryptoStream.Write(input, 0, input.Length);
                    cryptoStream.FlushFinalBlock();

                    return Convert.ToBase64String(stream.ToArray());
                }
            }
        }
    }
}
