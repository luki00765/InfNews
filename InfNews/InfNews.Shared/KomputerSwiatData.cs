using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Media.Imaging;

namespace InfNews
{
    class KomputerSwiatData
    {
		public string Title { get; set; }
		public BitmapImage Image { get; set; }
		public string Link { get; set; }
		public string Content { get; set; }

		public KomputerSwiatData(string title, BitmapImage image, string link)
		{
			Title = title;
			Image = image;
			Link = link;
		}
    }
}
