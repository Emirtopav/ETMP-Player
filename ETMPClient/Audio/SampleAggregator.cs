using NAudio.Dsp;
using System;

namespace ETMPClient.Audio
{
    public class SampleAggregator
    {
        private Complex[] fftBuffer;
        private FftEventArgs fftArgs;
        private int fftPos;
        private int fftLength;
        private int m;

        public event EventHandler<FftEventArgs>? FftCalculated;

        public SampleAggregator(int fftLength = 1024)
        {
            this.fftLength = fftLength;
            m = (int)Math.Log(fftLength, 2.0);
            fftBuffer = new Complex[fftLength];
            fftArgs = new FftEventArgs(fftBuffer);
        }

        public void Add(float value)
        {
            if (fftPos >= fftLength)
            {
                fftPos = 0;
            }

            fftBuffer[fftPos].X = (float)(value * FastFourierTransform.HammingWindow(fftPos, fftLength));
            fftBuffer[fftPos].Y = 0;
            fftPos++;

            if (fftPos >= fftLength)
            {
                FastFourierTransform.FFT(true, m, fftBuffer);
                FftCalculated?.Invoke(this, fftArgs);
            }
        }
    }

    public class FftEventArgs : EventArgs
    {
        public Complex[] Result { get; private set; }

        public FftEventArgs(Complex[] result)
        {
            Result = result;
        }
    }
}
