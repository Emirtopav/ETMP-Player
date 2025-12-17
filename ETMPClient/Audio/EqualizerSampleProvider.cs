using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.Dsp;
using System;

namespace ETMPClient.Audio
{
    /// <summary>
    /// 10-band parametric equalizer using BiQuad peaking filters
    /// </summary>
    public class EqualizerSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly BiQuadFilter[,] _filters; // [channel, band]
        private readonly float[] _bandGains; // dB values for each band
        private readonly int _channels;

        // Standard 10-band frequencies (Hz)
        private static readonly float[] Frequencies = { 31f, 62f, 125f, 250f, 500f, 1000f, 2000f, 4000f, 8000f, 16000f };
        private const float Q = 1.0f; // Bandwidth

        public WaveFormat WaveFormat => _source.WaveFormat;

        public EqualizerSampleProvider(ISampleProvider source)
        {
            _source = source;
            _channels = source.WaveFormat.Channels;
            _bandGains = new float[10];
            _filters = new BiQuadFilter[_channels, 10];

            // Initialize all filters
            for (int ch = 0; ch < _channels; ch++)
            {
                for (int band = 0; band < 10; band++)
                {
                    _filters[ch, band] = BiQuadFilter.PeakingEQ(
                        source.WaveFormat.SampleRate,
                        Frequencies[band],
                        Q,
                        0f // Initial gain = 0dB (flat)
                    );
                }
            }
        }

        /// <summary>
        /// Set gain for a specific band
        /// </summary>
        /// <param name="bandIndex">Band index (0-9)</param>
        /// <param name="gainDb">Gain in dB (-12 to +12)</param>
        public void SetBandGain(int bandIndex, float gainDb)
        {
            if (bandIndex < 0 || bandIndex >= 10)
                throw new ArgumentOutOfRangeException(nameof(bandIndex));

            _bandGains[bandIndex] = Math.Clamp(gainDb, -12f, 12f);

            // Update all channel filters for this band
            for (int ch = 0; ch < _channels; ch++)
            {
                _filters[ch, bandIndex] = BiQuadFilter.PeakingEQ(
                    _source.WaveFormat.SampleRate,
                    Frequencies[bandIndex],
                    Q,
                    _bandGains[bandIndex]
                );
            }
        }

        /// <summary>
        /// Get current gain for a band
        /// </summary>
        public float GetBandGain(int bandIndex)
        {
            if (bandIndex < 0 || bandIndex >= 10)
                throw new ArgumentOutOfRangeException(nameof(bandIndex));

            return _bandGains[bandIndex];
        }

        /// <summary>
        /// Reset all bands to 0dB (flat response)
        /// </summary>
        public void Reset()
        {
            for (int band = 0; band < 10; band++)
            {
                SetBandGain(band, 0f);
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = _source.Read(buffer, offset, count);

            // Apply EQ filters to each sample
            for (int i = 0; i < samplesRead; i++)
            {
                int channel = i % _channels;
                float sample = buffer[offset + i];

                // Apply all 10 bands sequentially
                for (int band = 0; band < 10; band++)
                {
                    sample = _filters[channel, band].Transform(sample);
                }

                buffer[offset + i] = sample;
            }

            return samplesRead;
        }
    }
}
