using Microsoft.UI.Xaml.Media.Imaging;
using RadioParadisePlayer.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadioParadisePlayer.Logic
{
    internal class ChannelView : Channel
    {
        public BitmapImage BitmapImage { get; set; }

        public ChannelView()
        {
            BitmapImage = new BitmapImage();
        }

        public ChannelView(Channel ch) : this()
        {
            Chan = ch.Chan;
            Title = ch.Title;
            Stream_Name = ch.Stream_Name;
            Image = ch.Image;
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
