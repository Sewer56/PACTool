using System.Security.Cryptography;

namespace PACLibrary.Structs.PAC
{
    /// <summary>
    /// Represents the individual master header of the PAC Archives.
    /// </summary>
    public unsafe struct Header
    {
        /// <summary>
        /// This is the amount of files within this .PAC archive.
        /// </summary>
        public int FileCount;

        /// <summary>
        /// This value is fixed 32 bytes.
        /// </summary>
        public int HeaderSize;

        /// <summary>
        /// The size of the actual file raw data.
        /// This is the size of the whole file minus the second set of headers.
        /// </summary>
        public int DataSize;

        /// <summary>
        /// This is the offset to the individual file raw data within the file.
        /// </summary>
        public int DataOffset;

        public fixed byte Padding[16];
    }
}
