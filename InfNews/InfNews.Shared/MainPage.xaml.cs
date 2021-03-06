﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.WebUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using HtmlAgilityPack;

// Szablon elementu Pusta strona jest udokumentowany pod adresem http://go.microsoft.com/fwlink/?LinkId=234238

namespace InfNews
{
	/// <summary>
	/// Pusta strona, która może być używana samodzielnie, lub do której można nawigować wewnątrz ramki.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		List<PurePcData> list_PurePc = new List<PurePcData>();
		List<PcLabData> list_PcLab = new List<PcLabData>();
		List<KomputerSwiatData> list_KomputerSwiat = new List<KomputerSwiatData>();

		public MainPage()
		{
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Required;
			TglButtonPurePc.IsEnabled = false;
			TglButtonPcLab.IsEnabled = false;
			TglButtonKomputerSwiat.IsEnabled = false;
			progressRing.IsActive = true;
		}

		private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
		{
			ParsingPurePc("http://www.purepc.pl");
			ParsingPcLab("http://www.pclab.pl");
			ParsingKomputerSwiat("http://www.komputerswiat.pl");
			progressRing.IsActive = false;
			TglButtonPurePc.IsEnabled = true;
			TglButtonPcLab.IsEnabled = true;
			TglButtonKomputerSwiat.IsEnabled = true;
		}

		/// <summary>
		/// Wywoływane, gdy ta strona ma być wyświetlona w ramce.
		/// </summary>
		/// <param name="e">Dane zdarzenia, opisujące, jak została osiągnięta ta strona.
		/// Ten parametr jest zazwyczaj używany do konfigurowania strony.</param>
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			// TODO: Tutaj przygotuj stronę do wyświetlenia.

