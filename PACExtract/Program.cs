using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PACExtract
{
    class Program
    {
        public static string FilePath; // The path to the file provided by the user.
        public static byte Action = 0; // What am I going to do?

        public static List<byte> PACFile; // The file we are going to Write
        public static byte[] PACFileArray; // The file we are going to Load

        public static string DirectoryPath; // File path of the directory we will write files in.
        public static string FileName; // Name of file

        // public static bool DebugMode; // Verbose?

        static void Main(string[] args)
        {

            // Check Arguments
            for (int x = 0; x < args.Length; x++)
            {
                if (args[x] == ("-f") | args[x] == ("--file")) { FilePath = args[x + 1]; } // File Path
                else if (args[x] == ("-e") | args[x] == ("--extract")) { Action = 1; }
                else if (args[x] == ("-c") | args[x] == ("--convert")) { Action = 2; }
                // else if (args[x] == ("-v") | args[x] == ("--verbose")) { DebugMode = true; }
            }

            switch (Action)
            {
                case 1:
                    ExtractPACFile();
                    break;
                case 2:
                    ConvertPACFile();
                    break;
                default:
                    ShowHelp();
                    break;
            }
            Console.WriteLine("Complete");
        }

        public static void ShowHelp()
        {
            Console.WriteLine("\n-------------------");
            Console.WriteLine("PACTool by Sewer56lol");
            Console.WriteLine("---------------------");
            Console.WriteLine("Usage ( .pac => .wav): PACTool.exe --extract --file <PACFile>");
            Console.WriteLine("Usage ( .wav => .pac): PACTool.exe --convert --file <WAVDirectory>");
            Console.WriteLine("There is a text file containing various pieces of information about the, wav exports\n" +
                             "please do not remove it.");
            Console.ReadLine();
        }

        public static void ConvertPACFile()
        {
            DirectoryPath = FilePath; // Path to the directory.
            FileName = Path.GetFileName(DirectoryPath) + ".pac"; // Get File Name
            string[] WaveFilesInDirectory = Directory.GetFiles(DirectoryPath + @"\", "*.wav"); // Current Files in Directory

            List<List<byte>> BankFiles = new List<List<byte>>(WaveFilesInDirectory.Length); // All BANK files.
            PACStructs.FileHeader[] BankFileHeaders = new PACStructs.FileHeader[WaveFilesInDirectory.Length]; // Yay

            byte Nullbyte = 0x00; // Used for writing 0x00, literally.

            // Generate Heroes RAW Audio Data, Read Bank Headers
            for (int x = 0; x < WaveFilesInDirectory.Length; x++)
            {
                byte[] WaveFile = File.ReadAllBytes(WaveFilesInDirectory[x]); // Get the wave file.

                /// Write Heroes Audio File Header
                PACStructs.SoundFile HeroesAudioFile = new PACStructs.SoundFile(); 
                HeroesAudioFile.Audio_Format = BitConverter.ToUInt16(WaveFile, 0x14);
                HeroesAudioFile.Number_Channels = BitConverter.ToUInt16(WaveFile, 0x16);
                HeroesAudioFile.Sample_Rate = BitConverter.ToUInt32(WaveFile, 0x18);
                HeroesAudioFile.Byte_Rate = BitConverter.ToUInt32(WaveFile, 0x1c);
                HeroesAudioFile.BlockAlign = BitConverter.ToUInt16(WaveFile, 0x20);
                HeroesAudioFile.BitsPerSample = BitConverter.ToUInt16(WaveFile, 0x22);
                HeroesAudioFile.Data_Size = BitConverter.ToUInt32(WaveFile, 0x28);

                // Write the file
                List<byte> HeroesPCMBankFile = new List<byte>((int)HeroesAudioFile.Data_Size + 0x20); // Header + Data Length

                // Time to write the file
                HeroesPCMBankFile.AddRange(BitConverter.GetBytes(HeroesAudioFile.Audio_Format));
                HeroesPCMBankFile.AddRange(BitConverter.GetBytes(HeroesAudioFile.Number_Channels));
                HeroesPCMBankFile.AddRange(BitConverter.GetBytes(HeroesAudioFile.Sample_Rate));
                HeroesPCMBankFile.AddRange(BitConverter.GetBytes(HeroesAudioFile.Byte_Rate));
                HeroesPCMBankFile.AddRange(BitConverter.GetBytes(HeroesAudioFile.BlockAlign));
                HeroesPCMBankFile.AddRange(BitConverter.GetBytes(HeroesAudioFile.BitsPerSample));

                for (int z = 0; z < 12; z++) { HeroesPCMBankFile.Add(Nullbyte); } // 12 null bytes, padding.

                HeroesPCMBankFile.AddRange(BitConverter.GetBytes(HeroesAudioFile.Data_Size));
                HeroesPCMBankFile.AddRange(WaveFile.Skip(0x2C)); // Append RAW Audio Data

                int ThirtyTwoKBPadding = (int)HeroesAudioFile.Data_Size;
                int BytesToPad = 0;
                while (ThirtyTwoKBPadding % 32 != 0) { ThirtyTwoKBPadding += 1; BytesToPad += 1; } // Pad the file to be divisible by 32KB
                for (int z = 0; z < BytesToPad; z++) { HeroesPCMBankFile.Add((byte)0x00); }

                BankFiles.Add(HeroesPCMBankFile);

                // Now for the BANK Files
                string WavePath = WaveFilesInDirectory[x].Substring(0, WaveFilesInDirectory[x].Length - 3);
                byte[] BankFile = File.ReadAllBytes(WavePath + "BankEntry"); // Get the bank entry file.
                PACStructs.FileHeader BankEntryHeader = new PACStructs.FileHeader();

                BankEntryHeader.BankID = BankFile[0]; //0x00
                BankEntryHeader.SoundID = BankFile[1]; //0x01
                BankEntryHeader.Flags = BitConverter.ToInt16(BankFile, 2); //0x02
                // Offset is calculated later, 0x04
                BankEntryHeader.Unknown1 = BitConverter.ToUInt32(BankFile, 0x08); // 0x08
                BankEntryHeader.Unknown2 = BitConverter.ToUInt32(BankFile, 0x0C); // 0x0C

                // Assign Header to Array
                BankFileHeaders[x] = BankEntryHeader;
            }

            // Write The Master Header
            PACStructs.MasterHeader PACHeader = new PACStructs.MasterHeader();
            PACHeader.FileCount = (uint)BankFiles.Count;
            PACHeader.MasterHeaderSize = 0x20; // Header size is constant.

            int DataSize = 0;
            for (int x = 0; x < BankFiles.Count; x++) { DataSize += BankFiles[x].Count; }
            PACHeader.Data_Size = (uint)DataSize;

            // Header size + file header entries.
            PACHeader.File_Data_Offset = 0x20 + (uint)(0x10*BankFiles.Count);

            // Calculate Offsets
            uint InitialPointerOffset = 0; // Initial offset to start counting data offsets from.
            for (int x = 0; x < BankFileHeaders.Length; x++)
            {
                BankFileHeaders[x].FileOffset = InitialPointerOffset;
                InitialPointerOffset += (uint)BankFiles[x].Count;
            }

            Directory.CreateDirectory(DirectoryPath + @"\" + Path.GetFileName(DirectoryPath)); // Create directory for output files
            List<byte> PACFile = new List<byte>((int)PACHeader.Data_Size + (int)PACHeader.File_Data_Offset); // File goes here.

            // Write the Header
            PACFile.AddRange(BitConverter.GetBytes(PACHeader.FileCount));
            PACFile.AddRange(BitConverter.GetBytes(PACHeader.MasterHeaderSize));
            PACFile.AddRange(BitConverter.GetBytes(PACHeader.Data_Size));
            PACFile.AddRange(BitConverter.GetBytes(PACHeader.File_Data_Offset));
            for (int x = 0; x < 16; x++) { PACFile.Add((byte)0x00); } /// 16 null bytes

            // Write File Headers
            for (int x = 0; x < BankFileHeaders.Length; x++)
            {
                PACFile.Add(BankFileHeaders[x].BankID);
                PACFile.Add(BankFileHeaders[x].SoundID);
                PACFile.AddRange(BitConverter.GetBytes(BankFileHeaders[x].Flags));
                PACFile.AddRange(BitConverter.GetBytes(BankFileHeaders[x].FileOffset));
                PACFile.AddRange(BitConverter.GetBytes(BankFileHeaders[x].Unknown1));
                PACFile.AddRange(BitConverter.GetBytes(BankFileHeaders[x].Unknown2));
            }

            // Add the individual sound files
            for (int x = 0; x < BankFiles.Count; x++)
            {
                PACFile.AddRange(BankFiles[x]);
            }

            File.WriteAllBytes(DirectoryPath + @"\" + Path.GetFileName(DirectoryPath) + @"\" + FileName, PACFile.ToArray());
        }

        public static void ExtractPACFile()
        {
            PACFileArray = File.ReadAllBytes(FilePath); // Populate the PAC File into List
            DirectoryPath = Path.GetDirectoryName(FilePath); // Path to the opened file.
            FileName = Path.GetFileNameWithoutExtension(FilePath); // Get File Name
            Directory.CreateDirectory(DirectoryPath + @"\" + FileName); // Create directory for output files

            // Read File
            int FileCount = BitConverter.ToInt32(PACFileArray, 0); // Amount of files.
            int CurrentCursorPosition = 0x20; // This is going to serve as a place from where we will read the array. 0x20 is length of header.
            int[] FileOffsets = new Int32[FileCount]; // Array of offsets.

            // if (DebugMode) { Console.WriteLine("File Count: " + FileCount + "\n"); }

            // Iterate over all offsets.
            for (int x = 0; x < FileCount; x++)
            {
                File.WriteAllBytes(DirectoryPath + @"\" + FileName + @"\Sound_" + x.ToString("D3") + ".BankEntry", GetArrayRange(CurrentCursorPosition, PACFileArray, 0x10)); // Write Entry File
                FileOffsets[x] = BitConverter.ToInt32(PACFileArray, CurrentCursorPosition + 4); // Read the file offset which is 0x4 from the header entry.
                // if (DebugMode) { Console.WriteLine("File Offset Added: " + FileOffsets[x]); }
                CurrentCursorPosition += 0x10; // Iterate the current cursor position to the nest file.
            }

            int StartingDataOffset = CurrentCursorPosition; // Offset from which the data is based from, i.e. After Finishing Reading Headers
            // if (DebugMode) { Console.WriteLine("\nStarting Offset: " + CurrentCursorPosition + "\n"); }

            // Go to the file data and export each file.
            for (int x = 0; x < FileOffsets.Length; x++)
            {
                PACStructs.SoundFile HeroesAudioFile = new PACStructs.SoundFile(); // Properties of a specific sound file
                int FileOffset = StartingDataOffset + FileOffsets[x]; // Direct Offset to file.

                // Populate properties of Heroes Audio File
                HeroesAudioFile.Audio_Format = BitConverter.ToUInt16(PACFileArray, FileOffset);
                HeroesAudioFile.Number_Channels = BitConverter.ToUInt16(PACFileArray, FileOffset + 0x02);
                HeroesAudioFile.Sample_Rate = BitConverter.ToUInt32(PACFileArray, FileOffset + 0x04);
                HeroesAudioFile.Byte_Rate = BitConverter.ToUInt32(PACFileArray, FileOffset + 0x08);
                HeroesAudioFile.BlockAlign = BitConverter.ToUInt16(PACFileArray, FileOffset + 0x0C);
                HeroesAudioFile.BitsPerSample = BitConverter.ToUInt16(PACFileArray, FileOffset + 0x0E);
                HeroesAudioFile.Data_Size = (uint)BitConverter.ToInt32(PACFileArray, FileOffset + 0x1C);

                // 0x2C: Header Length
                List<byte> WaveFile = new List<byte>((int)HeroesAudioFile.Data_Size + 0x2C); // Create a new Wave File

                // Write Wave Header. || See PACStructs.cs
                WaveFile.AddRange(Encoding.ASCII.GetBytes("RIFF"));
                WaveFile.AddRange(BitConverter.GetBytes(HeroesAudioFile.Data_Size + 0x2C)); // 0x2C: Header Length
                WaveFile.AddRange(Encoding.ASCII.GetBytes("WAVE"));
                WaveFile.AddRange(Encoding.ASCII.GetBytes("fmt "));
                WaveFile.AddRange(BitConverter.GetBytes((int)16)); //Length of chunk marker info.

                WaveFile.AddRange(BitConverter.GetBytes(HeroesAudioFile.Audio_Format));
                WaveFile.AddRange(BitConverter.GetBytes(HeroesAudioFile.Number_Channels));
                WaveFile.AddRange(BitConverter.GetBytes(HeroesAudioFile.Sample_Rate));
                WaveFile.AddRange(BitConverter.GetBytes(HeroesAudioFile.Byte_Rate));
                WaveFile.AddRange(BitConverter.GetBytes(HeroesAudioFile.BlockAlign));
                WaveFile.AddRange(BitConverter.GetBytes(HeroesAudioFile.BitsPerSample));

                WaveFile.AddRange(Encoding.ASCII.GetBytes("data"));
                WaveFile.AddRange(BitConverter.GetBytes(HeroesAudioFile.Data_Size));

                WaveFile.AddRange(GetArrayRange(FileOffset + 0x20, PACFileArray, (int)HeroesAudioFile.Data_Size)); // 0x20: Header of Heroes Sound File

                File.WriteAllBytes(DirectoryPath + @"\" + FileName + @"\Sound_" + x.ToString("D3") + ".wav", WaveFile.ToArray()); // Write File
            }
        }

        // Simply copies all bytes within a range A-B in an array.
        public static byte[] GetArrayRange(int SourceIndex, byte[] SourceArray, int Count)
        {
            byte[] NewArray = new Byte[Count];
            for (int x = 0; x < Count; x++)
            { NewArray[x] = SourceArray[SourceIndex + x]; }
            return NewArray;
        }
    }
}
