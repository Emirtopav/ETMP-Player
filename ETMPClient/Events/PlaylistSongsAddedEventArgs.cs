using ETMPData.DataEntities;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETMPClient.Events
{
    public class PlaylistSongsAddedEventArgs : EventArgs
    {
        public IEnumerable<MediaEntity> Songs { get; }

        public PlaylistSongsAddedEventArgs(IEnumerable<MediaEntity> songs)
        {
            Songs = songs;
        }
    }
}
