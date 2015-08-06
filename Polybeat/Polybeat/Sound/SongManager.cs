using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Polybeat
{
    public static class SongManager
    {
        /// <summary>
        /// Songs-kansiossa olevien kappaleiden nimet tiedostopäätteineen. Ei koko polkua.
        /// </summary>
        public static string[] Songs { get; private set; }

        public static PolySong CurrentSong { get; private set; }

        public static void FindSongs()
        {
            string[] songFPs = Directory.GetFiles(MusicDataRetriever.SONG_FOLDER, "*" + MusicDataRetriever.WAV_EXTENSION);

            Songs = new string[songFPs.Length];

            for (int i = 0; i < songFPs.Length; i++)
            {
                Songs[i] = FileSystemHelper.GetFilename(songFPs[i]);
            }
        }

        public static void LoadSong(string name)
        {
            if (Array.IndexOf(Songs, name) == -1)
                return;
            if (CurrentSong != null)
            {
                CurrentSong.Dispose();
            }

            CurrentSong = new PolySong(MusicDataRetriever.SONG_FOLDER + name);
        }

        public static void Dispose()
        {
            if (CurrentSong != null)
                CurrentSong.Dispose();
        }
    }
}
