using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MetaDel
{
    public class MetaDel
    {
        const int DATA_START_OFFSET = 0x24;
        const int FILESIZE_START_OFFSET = 0x28;
        const int FILESIZE_END_OFFSET = 0x32;

        const string DATA_STRING = "data";

        /// <summary>
        /// Poistaa .wav-tiedoston headerista metadatan ylikirjoittaen alkuperäisen tiedoston.
        /// </summary>
        /// <param name="args">Tiedoston polku.</param>
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("File name not specified.");
                return;
            }

            if (args[0].Length == 0)
            {
                Console.WriteLine("File name not specified.");
                return;
            }

            byte[] data = File.ReadAllBytes(args[0]);

            if (data.Length == 0) return;

            // ei tarvitse tehdä mitään
            if (ContainsString(data, DATA_START_OFFSET, FILESIZE_START_OFFSET, DATA_STRING))
            {
                Console.WriteLine("Header doesn't contain metadata.");
                return;
            }


            Console.WriteLine("Metadata found.");

            int dataStartIndex = -1;

            for (int i = FILESIZE_START_OFFSET; i < data.Length; i++)
            {
                if (ContainsString(data, i, i + DATA_STRING.Length, DATA_STRING))
                {
                    dataStartIndex = i;
                    break;
                }
            }

            if (dataStartIndex == -1)
            {
                Console.WriteLine("Header invalid ('data' subchunk header missing).");
                return;
            }

            int bytesToRemove = dataStartIndex - DATA_START_OFFSET;

            byte[] result = new byte[data.Length - bytesToRemove];

            Console.WriteLine("Removing " + bytesToRemove + " bytes of metadata...");

            // kopioidaan headerin alku
            Array.Copy(data, 0, result, 0, DATA_START_OFFSET);

            // kopioidaan loppu
            Array.Copy(data, dataStartIndex, result, DATA_START_OFFSET, data.Length - dataStartIndex);

            int bits = result.Length * 8;

            byte[] filesize = BitConverter.GetBytes(bits);

            Console.WriteLine("Updating filesize (" + bits + ") bits...");
            // asetetaan tiedostokoko
            Array.Copy(filesize, 0, result, FILESIZE_START_OFFSET, filesize.Length);

            File.WriteAllBytes(args[0], result);

            Console.WriteLine("Success!");
        }


        /// <summary>
        /// Sisältääkö taulukko merkkijonon.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startOffset">Starting byte (inclusive)</param>
        /// <param name="endOffset">Ending byte (exclusive)</param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool ContainsString(byte[] data, int startOffset, int endOffset, string str)
        {
            if (data == null) 
                throw new ArgumentException();

            if (startOffset < 0 || endOffset < 0)
                throw new ArgumentException();

            if (endOffset <= startOffset)
                throw new ArgumentException();

            if (endOffset > data.Length)
                throw new ArgumentException();

            string readString = "";

            for (int i = startOffset; i < endOffset; i++)
            {
                readString += (char)data[i];
            }

            return readString.Equals(str);
        }
    }
}