using System;
using System.IO;
using System.Text;
using Axiom.Core;
using Axiom.MathLib;

namespace Axiom.Serialization {

    public class BinaryMemoryReader {
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(BinaryMemoryReader));

        private int filePos = 0;
        private int fileLength;
        private byte[] fileBytes = null;

        public BinaryMemoryReader(Stream stream, System.Text.Encoding encoding) {
            Initialize(stream);
        }
        
        public BinaryMemoryReader(Stream stream) {
            Initialize(stream);
        }

        public void Initialize(Stream stream) {
            try {
                fileLength = (int)stream.Length;
                fileBytes = new byte[fileLength];
                stream.Read(fileBytes, 0, fileLength);
                stream.Close();
            }
            catch (Exception e) {
                log.ErrorFormat("BinaryMemoryReader.Initialize: Error '{0}' reading stream of length {1} bytes", e.Message, fileLength);
            }
        }

        private string ReadTooFar(string routineName) {
            string s = string.Format("BinaryMemoryReader.{0}: Read of stream beyond file byte count {1}",
                routineName, fileLength);
            log.Error(s);
            return s;
        }

        public long Seek(long longOffset, SeekOrigin origin) {
            int offset = (int)longOffset;
            int target = 0;
            switch (origin) {
            case SeekOrigin.Begin:
                target = offset;
                break;
            case SeekOrigin.Current:
                target = filePos + offset;
                break;
            case SeekOrigin.End:
                target = fileLength + offset;
                break;
            }
            if (target < 0 || target > filePos) {
                string s = string.Format("BinaryMemoryReader.{0}: Seek to {0}, but the file length is {1}", target, fileLength);
                log.Error(s);
                throw new Exception(s);
            }
            filePos = target;
            return (long)filePos;
        }
        
        public byte ReadByte() {
            if (filePos < fileLength)
                return fileBytes[filePos++];
            else
                throw new Exception(ReadTooFar("ReadByte"));
        }

        public char ReadChar() {
            if (filePos < fileLength)
                return (char)fileBytes[filePos++];
            else
                throw new Exception(ReadTooFar("ReadChar"));
        }

        public bool ReadBoolean() {
            if (filePos < fileLength)
                return fileBytes[filePos++] != 0;
            else
                throw new Exception(ReadTooFar("ReadBoolean"));
        }

        public short ReadInt16() {
            if (filePos + 1 < fileLength) {
                short x = BitConverter.ToInt16(fileBytes, filePos);
                filePos += 2;
                return x;
            }
            else
                throw new Exception(ReadTooFar("ReadInt16"));
        }
            
        public ushort ReadUInt16() {
            if (filePos + 1 < fileLength) {
                ushort x = BitConverter.ToUInt16(fileBytes, filePos);
                filePos += 2;
                return x;
            }
            else
                throw new Exception(ReadTooFar("ReadUInt16"));
        }
            
        public int ReadInt32() {
            if (filePos + 3 < fileLength) {
                int x = BitConverter.ToInt32(fileBytes, filePos);
                filePos += 4;
                return x;
            }
            else
                throw new Exception(ReadTooFar("ReadInt32"));
        }

        public uint ReadUInt32() {
            if (filePos + 3 < fileLength) {
                uint x = BitConverter.ToUInt32(fileBytes, filePos);
                filePos += 4;
                return x;
            }
            else
                throw new Exception(ReadTooFar("ReadUInt32"));
        }

        public long ReadInt64() {
            if (filePos + 7 < fileLength) {
                long x = BitConverter.ToInt64(fileBytes, filePos);
                filePos += 8;
                return x;
            }
            else
                throw new Exception(ReadTooFar("ReadInt64"));
        }

        public ulong ReadUInt64() {
            if (filePos + 7 < fileLength) {
                ulong x = BitConverter.ToUInt64(fileBytes, filePos);
                filePos += 8;
                return x;
            }
            else
                throw new Exception(ReadTooFar("ReadInt64"));
        }

        public float ReadSingle() {
            if (filePos + 3 < fileLength) {
                float x = BitConverter.ToSingle(fileBytes, filePos);
                filePos += 4;
                return x;
            }
            else
                throw new Exception(ReadTooFar("ReadSingle"));
        }

        public double ReadDouble() {
            if (filePos + 7 < fileLength) {
                double x = BitConverter.ToDouble(fileBytes, filePos);
                filePos += 8;
                return x;
            }
            else
                throw new Exception(ReadTooFar("ReadDouble"));
        }

        public byte[] ReadBytes(int len) {
            if (filePos + len - 1 < fileLength) {
                byte[] dest = new byte[len];
                Array.Copy(fileBytes, filePos, dest, 0, len);
                filePos += len;
                return dest;
            }
            else
                throw new Exception(ReadTooFar("ReadBytes"));
        }
        
        public string ReadString() {
            if (filePos + 3 < fileLength) {
                int len = 0;
                for (int i=0; i<4; i++)
                    len = (len << 7) | fileBytes[filePos + i];
                filePos += 4;
                if (len > 0) {
                    if (filePos + len - 1 < fileLength) {
                        string s = Encoding.UTF8.GetString(ReadBytes(len));
                        return s;
                    }
                    else
                        throw new Exception(ReadTooFar("ReadString"));
                }
                else
                    return "";
            }
            else
                throw new Exception(ReadTooFar("ReadString"));
        }
        
        public int PeekChar() {
            if (filePos < fileLength)
                return (int)fileBytes[filePos];
            else
                return -1;
        }
        
    }
}
