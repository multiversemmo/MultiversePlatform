/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Multiverse.Tools.DecTree
{
    class Program
    {
        static void GenKey(string baseName, out byte[] desKey, out byte[] desIV)
        {
            byte[] secretBytes = { 6, 29, 66, 6, 2, 68, 4, 7, 70 };

            byte[] baseNameBytes = new ASCIIEncoding().GetBytes(baseName);

            byte[] hashBytes = new byte[secretBytes.Length + baseNameBytes.Length];

            // copy secret byte to start of hash array
            for (int i = 0; i < secretBytes.Length; i++)
            {
                hashBytes[i] = secretBytes[i];
            }

            // copy filename byte to end of hash array
            for (int i = 0; i < baseNameBytes.Length; i++)
            {
                hashBytes[i + secretBytes.Length] = baseNameBytes[i];
            }

            SHA1Managed sha = new SHA1Managed();

            // run the sha1 hash
            byte[] hashResult = sha.ComputeHash(hashBytes);

            desKey = new byte[8];
            desIV = new byte[8];

            for (int i = 0; i < 8; i++)
            {
                desKey[i] = hashResult[i];
                desIV[i] = hashResult[8 + i];
            }
        }

        static void DumpKeyIV(byte[] desKey, byte[] desIV)
        {
            Console.WriteLine("Key:");
            for (int i = 0; i < 8; i++)
            {
                Console.Write("{0:x} ", desKey[i]);
            }
            Console.WriteLine("");

            Console.WriteLine("IV:");
            for (int i = 0; i < 8; i++)
            {
                Console.Write("{0:x} ", desIV[i]);
            }
            Console.WriteLine("");

        }

        static void Main(string[] args)
        {
            foreach (string fileName in args)
            {
                string encName = fileName;
                //string encName = "EnglishOak_RT.tre";

                int extOffset = encName.LastIndexOf('.');

                string baseName = encName.Substring(0, extOffset);

                byte[] desKey;
                byte[] desIV;

                GenKey(baseName, out desKey, out desIV);

                DumpKeyIV(desKey, desIV);

                //Create the file streams to handle the input and output files.
                FileStream fin = new FileStream(encName, FileMode.Open, FileAccess.Read);
                FileStream fout = new FileStream(String.Format("{0}.spt", baseName), FileMode.OpenOrCreate, FileAccess.Write);
                fout.SetLength(0);

                //Create variables to help with read and write.
                byte[] bin = new byte[256];  //This is intermediate storage for the encryption.
                int len;                     //This is the number of bytes to be written at a time.

                DES des = new DESCryptoServiceProvider();
                CryptoStream encStream = new CryptoStream(fin, des.CreateDecryptor(desKey, desIV), CryptoStreamMode.Read);

                //Read from the input file, then encrypt and write to the output file.
                while ((len = encStream.Read(bin, 0, bin.Length)) > 0)
                {
                    fout.Write(bin, 0, len);
                }

                encStream.Close();
                fout.Close();
                fin.Close();
            }
        }
    }
}
