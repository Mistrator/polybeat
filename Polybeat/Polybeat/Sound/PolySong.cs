using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;
using System.Diagnostics;
using System.IO;
using CSCore;
using CSCore.SoundOut;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.DirectSound;

namespace Polybeat
{
    /// <summary>
    /// .wav-muotoinen kappale ja sen data.
    /// </summary>
    public class PolySong
    {
        public string FullPath { get; private set; }

        public string Filename { get; private set; }

        public string FilenameWithoutExtension { get; private set; }

        private IWaveSource soundSource;
        private ISoundOut soundOut;

        public float SecondsPlayed
        {
            get
            {
                TimeSpan pos = soundSource.GetPosition();
                return pos.Hours * 60.0f * 60.0f + pos.Minutes * 60.0f + pos.Seconds + (pos.Milliseconds / 1000.0f);
            }
        }

        private float[] beatMoments;
        private Onset[] onsets;

        private float[] frequencyMoments;
        private float[] frequencies;

        private float[] onsetFrequencies;

        private int currentBeatIndex = 0;
        private int currentOnsetIndex = 0;
        private int currentFrequencyIndex = 0;

        public int LastOnsetClicked = 0;

        public event Action OnsetMissed;

        /// <summary>
        /// Kerroin puolisävelaskeleen muutokselle äänen taajuudessa.
        /// == root12(2)
        /// </summary>
        const float HALF_STEP_FREQUENCY_CHANGE = 1.059463094f;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">Kappaleen täysi hakemistopolku.</param>
        public PolySong(string path)
        {
            FullPath = path;
            Filename = FileSystemHelper.GetFilename(FullPath);
            FilenameWithoutExtension = FileSystemHelper.RemoveExtension(Filename);


            while (!MusicDataRetriever.UpdateComplete)
            {
                // biisien dataa ei voi ladata ennen kuin se on olemassa
            }

            LoadFromFile(FullPath);
            LoadSongData();
        }

        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// Lataa kappaleen tiedostosta.
        /// </summary>
        /// <param name="fileName">Tiedoston nimi.</param>
        private void LoadFromFile(string fileName)
        {
            soundSource = CodecFactory.Instance.GetCodec(fileName);
            soundOut = GetSoundOut();

            ChooseSoundDevice();
            soundOut.Initialize(soundSource);
        }

        private void LoadSongData()
        {
            string beatPath = MusicDataRetriever.SONG_DATA_FOLDER + FilenameWithoutExtension + MusicDataRetriever.BEAT_NAME;
            string onsetPath = MusicDataRetriever.SONG_DATA_FOLDER + FilenameWithoutExtension + MusicDataRetriever.ONSET_NAME;
            string frequencyPath = MusicDataRetriever.SONG_DATA_FOLDER + FilenameWithoutExtension + MusicDataRetriever.PITCH_NAME;

            string[] beatValues = File.ReadAllLines(beatPath);
            string[] onsetValues = File.ReadAllLines(onsetPath);
            string[] frequencyValues = File.ReadAllLines(frequencyPath);

            beatMoments = GetFloatValues(beatValues, 0);
            onsets = CreateOnsets(GetFloatValues(onsetValues, 0));
            frequencyMoments = GetFloatValues(frequencyValues, 0);
            frequencies = GetFloatValues(frequencyValues, 1);

            GetFrequenciesForOnsets(frequencyMoments, frequencies, onsets, 40.0f, 2000.0f);

            // poistetaan piikkejä taajuuksista
            // SmoothenArray(frequencies, 20, 40.0f, 2000.0f);

            //SmoothenTimings(beatMoments, 100);
        }

        private ISoundOut GetSoundOut()
        {
            if (WasapiOut.IsSupportedOnCurrentPlatform)
                return new WasapiOut();
            else
                return new DirectSoundOut();
        }

        private void ChooseSoundDevice()
        {
            using (MMDeviceEnumerator enumerator = new MMDeviceEnumerator())
            {
                if (soundOut is WasapiOut)
                {
                    ((WasapiOut)soundOut).Device = enumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active)[0];
                }
                else
                {
                    ((DirectSoundOut)soundOut).Device = DirectSoundDeviceEnumerator.EnumerateDevices().First().Guid;
                }
            }
        }

        private float[] GetFloatValues(string[] source, int column)
        {
            if (source == null) return null;
            if (column < 0) return null;

            float[] destination = new float[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                string[] columns = source[i].Split(' ');

                destination[i] = float.Parse(columns[column], System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo);
            }

            return destination;
        }

        private Onset[] CreateOnsets(float[] moments)
        {
            Onset[] onsets = new Onset[moments.Length];

            for (int i = 0; i < moments.Length; i++)
            {
                onsets[i] = new Onset(moments[i], false);
            }

            return onsets;
        }

        /// <summary>
        /// Tasoittaa piikkejä taulukon lukuarvoista ottamalla keskiarvo toisiaan 
        /// lähellä olevista luvuista ja poistamalla erittäin korkeat ja matalat piikit.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="chunkSize">Miten monen luvun palasia käsitellään</param>
        /// <param name="lowCutoff">Pienemmät arvot poistetaan</param>
        /// <param name="highCutoff">Suuremmat arvot poistetaan</param>
        private void SmoothenArray(float[] array, int chunkSize, float lowCutoff, float highCutoff)
        {
            if (chunkSize > array.Length) return;

            for (int i = 0; i < array.Length; i += chunkSize)
            {
                float average = 0.0f;
                int count = 0;

                for (int j = 0; j < chunkSize; j++)
                {
                    if (i + j >= array.Length)
                        break;

                    if (lowCutoff < array[i + j] && array[i + j] < highCutoff)
                    {
                        average += array[i + j];
                        count++;
                    }
                }

                average /= count;

                for (int j = 0; j < chunkSize; j++)
                {
                    if (i + j >= array.Length)
                        break;

                    array[i + j] = average;
                }
            }
        }

