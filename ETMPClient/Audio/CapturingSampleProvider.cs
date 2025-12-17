using NAudio.Wave;
using System;

namespace ETMPClient.Audio
{
    public class CapturingSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly SampleAggregator sampleAggregator;

        public WaveFormat WaveFormat => source.WaveFormat;

        public CapturingSampleProvider(ISampleProvider source, SampleAggregator sampleAggregator)
        {
            this.source = source;
            this.sampleAggregator = sampleAggregator;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = source.Read(buffer, offset, count);

            // Feed samples to FFT aggregator (use left channel only for mono FFT)
            for (int i = 0; i < samplesRead; i++)
            {
                sampleAggregator.Add(buffer[offset + i]);
            }

            return samplesRead;
        }
    }
}
