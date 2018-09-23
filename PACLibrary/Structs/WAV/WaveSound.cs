using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PACLibrary.Structs.WAV
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class WaveSound
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[]   WaveHeader = "RIFF".ToCharArray();      // 0x00 | "RIFF"
        public int      FileSize;                               // 0x04 | Size of whole file.

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[]   FileTypeHeader = "WAVE".ToCharArray();  // 0x08 | "WAVE"

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] FormatChunkHeader = "fmt ".ToCharArray(); // 0x0C | "fmt " | Chunk Marker
        public int      HeaderLength = 16;                      // 0x10 | 16 | Length of fmt chunk marker

        public ushort   AudioFormat;                            // 0x14 | Same as SoundFile
        public ushort   NumberOfChannels;                       // 0x16 | Same as SoundFile
        public int      SampleRate;                             // 0x18 | Same as SoundFile
        public int      ByteRate;                               // 0x1C | Same as SoundFile | (Sample Rate * BitsPerSample * Channels)/8. 
        public ushort   BlockAlign;                             // 0x20 | Same as SoundFile
        public ushort   BitsPerSample;                          // 0x22 | Same as SoundFile

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] ChunkID = "data".ToCharArray();           // 0x24 | "data"
        public int      DataSize;                               // 0x28 | Length of data
    }
}
