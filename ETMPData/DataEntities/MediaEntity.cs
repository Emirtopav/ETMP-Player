using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETMPData.DataEntities
{
    public class MediaEntity : BaseEntity
    {
        [MaxLength(256)]
        public string? FilePath { get; set; }
        public virtual int? PlayerlistId { get; set; }
        public virtual PlaylistEntity? Playerlist { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        private string? _title;
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? Title 
        { 
            get 
            {
                 if (string.IsNullOrEmpty(_title) && !string.IsNullOrEmpty(FilePath))
                    return System.IO.Path.GetFileNameWithoutExtension(FilePath);
                return _title ?? "Unknown Title";
            }
            set { _title = value; OnPropertyChanged(); } 
        }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        private string? _artist;
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? Artist 
        { 
            get => _artist; 
            set { _artist = value; OnPropertyChanged(); } 
        }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        private string? _album;
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? Album 
        { 
            get => _album; 
            set { _album = value; OnPropertyChanged(); } 
        }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        private byte[]? _coverArtData;
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public byte[]? CoverArtData 
        { 
            get => _coverArtData; 
            set { _coverArtData = value; OnPropertyChanged(); } 
        }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public TimeSpan Duration { get; set; }
    }
}
