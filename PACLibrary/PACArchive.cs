using System.Collections.Generic;
using PACLibrary.Structs.PAC;
using PACLibrary.Utilities;

namespace PACLibrary
{
    /// <summary>
    /// Contains a simple parser for PAC archives.
    /// </summary>
    public unsafe class PACArchive
    {
        /// <summary>
        /// Contains a list of PAC files within the current archive.
        /// </summary>
        public List<PACFile> Files = new List<PACFile>();

        /*
            ------------
            Constructors
            ------------
        */

        public PACArchive(string pacFilePath)
        {
            byte[] file = System.IO.File.ReadAllBytes(pacFilePath);
            FromByteArray(file);
        }

        public PACArchive(byte[] pacFile)
        {
            FromByteArray(pacFile);
        }

        public PACArchive() { }

        /*
            --------------        
            Core Functions
            --------------
        */
        public void SaveToFile(string filePath)
        {
            // Generate Header.
            Header fileHeader = new Header();
            fileHeader.FileCount  = Files.Count;
            fileHeader.HeaderSize = 32; // This is constant.
            fileHeader.DataOffset = 32 + (sizeof(File) * Files.Count);

            int soundDataSize = 0;
            foreach (var file in Files)
                soundDataSize += file.Sound.GetSize();

            fileHeader.DataSize = soundDataSize;

            // ------------------
            // Process all files.
            // ------------------
            
            // Setup file offsets.
            int currentFileOffset = 0;
            foreach (var file in Files)
            {
                file.File.FileOffset = currentFileOffset;
                currentFileOffset += file.Sound.GetSize();
            }

            // ----------
            // Build file
            // ----------
            List<byte> endFile = new List<byte>(fileHeader.DataOffset + fileHeader.DataSize);
            endFile.AddRange(StructUtilities.ConvertStructureToByteArray(ref fileHeader));

            foreach (var file in Files)
                endFile.AddRange(StructUtilities.ConvertStructureToByteArray(ref file.File));

            foreach (var file in Files)
            {
                endFile.AddRange(StructUtilities.ConvertStructureToByteArray(ref file.Sound.SoundHeader));
                endFile.AddRange(file.Sound.SoundData.ToArray());
            }

            System.IO.File.WriteAllBytes(filePath, endFile.ToArray());
        }

        private void FromByteArray(byte[] pacFile)
        {
            // Increases as we parse the file.
            int filePointer = 0;

            // Parse header.
            Header fileHeader   = StructUtilities.ArrayToStructure<Header>(ref pacFile, filePointer, ref filePointer);
            Files               = new List<PACFile>(fileHeader.FileCount);

            for (int x = 0; x < fileHeader.FileCount; x++)
            {
                File file = StructUtilities.ArrayToStructure<File>(ref pacFile, filePointer, ref filePointer);
                Files.Add(new PACFile(pacFile, file));
            }
        }
    }
}
