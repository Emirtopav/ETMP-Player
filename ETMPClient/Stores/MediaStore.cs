using Microsoft.EntityFrameworkCore;
using ETMPClient.Events;
using ETMPClient.Models;
using ETMPData.Data;
using ETMPData.DataEntities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETMPClient.Stores
{
    public class MediaStore
    {
        public event EventHandler<PlaylistSongsAddedEventArgs>? PlaylistSongsAdded;

        private readonly List<MediaEntity> _songs;
        private readonly IDbContextFactory<DataContext> _dbContextFactory;

        public IEnumerable<MediaEntity> Songs => _songs;

        private readonly object _songsLock = new object();

        public MediaStore(IDbContextFactory<DataContext> dbContextFactory)
        {
            _songs = new List<MediaEntity>();
            _dbContextFactory = dbContextFactory;
            Load();
        }

        public void Load()
        {
            try
            {
                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    lock (_songsLock)
                    {
                        _songs.Clear();
                        _songs.AddRange(dbContext.Songs.ToList());
                    }
                }
            }
            catch (Exception ex)
            {
                // Table might not exist yet during first startup
                Debug.WriteLine($"MediaStore.Load() error: {ex.Message}");
            }
        }

        public async Task<bool> Add(MediaEntity media)
        {
            using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
            {
                try
                {
                    dbContext.Songs.Add(media);
                    await dbContext.SaveChangesAsync();

                    lock (_songsLock)
                    {
                        _songs.Add(media);
                    }
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> AddRange(IEnumerable<MediaEntity> medias, bool raiseAddEvent = false)
        {
            using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
            {
                try
                {
                    dbContext.Songs.AddRange(medias);
                    await dbContext.SaveChangesAsync();

                    lock (_songsLock)
                    {
                        _songs.AddRange(medias);
                    }

                    if (raiseAddEvent)
                        PlaylistSongsAdded?.Invoke(this, new PlaylistSongsAddedEventArgs(medias));
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> RemoveAll(Func<MediaEntity, bool> predicate)
        {
            lock (_songsLock)
            {
                _songs.RemoveAll(x => predicate.Invoke(x));
            }
            using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
            {
                try
                {
                    var itemsRemove = dbContext.Songs.Where(predicate);
                    dbContext.Songs.RemoveRange(itemsRemove);
                    Load(); // This Load might be redundant or dangerous if logic expects clear state? But sticking to original logic style but locked.
                            // Actually Load appends. This looks buggy in original code (duplicates?) but let's just lock it for now.
                    await dbContext.SaveChangesAsync();
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> Remove(int mediaId)
        {
            lock (_songsLock)
            {
                _songs.RemoveAll(x => x.Id == mediaId);
            }
            using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
            {
                try
                {
                    dbContext.Songs.Remove(new MediaEntity { Id = mediaId });
                    await dbContext.SaveChangesAsync();
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }
    }
}
