using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PACLibrary.Structs.PAC
{
    /// <summary>
    /// Represents an individual sound file pointed to by the PAC File Header.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public unsafe struct Sound
    {
        /* The following are identical to the components of a WAV header.
           Search info on WAV headers should you be interested. */

        // 0 Bytes
        public ushort AudioFormat;       // 0x0 | 01
        public ushort NumberOfChannels;  // 0x2 | 01
        public int SampleRate;           // 0x4 | 22050
        public int ByteRate;             // 0x8 | 44100 | (Sample Rate * BitsPerSample * Channels)/8. 
        public ushort BlockAlign;        // 0xC | 02
        public ushort BitsPerSample;     // 0xE | 16

        // 16 Bytes
        public fixed byte Padding[12];
        public int DataSize;             // 0x1C

        // 32 Bytes
        // RAW PCM Data Follows Here; Packed 32

    }
}
