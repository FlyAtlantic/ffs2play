/****************************************************************************
**
** Copyright (C) 2017 FSFranceSimulateur team.
** Contact: https://github.com/ffs2/ffs2play
**
** FFS2Play is free software; you can redistribute it and/or modify
** it under the terms of the GNU General Public License as published by
** the Free Software Foundation; either version 3 of the License, or
** (at your option) any later version.
**
** FFS2Play is distributed in the hope that it will be useful,
** but WITHOUT ANY WARRANTY; without even the implied warranty of
** MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
** GNU General Public License for more details.
**
** The license is as published by the Free Software
** Foundation and appearing in the file LICENSE.GPL3
** included in the packaging of this software. Please review the following
** information to ensure the GNU General Public License requirements will
** be met: https://www.gnu.org/licenses/gpl-3.0.html.
****************************************************************************/

/****************************************************************************
 * AEStoPHPCryptography.cs is part of FF2Play project
 *
 * This class purpose a dialog interface to manage account profils
 * to connect severals FFS2Play networks servers
 * **************************************************************************/


using System;
using System.IO;
using System.Security.Cryptography;

namespace ffs2play
{
    public class AEStoPHPCryptography
    {
        private byte[] Key;
        private byte[] IV;

        /// <summary>
        /// Gets the encryption key as a base64 encoded string.
        /// </summary>
        public string EncryptionKeyString
        {
            get { return Convert.ToBase64String(Key); }
        }

        /// <summary>
        /// Gets the initialization key as a base64 encoded string.
        /// </summary>
        public string EncryptionIVString
        {
            get { return Convert.ToBase64String(IV); }
        }

        /// <summary>
        /// Gets the encryption key.
        /// </summary>
        public byte[] EncryptionKey
        {
            get { return Key; }
        }

        /// <summary>
        /// Gets the initialization key.
        /// </summary>
        public byte[] EncryptionIV
        {
            get { return IV; }
        }

        public AEStoPHPCryptography()
        {
            Key = new byte[256 / 8];
            IV = new byte[128 / 8];

            GenerateRandomKeys();
        }

        public AEStoPHPCryptography(string key, string iv)
        {
            Key = Convert.FromBase64String(key);
            IV = Convert.FromBase64String(iv);

            if (Key.Length * 8 != 256)
            {
                throw new Exception("The Key must be exactally 256 bits long!");
            }
            if (IV.Length * 8 != 128)
            {
                throw new Exception("The IV must be exactally 128 bits long!");
            }
        }

        /// <summary>
        /// Generate the cryptographically secure random 256 bit Key and 128 bit IV for the AES algorithm.
        /// </summary>
        public void GenerateRandomKeys()
        {
            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
            random.GetBytes(Key);
            random.GetBytes(IV);
        }

        /// <summary>
        /// Encrypt a message using AES.
        /// </summary>
        /// <param name="plainText">The message to encrypt.</param>
        private byte[] Encrypt2(string plainText)
        {
            try
            {
                RijndaelManaged aes = new RijndaelManaged();
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;
                aes.KeySize = 256;
                aes.Key = Key;
                aes.IV = IV;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                MemoryStream msEncrypt = new MemoryStream();
                CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                StreamWriter swEncrypt = new StreamWriter(csEncrypt);

                swEncrypt.Write(plainText);

                swEncrypt.Close();
                csEncrypt.Close();
                aes.Clear();

                return msEncrypt.ToArray();
            }
            catch (Exception ex)
            {
                throw new CryptographicException("Problem trying to encrypt.", ex);
            }
        }

        /// <summary>
        /// Decrypt a message that was AES encrypted.
        /// </summary>
        /// <param name="cipherText">The string to decrypt.</param>
        private string Decrypt2(byte[] cipherText)
        {
            try
            {
                RijndaelManaged aes = new RijndaelManaged();
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;
                aes.KeySize = 256;
                aes.Key = Key;
                aes.IV = IV;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                MemoryStream msDecrypt = new MemoryStream(cipherText);
                CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                StreamReader srDecrypt = new StreamReader(csDecrypt);

                string plaintext = srDecrypt.ReadToEnd();

                srDecrypt.Close();
                csDecrypt.Close();
                msDecrypt.Close();
                aes.Clear();

                return plaintext;
            }
            catch (Exception ex)
            {
                throw new CryptographicException("Problem trying to decrypt.", ex);
            }
        }
    }
}
