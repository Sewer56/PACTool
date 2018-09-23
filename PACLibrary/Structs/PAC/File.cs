using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PACLibrary.Structs.PAC
{
    /// <summary>
    /// Describes an individual file entry existing after the PAC header.
    /// </summary>
    public struct File
    {
        /// <summary>
        /// The index of the bank file this entry belongs to: e.g. Bank1 = 01, Bank2 = 02
        /// </summary>
        [Description("[Unique] The index of the bank file this entry belongs to: e.g. Bank1 = 01, Bank2 = 02")]
        public byte BankIndex { get; set; }

        /// <summary>
        /// The index of the current file within the archive.
        /// </summary>
        [Description("[Unique] The index of the current file within the archive.")]
        public byte FileIndex { get; set; }

        /// <summary>
        /// Either 1 or 0 (1 byte boolean). Purpose unknown.
        /// </summary>
        [Description("Either 1 or 0 (1 byte boolean). Purpose unknown.")]
        public byte FileFlagA { get; set; }

        /// <summary>
        /// Either 1 or 0 (1 byte boolean). Purpose unknown.
        /// </summary>
        [Description("Either 1 or 0 (1 byte boolean). Purpose unknown.")]
        public byte FileFlagB { get; set; }

        /// <summary>
        /// This is the offset of the current file from the end of the file entry list.
        /// Add this to the offset in the header (DataOffset) to get absolute offset into file.
        /// </summary>
        [Browsable(false)]
        public int FileOffset { get; set; }

        /// <summary>
        /// This is a number, of something.
        /// </summary>
        [Description("This is a number, of something.")]
        public int SomeCount { get; set; }

        /// <summary>
        /// Generally size of file PLUS 32 or 34.
        /// FileFlagA appears to be always set when this is defined.
        /// </summary>
        [Description("Generally size of file PLUS 32 or 34. FileFlagA appears to be always set when this is defined.")]
        public int SizeOfFile { get; set; }

        /// <summary>
        /// Retrieves the individual <see cref="Sound"/> file from the array the original contents of the PAC file.
        /// </summary>
        /// <param name="pacFileBytes">Contains the whole PAC Archive.</param>
        /// <returns></returns>
        public Memory<byte> GetSoundBytes(byte[] pacFileBytes)
        {
            // Parse PAC Header.
            Header pacHeader        = Utilities.StructUtilities.ArrayToStructure<Header>(ref pacFileBytes);

            // Assign PAC file and slice the file.
            Memory<byte> pacFile    = pacFileBytes;
            int startOffset         = pacHeader.DataOffset + FileOffset;

            return pacFile.Slice(startOffset);
        }
    }
}
