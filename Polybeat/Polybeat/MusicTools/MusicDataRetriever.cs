using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Polybeat
{
    /// <summary>
    /// Luo tiedostot kappaleiden rytmistä, taajuudesta ja nuoteista.
    /// </summary>
    public static class MusicDataRetriever
    {
        public static readonly string SONG_FOLDER = Main.PROGRAM_ROOT_DIRECTORY + "Songs\\";
        public static readonly string SONG_DATA_FOLDER = SONG_FOLDER + "SongData\\";
        public static readonly string MUSICTOOLS_FOLDER = Main.PROGRAM_ROOT_DIRECTORY + "MusicTools\\";

        public static string WAV_EXTENSION = ".wav";

        public const string BEAT_NAME = "_beat.dat";
        public const string ONSET_NAME = "_onset.dat";
        public const string PITCH_NAME = "_pitch.dat";

        private static List<Process> pending = new List<Process>();

        public static bool UpdateComplete
        {
            get { return pending.Count == 0; }
        }

        public static void UpdateSongDataFiles()
        {
            string[] songs = Directory.GetFiles(SONG_FOLDER, "*" + WAV_EXTENSION);

            if (!Directory.Exists(SONG_DATA_FOLDER))
                Directory.CreateDirectory(SONG_DATA_FOLDER);

            for (int i = 0; i < songs.Length; i++)
            {
                bool b = false;
                bool o = false;
                bool p = false;

                string songFilename = FileSystemHelper.GetFilename(songs[i]);
                string songFilenameWithoutExtension = FileSystemHelper.RemoveExtension(songFilename);

                b = File.Exists(SONG_DATA_FOLDER + songFilenameWithoutExtension + BEAT_NAME);
                o = File.Exists(SONG_DATA_FOLDER + songFilenameWithoutExtension + ONSET_NAME);
                p = File.Exists(SONG_DATA_FOLDER + songFilenameWithoutExtension + PITCH_NAME);

                if (b == o == p == true)
                    continue;

                if (b == o == p == false)
                {
                    // kappale juuri lisätty, käsitellään headeri
                    ProcessStartInfo mdInfo = new ProcessStartInfo(MUSICTOOLS_FOLDER + "MetaDel.exe", songs[i]);
                    mdInfo.CreateNoWindow = true;
                    Process metadel = Process.Start(mdInfo);
                    metadel.EnableRaisingEvents = true;
                    pending.Add(metadel);
                    metadel.Exited += delegate {
                        CreateDataFiles(songFilename, true, true, true);
                        pending.Remove(metadel);
                    };
                }
                else CreateDataFiles(songFilename, b, o, p);    
            }
        }

        private static void CreateDataFiles(string songName, bool beat, bool onset, bool pitch)
        {
            if (songName == string.Empty)
                return;

            string songNameWithoutExtension = FileSystemHelper.RemoveExtension(songName);
            string songPath = SONG_FOLDER + songName;

            // dem copypasta :/
            if (beat)
            {
                ProcessStartInfo info = new ProcessStartInfo("cmd.exe");
                string dBmin = "-90.0";
                info.Arguments = "/C " + MUSICTOOLS_FOLDER + "aubiotrack.exe -i " + songPath + " -s " + dBmin + " > " + SONG_DATA_FOLDER + songNameWithoutExtension + BEAT_NAME;
                info.CreateNoWindow = false;
                info.RedirectStandardOutput = false;
                info.UseShellExecute = false;
                Process p = new Process();
                p.StartInfo = info;
                p.Start();

                pending.Add(p);
                p.EnableRaisingEvents = true;
                p.Exited += delegate 
                {
                    pending.Remove(p);
                };
            }

            if (onset)
            {
                ProcessStartInfo info = new ProcessStartInfo("cmd.exe");
                info.Arguments = "/C " + MUSICTOOLS_FOLDER + "aubioonset.exe -i " + songPath + " -O complex" + " > " + SONG_DATA_FOLDER + songNameWithoutExtension + ONSET_NAME;
                info.CreateNoWindow = false;
                info.RedirectStandardOutput = false;
                info.UseShellExecute = false;
                Process p = new Process();
                p.StartInfo = info;
                p.Start();

                pending.Add(p);
                p.EnableRaisingEvents = true;
                p.Exited += delegate
                {
                    pending.Remove(p);
                };

            }

            if (pitch)
            {
                ProcessStartInfo info = new ProcessStartInfo("cmd.exe");
                info.Arguments = "/C " + MUSICTOOLS_FOLDER + "aubiopitch.exe -i " + songPath + " -p mcomb -l 0.5" + " > " + SONG_DATA_FOLDER + songNameWithoutExtension + PITCH_NAME;
                info.CreateNoWindow = false;
                info.RedirectStandardOutput = false;
                info.UseShellExecute = false;
                Process p = new Process();
                p.StartInfo = info;
                p.Start();

                pending.Add(p);
                p.EnableRaisingEvents = true;
                p.Exited += delegate
                {
                    pending.Remove(p);
                };

            }
        }
    }
}