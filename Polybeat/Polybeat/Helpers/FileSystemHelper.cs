using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Polybeat
{
    public static class FileSystemHelper
    {
        /// <summary>
        /// Palauttaa tiedostonimen ilman päätettä.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string RemoveExtension(string filename)
        {
            for (int i = filename.Length - 1; i >= 0; i--)
            {
                if (filename[i] == '.')
                    return filename.Substring(0, i);
            }
            return String.Empty;
        }

        /// <summary>
        /// Irrottaa tiedostonimen kokonaisesta hakemistopolusta.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static string GetFilename(string fullPath)
        {
            for (int i = fullPath.Length - 1; i >= 0; i--)
            {
                if (fullPath[i] == '\\')
                    return fullPath.Substring(i + 1, fullPath.Length - (i + 1));
            }
            return String.Empty;
        }
    }
}