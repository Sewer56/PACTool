using System;
using System.Collections.Generic;
using System.Text;

namespace PACExtract
{
    class PACStructs
    {

        public struct MasterHeader // 32 Bytes
        {
            public UInt32 FileCount; // Amount of files in .pac file
            public UInt32 MasterHeaderSize; // 32 Bytes
            public UInt32 Data_Size; // Everything Past This Header & Second Set of Headers
            public UInt32 File_Data_Offset; // Offset to File Data
            // 16 Null Bytes Padding;
        }

        public struct FileHeader // 16 Bytes, Repeats FileCount Times
        {
            public byte BankID; // Bank File Number | Bank 1 = 01, Bank 2 = 02
            public byte SoundID; // Bank ID, Iterates up from 0
            public short Flags; // Some flags for each bank.
            public UInt32 FileOffset; // (Excludes Header Size)
            public UInt32 Unknown1; // Not known
            public UInt32 Unknown2; // Not known
        }

        public struct SoundFile
        {
            public ushort Audio_Format;       // 0x0 | 01
            public ushort Number_Channels;    // 0x2 | 01
            public UInt32 Sample_Rate;        // 0x4 | 22050
            public UInt32 Byte_Rate;          // 0x8 | 44100 | (Sample Rate * BitsPerSample * Channels)/8. 
            public ushort BlockAlign;         // 0xC | 02
            public ushort BitsPerSample;      // 0xE | 16

            public UInt32 Data_Size; 		   // 0x1C
            // [RAW FILE DATA]		   // 0x20 | RAW PCM DATA!
        }

        public struct WaveFile
        {
            public string WaveHeader;         // 0x00 | "RIFF"
            public int FileSize;              // 0x04 |Size of whole file.
            public string FileTypeHeader;     // 0x08 |"WAVE"
            public string FormatChunkHeader;  // 0x0C |"fmt " | Chunk Marker
            public int HeaderLength;          // 0x10 |16 | Length of fmt chunk marker

            public ushort Audio_Format;       // 0x14 | Same as SoundFile
            public ushort Number_Channels;    // 0x16 | Same as SoundFile
            public UInt32 Sample_Rate;        // 0x18 | Same as SoundFile
            public UInt32 Byte_Rate;          // 0x1C | Same as SoundFile | (Sample Rate * BitsPerSample * Channels)/8. 
            public ushort BlockAlign;         // 0x20 | Same as SoundFile
            public ushort BitsPerSample;      // 0x22 | Same as SoundFile

            public string ChunkID;            // 0x24 | "data"
            public UInt32 Data_Size;          // 0x28 | Length of data

        }

    }
}
