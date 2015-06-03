using System;
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
		public MainPage()
		{
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Required;
		}

		private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
		{
			ParsingPurePc("http://www.purepc.pl");
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

		private void TglButtonPurePc_OnClick(object sender, RoutedEventArgs e)
		{

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
	}
}
