using RadioParadisePlayer.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadioParadisePlayer.Logic
{
    class SongInfoViewModel : INotifyPropertyChanged
    {
        private SongInfo songInfo;

        private bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                if (isLoading != value)
                {
                    isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        private string songLyrics;

        public string SongLyrics
        {
            get { return songLyrics; }
            set
            {
                if (songLyrics != value)
                {
                    songLyrics = value;
                    OnPropertyChanged(nameof(SongLyrics));
                }
            }
        }

        private string songWikiInfo;

        public string SongWikiInfo
        {
            get { return songWikiInfo; }
            set
            {
                if (songWikiInfo != value)
                {
                    songWikiInfo = value;
                    OnPropertyChanged(nameof(songWikiInfo));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task LoadSongInfo(Song song, string userId)
        {
            IsLoading = true;
            try
            {
                songInfo = await RpApiClient.GetSongInfoAsync(song, userId);

                SongLyrics = songInfo.Lyrics
                    .Replace(@"<br />", Environment.NewLine)
                    .Replace("\r\r", Environment.NewLine);
                SongWikiInfo = songInfo.WikiHtml;
            }
            catch
            {
                //ignore the error
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void Clear()
        {
            songWikiInfo = "";
            SongLyrics = "";
        }
    }
}
