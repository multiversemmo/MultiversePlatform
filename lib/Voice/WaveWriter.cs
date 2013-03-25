#region Using directives

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using log4net;
using Multiverse.Lib.LogUtil;

#endregion


namespace Multiverse.Voice
{

    ///<summary>
    ///    A class to write wave files
    ///</summary>
    public class WaveWriter {
        public static void WriteWavHeader(FileStream stream, FMOD.Sound sound, int length, Object usingFmod) {
            FMOD.SOUND_TYPE     type   = FMOD.SOUND_TYPE.UNKNOWN;
            FMOD.SOUND_FORMAT   format = FMOD.SOUND_FORMAT.NONE;
            int                 channels = 0, bits = 0, temp1 = 0;
            float               rate = 0.0f;
            float               temp = 0.0f;

            if (sound == null)
                return;
            lock(usingFmod) {
                sound.getFormat(ref type, ref format, ref channels, ref bits);
                sound.getDefaults(ref rate, ref temp, ref temp, ref temp1);
            }

            log.Info("WaveWriter.WriteWavHeader: sound format: dataLength " + length + ", type " + type + ", format " + format + 
                ", channels " + channels + ", bits " + bits + ", rate " + rate);

            stream.Seek(0, SeekOrigin.Begin);

            FmtChunk            fmtChunk  = new FmtChunk();
            DataChunk           dataChunk = new DataChunk();
            WavHeader           wavHeader = new WavHeader();
            RiffChunk           riffChunk = new RiffChunk();

            fmtChunk.chunk = new RiffChunk();
            fmtChunk.chunk.id = new char[4];
            fmtChunk.chunk.id[0]     = 'f';
            fmtChunk.chunk.id[1]     = 'm';
            fmtChunk.chunk.id[2]     = 't';
            fmtChunk.chunk.id[3]     = ' ';
            fmtChunk.chunk.size      = Marshal.SizeOf(fmtChunk) - Marshal.SizeOf(riffChunk);
            fmtChunk.wFormatTag      = 1;
            fmtChunk.nChannels       = (ushort)channels;
            fmtChunk.nSamplesPerSec  = (uint)rate;
            fmtChunk.nAvgBytesPerSec = (uint)(rate * channels * bits / 8);
            fmtChunk.nBlockAlign     = (ushort)(1 * channels * bits / 8);
            fmtChunk.wBitsPerSample  = (ushort)bits;

            dataChunk.chunk = new RiffChunk();
            dataChunk.chunk.id = new char[4];
            dataChunk.chunk.id[0]    = 'd';
            dataChunk.chunk.id[1]    = 'a';
            dataChunk.chunk.id[2]    = 't';
            dataChunk.chunk.id[3]    = 'a';
            dataChunk.chunk.size     = (int)length;

            wavHeader.chunk = new RiffChunk();
            wavHeader.chunk.id = new char[4];
            wavHeader.chunk.id[0]   = 'R';
            wavHeader.chunk.id[1]   = 'I';
            wavHeader.chunk.id[2]   = 'F';
            wavHeader.chunk.id[3]   = 'F';
            wavHeader.chunk.size    = (int)(Marshal.SizeOf(fmtChunk) + Marshal.SizeOf(riffChunk) + length);
            wavHeader.rifftype = new char[4];
            wavHeader.rifftype[0]   = 'W';
            wavHeader.rifftype[1]   = 'A';
            wavHeader.rifftype[2]   = 'V';
            wavHeader.rifftype[3]   = 'E';

            /*
                Write out the WAV header.
            */
            IntPtr wavHeaderPtr = Marshal.AllocHGlobal(Marshal.SizeOf(wavHeader));
            IntPtr fmtChunkPtr  = Marshal.AllocHGlobal(Marshal.SizeOf(fmtChunk));
            IntPtr dataChunkPtr = Marshal.AllocHGlobal(Marshal.SizeOf(dataChunk));
            byte   []wavHeaderBytes = new byte[Marshal.SizeOf(wavHeader)];
            byte   []fmtChunkBytes  = new byte[Marshal.SizeOf(fmtChunk)];
            byte   []dataChunkBytes = new byte[Marshal.SizeOf(dataChunk)];

            Marshal.StructureToPtr(wavHeader, wavHeaderPtr, false);
            Marshal.Copy(wavHeaderPtr, wavHeaderBytes, 0, Marshal.SizeOf(wavHeader));

            Marshal.StructureToPtr(fmtChunk, fmtChunkPtr, false);
            Marshal.Copy(fmtChunkPtr, fmtChunkBytes, 0, Marshal.SizeOf(fmtChunk));

            Marshal.StructureToPtr(dataChunk, dataChunkPtr, false);
            Marshal.Copy(dataChunkPtr, dataChunkBytes, 0, Marshal.SizeOf(dataChunk));

            stream.Write(wavHeaderBytes, 0, Marshal.SizeOf(wavHeader));
            stream.Write(fmtChunkBytes, 0, Marshal.SizeOf(fmtChunk));
            stream.Write(dataChunkBytes, 0, Marshal.SizeOf(dataChunk));
        }

        // WAV Structures 

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct RiffChunk
        {
            [MarshalAs(UnmanagedType.ByValArray,SizeConst=4)]
                public char[] id;
            public int 	  size;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct FmtChunk
        {
            public RiffChunk    chunk;
            public ushort	    wFormatTag;        /* format type  */
            public ushort	    nChannels;         /* number of channels (i.e. mono, stereo...)  */
            public uint	        nSamplesPerSec;    /* sample rate  */
            public uint	        nAvgBytesPerSec;   /* for buffer estimation  */
            public ushort	    nBlockAlign;       /* block size of data  */
            public ushort	    wBitsPerSample;    /* number of bits per sample of mono data */
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DataChunk
        {
            public RiffChunk   chunk;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct WavHeader
        {
            public RiffChunk   chunk;
            [MarshalAs(UnmanagedType.ByValArray,SizeConst=4)]
            public char[]      rifftype;
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(WaveWriter));
    }

}
