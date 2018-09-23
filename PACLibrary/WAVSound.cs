using System;
using System.Collections.Generic;
using System.Text;
using PACLibrary.Structs.WAV;
using PACLibrary.Utilities;

namespace PACLibrary
{
    public class WAVSound
    {
        /// <summary>
        /// The underlying WAVE Sound Header.
        /// </summary>
        public WaveSound SoundHeader = new WaveSound();

        /// <summary>
        /// The RAW PCM data of the sound.
        /// </summary>
        public Memory<byte> SoundData;

        /// <summary>
        /// Converts a PAC sound into a WAV sound.
        /// </summary>
        /// <param name="pacSound">PAC sound to convert from.</param>
        public static WAVSound FromPACSound(PACSound pacSound)
        {
            // Copy Over Sound Details.
            WAVSound wavSound                      = new WAVSound();
            wavSound.SoundHeader.AudioFormat       = pacSound.SoundHeader.AudioFormat;
            wavSound.SoundHeader.BitsPerSample     = pacSound.SoundHeader.BitsPerSample;
            wavSound.SoundHeader.BlockAlign        = pacSound.SoundHeader.BlockAlign;
            wavSound.SoundHeader.ByteRate          = pacSound.SoundHeader.ByteRate;
            wavSound.SoundHeader.DataSize          = pacSound.SoundHeader.DataSize;
            wavSound.SoundHeader.SampleRate        = pacSound.SoundHeader.SampleRate;
            wavSound.SoundHeader.NumberOfChannels  = pacSound.SoundHeader.NumberOfChannels;

            // Raw Data
            wavSound.SoundData              = pacSound.SoundData;

            return wavSound;
        }

        /// <summary>
        /// Creates a new WAVE sound from a specified file path.
        /// </summary>
        public static WAVSound FromFile(string filePath)
        {
            byte[] wavBytes     = System.IO.File.ReadAllBytes(filePath);
            int filePointer     = 0;

            WAVSound wavSound   = new WAVSound();
            wavSound.SoundHeader= StructUtilities.ArrayToStructure<WaveSound>(ref wavBytes, filePointer, ref filePointer);
            wavSound.SoundData  = ((Memory<byte>) (wavBytes)).Slice(filePointer, wavSound.SoundHeader.DataSize);

            return wavSound;
        }
    }
}