        private void SmoothenTimings(float[] arr, int chunkSize)
        {
            if (chunkSize > arr.Length) return;

            for (int i = 0; i < arr.Length; i += chunkSize)
            {
                float startingTime = arr[i];

                int steps = chunkSize;
                if (i + (chunkSize - 1) >= arr.Length)
                {
                    steps = arr.Length - 1 - i;
                }

                float endingTime = arr[i + steps];
                float increase = (endingTime - startingTime) / steps;

                for (int j = 0; j < steps; j++)
                {
                    arr[i + j] = startingTime + j * increase;
                }
            }
        }

        private void GetFrequenciesForOnsets(float[] freqTimes, float[] frequencies, Onset[] onsets, float lowCutoff, float highCutoff)
        {
            onsetFrequencies = new float[onsets.Length];
            int freqEndComplementIndex = 0;

            for (int i = 0; i < onsets.Length - 1; i++)
            {
                float startTime = onsets[i].Time;
                float endTime = onsets[i + 1].Time;

                int freqStartComplementIndex = Array.BinarySearch(freqTimes, startTime);
                freqEndComplementIndex = Array.BinarySearch(freqTimes, endTime);

                if (freqStartComplementIndex < 0)
                    freqStartComplementIndex = ~freqStartComplementIndex;
                if (freqEndComplementIndex < 0)
                    freqEndComplementIndex = ~freqEndComplementIndex;

                freqEndComplementIndex--;

                float avgFrequency = GetAverageFrequency(frequencies, freqStartComplementIndex, freqEndComplementIndex, lowCutoff, highCutoff);

                onsetFrequencies[i] = avgFrequency;
            }

            onsetFrequencies[onsetFrequencies.Length - 1] = GetAverageFrequency(frequencies, freqEndComplementIndex + 1, frequencies.Length, lowCutoff, highCutoff);
        }

        private float GetAverageFrequency(float[] freqs, int start, int end, float lowCutoff, float highCutoff)
        {
            int added = 0;
            float freqSum = 0.0f;

            for (int i = start; i < end; i++)
            {
                if (lowCutoff < freqs[i] || freqs[i] < highCutoff)
                {
                    freqSum += frequencies[i];
                    added++;
                }
            }

            return freqSum / added;
        }

        public float GetCurrentFrequency()
        {
            if (soundOut.PlaybackState != PlaybackState.Playing) return 0.0f;

            int index = Array.BinarySearch(frequencyMoments, SecondsPlayed);

            if (index < 0)
                index = ~index;
            return frequencies[index];
        }

        /// <summary>
        /// Aloittaa toiston alusta.
        /// </summary>
        public void Play()
        {
            soundOut.Play();
        }

        /// <summary>
        /// Pysäyttää toiston.
        /// </summary>
        public void Stop()
        {
            soundOut.Stop();
        }

        /// <summary>
        /// Keskeyttää toiston tilapäisesti, jatka Continue-metodilla.
        /// </summary>
        public void Pause()
        {
            soundOut.Pause();
        }

        /// <summary>
        /// Jatkaa toistoa siitä mihin se jäi.
        /// </summary>
        public void Continue()
        {
            soundOut.Resume();
        }

        /// <summary>
        /// Palauttaa kappaleessa tapahtuneet muutokset edelliseen päivitykseen nähden.
        /// </summary>
        /// <returns></returns>
        public SongState Update()
        {
            SongState state = new SongState(false, false);

            if (currentBeatIndex < beatMoments.Length)
            {
                state.IsBeat = SecondsPlayed > beatMoments[currentBeatIndex];
                if (state.IsBeat)
                    currentBeatIndex++;
            }

            if (currentOnsetIndex < onsets.Length)
            {
                state.IsOnset = SecondsPlayed > onsets[currentOnsetIndex].Time;

                if (state.IsOnset)
                {
                    if (currentOnsetIndex > 0 && OnsetMissed != null)
                    {
                        if (!onsets[currentOnsetIndex - 1].Clicked)
                        {
                            OnsetMissed();
                            onsets[currentOnsetIndex - 1].Clicked = true; // ettei tule penaltya enemmän kuin kerran
                        }
                    }

                }

                if (TimeToClosestOnset().Item2 > currentOnsetIndex)
                {
                    currentOnsetIndex++;
                }
            }

            return state;
        }

        /// <summary>
        /// Aika lähimpään onsetiin.
        /// </summary>
        /// <returns></returns>
        public Tuple<float, int> TimeToClosestOnset()
        {
            int cOnsetIndex = currentOnsetIndex < onsets.Length ? currentOnsetIndex : onsets.Length - 1;

            float current = SecondsPlayed - onsets[cOnsetIndex].Time;
            float next = (cOnsetIndex + 1) < onsets.Length ? onsets[cOnsetIndex + 1].Time - SecondsPlayed : float.MaxValue;

            if (current < next)
                return new Tuple<float, int>(Math.Abs(current), cOnsetIndex);

            return new Tuple<float, int>(Math.Abs(next), cOnsetIndex + 1);
        }

        public void SetClicked(int onsetIndex)
        {
            onsets[onsetIndex].Clicked = true;
        }

        public bool IsClicked(int onsetIndex)
        {
            return onsets[onsetIndex].Clicked;
        }
    }
}