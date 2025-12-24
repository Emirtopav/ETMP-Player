using System;

namespace ETMPClient.Models
{
    public class MidiFileInfo
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public int TrackCount { get; set; }
        public int ChannelCount { get; set; }
    }
}
