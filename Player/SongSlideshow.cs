using RadioParadisePlayer.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadioParadisePlayer.Player
{
    internal class SongSlideshow : IEnumerable<string>
    {
        private List<string> pictureUrlList;
        private int currentIndex = 0;

        public int Duration { get; private set; }

        public int Count => pictureUrlList.Count;

        public string this[int index] => pictureUrlList[index];

        public string CurrentPictureUrl => pictureUrlList[currentIndex];

        public SongSlideshow(ProgramBlock block, Song song)
        {
            string imgBaseUrl = block.Image_Base;
            string[] picIds = song.Slideshow.Split(",");
            Duration = song.Duration / picIds.Length; 
            pictureUrlList = picIds.Select(pid => "https:" + imgBaseUrl + @"slideshow/720/" + pid + ".jpg").ToList();
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
