using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Media.Imaging;

namespace InfNews
{
	public class PurePcData
	{
		public string Title { get; set; }
		public BitmapImage Image { get; set; }
		public string Link { get; set; }
		public string Content { get; set; }

		public PurePcData(string title, BitmapImage image, string link)
		{
			Title = title;
			Image = image;
			Link = link;
		}

	}
}
