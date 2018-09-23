using System;
using System.Collections.Generic;
using System.Text;
using PACLibrary.Structs.PAC;
using PACLibrary.Utilities;

namespace PACLibrary
{
    public class PACSound
    {
        /// <summary>
        /// The underlying PAC Sound Header.
        /// </summary>
        public Sound SoundHeader;

        /// <summary>
        /// The RAW PCM data of the sound.
        /// </summary>
        public Memory<byte> SoundData;
        
        /// <summary>
        /// Converts a WAV sound into a PAC sound.
        /// </summary>
        /// <param name="wavSound">WAV sound to convert from.</param>
        public static PACSound FromWAVSound(WAVSound wavSound)
        {
            // Copy Over Sound Details.
            PACSound pacSound                      = new PACSound();
            pacSound.SoundHeader.AudioFormat       = wavSound.SoundHeader.AudioFormat;
            pacSound.SoundHeader.BitsPerSample     = wavSound.SoundHeader.BitsPerSample;
            pacSound.SoundHeader.BlockAlign        = wavSound.SoundHeader.BlockAlign;
            pacSound.SoundHeader.ByteRate          = wavSound.SoundHeader.ByteRate;
            pacSound.SoundHeader.DataSize          = wavSound.SoundHeader.DataSize;
            pacSound.SoundHeader.SampleRate        = wavSound.SoundHeader.SampleRate;
            pacSound.SoundHeader.NumberOfChannels  = wavSound.SoundHeader.NumberOfChannels;

            // Raw Data
            pacSound.SoundData              = wavSound.SoundData;

            return pacSound;
        }

        /// <summary>
        /// Creates a PAC sound from a supplied byte array.
        /// The byte array should start at the start of the Sound file, and can end at any place; as long as the entirety
        /// of the RAW PCM data is contained inside.
        /// </summary>
        public static PACSound FromByteArray(Memory<byte> byteArray)
        {
            PACSound pacSound       = new PACSound();
            int      filePointer    = 0;

            pacSound.SoundHeader    = StructUtilities.ArrayToStructure<Sound>(ref byteArray, filePointer, ref filePointer);
            pacSound.SoundData      = byteArray.Slice(filePointer, pacSound.SoundHeader.DataSize);

            return pacSound;
        }

        /// <summary>
        /// Retrieves the complete size (in bytes) of the Sound. This includes both the header and the raw bytes.
        /// </summary>
        public unsafe int GetSize()
        {
            return sizeof(Sound) + SoundData.Length;
        }
    }
}
