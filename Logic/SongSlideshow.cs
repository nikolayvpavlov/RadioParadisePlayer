using RadioParadisePlayer.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadioParadisePlayer.Logic
{
    internal class SongSlideshow : IEnumerable<string>
    {
        private List<string> pictureUrlList;
        private int currentIndex = 0;

        public int Count => pictureUrlList.Count;

        public string this[int index] => pictureUrlList[index];

        public string CurrentPictureUrl => pictureUrlList[currentIndex];

        public SongSlideshow(Playlist block, Song song)
        {
            pictureUrlList = song.Slideshow.Select(pid => "https:" + block.Image_Base + block.Slideshow_Path + pid + ".jpg").ToList();
        }

        public void MoveNext()
        {
            if (currentIndex < Count-1) currentIndex++;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return ((IEnumerable<string>)pictureUrlList).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)pictureUrlList).GetEnumerator();
        }
    }
}
