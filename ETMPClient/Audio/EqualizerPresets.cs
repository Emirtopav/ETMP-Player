using System.Collections.Generic;

namespace ETMPClient.Audio
{
    /// <summary>
    /// Equalizer preset definitions
    /// </summary>
    public static class EqualizerPresets
    {
        // Band indices: 0=31Hz, 1=62Hz, 2=125Hz, 3=250Hz, 4=500Hz, 5=1kHz, 6=2kHz, 7=4kHz, 8=8kHz, 9=16kHz

        public static readonly Dictionary<string, float[]> Presets = new()
        {
            ["Flat"] = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            
            ["Rock"] = new float[] { 5, 4, 3, -1, -2, 1, 3, 4, 5, 5 },
            // Bass boost, mid scoop, treble boost
            
            ["Pop"] = new float[] { 2, 3, 2, 0, -1, 0, 2, 3, 3, 2 },
            // Slight bass boost, vocal presence, bright highs
            
            ["Jazz"] = new float[] { 0, 0, 0, 2, 3, 2, 1, 0, 0, 0 },
            // Warm mids, smooth highs
            
            ["Classical"] = new float[] { 0, 0, 0, 0, 0, 0, -1, -1, -2, -2 },
            // Natural, slight bass roll-off, reduced treble
            
            ["Bass Boost"] = new float[] { 8, 7, 6, 4, 2, 0, 0, 0, 0, 0 },
            // Heavy low-end emphasis
            
            ["Treble Boost"] = new float[] { 0, 0, 0, 0, 0, 0, 4, 6, 7, 8 },
            // High-end emphasis
            
            ["Vocal"] = new float[] { -2, -1, 0, 2, 4, 4, 3, 1, 0, -1 },
            // Mid-range focus for vocals
            
            ["Electronic"] = new float[] { 6, 5, 3, 0, -2, 0, 2, 4, 5, 6 },
            // Deep bass, scooped mids, bright highs
        };

        public static string[] PresetNames => new string[]
        {
            "Flat",
            "Rock",
            "Pop",
            "Jazz",
            "Classical",
            "Bass Boost",
            "Treble Boost",
            "Vocal",
            "Electronic"
        };
    }
}