			// TODO: Jeśli aplikacja zawiera wiele stron, upewnij się, że jest
			// obsługiwany przycisk Wstecz sprzętu, rejestrując
			// zdarzenie Windows.Phone.UI.Input.HardwareButtons.BackPressed.
			// Jeśli używasz obiektu NavigationHelper dostarczanego w niektórych szablonach,
			// to zdarzenie jest już obsługiwane.
		}

		private async void ParsingPurePc(string url)
		{
			try
			{
				string url_string;
				using (var client = new HttpClient())
				{
					url_string = await client.GetStringAsync(new Uri(url));
				}

				HtmlDocument htmlDocument = new HtmlDocument();
				htmlDocument.LoadHtml(url_string);

				HtmlNode node = htmlDocument.DocumentNode.Descendants("div").FirstOrDefault(o => o.GetAttributeValue("class", null) == "latest_items");
				HtmlNodeCollection nodeCollection = node.ChildNodes;

				foreach (HtmlNode itemNode in nodeCollection)
				{
					var titleAndImage = itemNode.Descendants("a").FirstOrDefault(x => x.GetAttributeValue("class", null) == "ni_image");
					if (titleAndImage != null)
					{
						var attributes = titleAndImage.Descendants("img").FirstOrDefault(x => x.GetAttributeValue("alt", "") != null);
						var title = attributes.Attributes["alt"].Value;
						var image = attributes.Attributes["src"].Value;
						//link do szczegółowych informacji, które zostaną pobrane po dwukrotnym kliknięciu na item z listBoxa
						var attributeLink = itemNode.Descendants("a").FirstOrDefault(x => x.GetAttributeValue("href", "") != null);
						var link = url + attributeLink.Attributes["href"].Value;

						list_PurePc.Add(new PurePcData(title, new BitmapImage(new Uri(image)), link));
					}
				}

				listBox.ItemsSource = list_PurePc;
			}
			catch (Exception e)
			{
				MessageDialog msgDialog = new MessageDialog(e.Message);
				msgDialog.ShowAsync();
			}
		}

		private void TglButton_OnClick(object sender, RoutedEventArgs e)
		{
			var btn = sender as ToggleButton;

			foreach (FrameworkElement item in ((Panel)btn.Parent).Children)
			{
				if (item is ToggleButton)
				{
					if (btn.Name != item.Name)
					{
						item.IsTapEnabled = !item.IsTapEnabled;
						if (item.IsTapEnabled == false)
							item.Visibility = Visibility.Collapsed;
						else
							item.Visibility = Visibility.Visible;
					}
				}
			}
		}

		private async void ListBox_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
		{
			var item = sender as ListBox;
			var index = item.SelectedIndex;
			string url = list_PurePc[index].Link;

			if (list_PurePc[index].Content == null)
			{
				try
				{
					string url_string;
					using (var client = new HttpClient())
					{
						url_string = await client.GetStringAsync(new Uri(url));
					}

					HtmlDocument htmlDocument = new HtmlDocument();
					htmlDocument.LoadHtml(url_string);

					HtmlNode node = htmlDocument.DocumentNode.Descendants("div").FirstOrDefault(o => o.GetAttributeValue("class", null) == "content clear-block");
					HtmlNodeCollection nodeCollection = node.ChildNodes;

					string content = "";
					foreach (HtmlNode itemNode in nodeCollection)
					{
						if (itemNode.Name == "p" || itemNode.Name == "h3")
						{
							content += itemNode.InnerText + "\n ";
						}
					}
					txtBlockTitle.Text = list_PurePc[index].Title;
					list_PurePc[index].Content = content;
					txtBlockContent.Text = content;
				}
				catch (Exception exception)
				{
					MessageDialog msg = new MessageDialog(exception.Message);
					msg.ShowAsync();
				}
			}
			else
			{
				txtBlockTitle.Text = list_PurePc[index].Title;
				txtBlockContent.Text = list_PurePc[index].Content;
			}
			
		}
		//PcLab
		private async void ParsingPcLab(string url)
		{
			try
			{
				string url_string;
				using (var client = new HttpClient())
				{
					/*
					 * Kodowanie na PcLab to iso-8859-2 którego nie obsługuje Windows Phone 8.1, ale obsługuje Windows 8.1
					 * Dostępne kodowania w Win Phone to: UTF8 i Unicode;
					 */
#if WINDOWS_PHONE_APP
						var response = await client.GetByteArrayAsync(new Uri(url));
						//url_string = Encoding.GetEncoding("iso-8859-2").GetString(response, 0, response.Length - 1);
						url_string = Encoding.UTF8.GetString(response, 0, response.Length - 1);
#endif
#if WINDOWS_APP
					url_string = await client.GetStringAsync(new Uri(url));
#endif
				}

				HtmlDocument htmlDocument = new HtmlDocument();
				htmlDocument.LoadHtml(url_string);

				HtmlNode node = htmlDocument.DocumentNode.Descendants("div").FirstOrDefault(o => o.GetAttributeValue("class", null) == "previews");
				HtmlNodeCollection nodeCollection = node.ChildNodes;
				
				foreach (HtmlNode itemNode in nodeCollection)
				{
					var imagelink = itemNode.Descendants("img").FirstOrDefault(x => x.GetAttributeValue("src", null) != null);
					var titleAndLink = itemNode.Descendants("a").FirstOrDefault(x => x.GetAttributeValue("href", null) != null);
					if (titleAndLink != null)
					{
						var link = url + titleAndLink.Attributes["href"].Value;
						var title = titleAndLink.InnerText;
						if (title.Contains("&#8211;")) title = title.Replace("&#8211;", "-");
						var image = url + imagelink.Attributes["src"].Value;

						list_PcLab.Add(new PcLabData(title, new BitmapImage(new Uri(image)), link));
					}
				}
					// przypisz do listboxa
				listBoxPcLab.ItemsSource = list_PcLab;
			}
			catch (Exception e)
			{
				MessageDialog msgDialog = new MessageDialog(e.Message);
				msgDialog.ShowAsync();
			}
		}

		private async void ListBoxPcLab_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
		{
			var item = sender as ListBox;
			var index = item.SelectedIndex;
			string url = list_PcLab[index].Link;

			if (list_PcLab[index].Content == null)
			{
				try
				{
					string url_string;
					using (var client = new HttpClient())
					{
						/*
						 * Kodowanie na PcLab to iso-8859-2 którego nie obsługuje Windows Phone 8.1, ale obsługuje Windows 8.1
						 * Dostępne kodowania w Win Phone to: UTF8 i Unicode;
						 */
#if WINDOWS_PHONE_APP
						var response = await client.GetByteArrayAsync(new Uri(url));
						//url_string = Encoding.GetEncoding("iso-8859-2").GetString(response, 0, response.Length - 1);
						url_string = Encoding.UTF8.GetString(response, 0, response.Length - 1);
#endif
#if WINDOWS_APP
						url_string = await client.GetStringAsync(new Uri(url));
#endif
					}

					HtmlDocument htmlDocument = new HtmlDocument();
					htmlDocument.LoadHtml(url_string);

					HtmlNode node = htmlDocument.DocumentNode.Descendants("div").FirstOrDefault(o => o.GetAttributeValue("class", null) == "substance");
					HtmlNode node2 = node.Descendants("div").FirstOrDefault(o => o.GetAttributeValue("class", null) == "data");
					HtmlNodeCollection nodeCollection = node2.ChildNodes;

					string content = "";
					foreach (HtmlNode itemNode in nodeCollection)
					{
						if (itemNode.Name == "p" || itemNode.Name == "h3")
						{
							content += itemNode.InnerText + "\n ";
							if (content.Contains("&#8211;")) content = content.Replace("&#8211;", "-");
						}
					}
					txtBlockTitlePcLab.Text = list_PcLab[index].Title;
					list_PcLab[index].Content = content;
					txtBlockContentPcLab.Text = content;
				}
				catch (Exception exception)
				{
					MessageDialog msg = new MessageDialog(exception.Message);
					msg.ShowAsync();
				}
			}
			else
			{
				txtBlockTitlePcLab.Text = list_PcLab[index].Title;
				txtBlockContentPcLab.Text = list_PcLab[index].Content;
			}
		}

		// Komputer Świat
		private async void ParsingKomputerSwiat(string url)
		{
			try
			{
				string url_string;
				using (var client = new HttpClient())
				{
					url_string = await client.GetStringAsync(new Uri(url));
				}

				HtmlDocument htmlDocument = new HtmlDocument();
				htmlDocument.LoadHtml(url_string);

				HtmlNode node = htmlDocument.DocumentNode.Descendants("div").FirstOrDefault(o => o.GetAttributeValue("id", null) == "main_topic");
				HtmlNodeCollection nodeCollection = node.ChildNodes;

				foreach (HtmlNode itemNode in nodeCollection)
				{
					var imageAndTitle = itemNode.Descendants("img").FirstOrDefault(x => x.GetAttributeValue("src", null) != null);

					if (imageAndTitle != null)
					{
						var link = url + itemNode.Attributes["href"].Value;
						var title = imageAndTitle.Attributes["alt"].Value;
						var image = url + imageAndTitle.Attributes["src"].Value;

						list_KomputerSwiat.Add(new KomputerSwiatData(title, new BitmapImage(new Uri(image)), link));
					}
				}

				listBoxKomputerSwiat.ItemsSource = list_KomputerSwiat;
			}
			catch (Exception e)
			{
				MessageDialog msgDialog = new MessageDialog(e.Message);
				msgDialog.ShowAsync();
			}
		}

		private async void ListBoxKomputerSwiat_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
		{
			var item = sender as ListBox;
			var index = item.SelectedIndex;
			string url = list_KomputerSwiat[index].Link;

			if (list_KomputerSwiat[index].Content == null)
			{
				try
				{
					string url_string;
					using (var client = new HttpClient())
					{
						url_string = await client.GetStringAsync(new Uri(url));
					}

					HtmlDocument htmlDocument = new HtmlDocument();
					htmlDocument.LoadHtml(url_string);

					HtmlNode node = htmlDocument.DocumentNode.Descendants("div").FirstOrDefault(o => o.GetAttributeValue("id", null) == "onet-ad-flat-intext");
					HtmlNodeCollection nodeCollection = node.ChildNodes;

					string content = "";
					foreach (HtmlNode itemNode in nodeCollection)
					{
						if (itemNode.Name == "p")
						{
							content += itemNode.InnerText + "\n ";
						}
					}
					txtBlockTitleKomputerSwiat.Text = list_KomputerSwiat[index].Title;
					list_KomputerSwiat[index].Content = content;
					txtBlockContentKomputerSwiat.Text = content;
				}
				catch (Exception exception)
				{
					MessageDialog msg = new MessageDialog(exception.Message);
					msg.ShowAsync();
				}
			}
			else
			{
				txtBlockTitleKomputerSwiat.Text = list_KomputerSwiat[index].Title;
				txtBlockContentKomputerSwiat.Text = list_KomputerSwiat[index].Content;
			}

		}
	}
}
