using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Timers;
using System.Web;

namespace Loupedeck.WeatherWidgetPlugin
{
	class WeatherWidgetCommand : PluginDynamicCommand
	{
		protected HttpClient httpClient = new HttpClient();
		protected IDictionary<string, WidgetData> widgetData = new Dictionary<string, WidgetData>();
		protected IDictionary<string, BitmapImage> imagesCache = new Dictionary<string, BitmapImage>();
		protected IDictionary<string, BitmapImage> weatherImagesCache = new Dictionary<string, BitmapImage>();
		protected Timer timer;

		protected class WidgetData
		{
			public string Name;
			public float Temperature = -999;
			public BitmapImage WeatherIcon;

			public bool IsValid = false;
			public bool IsLoading = false;
			public IDictionary<PluginImageSize, BitmapImage> IconsCache = new Dictionary<PluginImageSize, BitmapImage>();
		}

		public WeatherWidgetCommand() : base("_", "_", "_") {
			this.DisplayName = "Weather Widget";
			this.Description = "Shows weather info for a given area. Automatically updates every 5 minutes (or on click).";
			this.GroupName = "Weather";
			this.MakeProfileAction("text;Location and Weather API key (separate with ':', for example 'New York:10245'). You can get a free key on weatherapi.com.");

			// Reload the data periodically every 5 minutes
			timer = new Timer(60 * 5 * 1000);
			timer.Elapsed += (Object, ElapsedEventArgs) => {
				foreach (string actionParameter in new List<string>(widgetData.Keys))
					LoadData(actionParameter);
			};
			timer.AutoReset = true;
			timer.Enabled = true;
		}

		protected override void RunCommand(string actionParameter) {
			LoadData(actionParameter);
		}

		protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) {
			if (actionParameter == null)
				return null;

			WidgetData d = GetWidgetData(actionParameter);
			if (!d.IsValid)
				return null;

			if (d.IconsCache.TryGetValue(imageSize, out BitmapImage r))
				return r;

			var img = new BitmapBuilder(imageSize);
			img.Clear(BitmapColor.Black);

			if (d.WeatherIcon != null)
				img.DrawImage(d.WeatherIcon);

			img.FillRectangle(0, 0, img.Width, img.Height, new BitmapColor(0, 0, 0, 128));
			img.DrawText($"{d.Name}\n\u00A0\n{d.Temperature} °C"); // NBSP on the middle line to prevent coalescing

			BitmapImage r2 = img.ToImage();
			imagesCache[actionParameter] = r2;
			return r2;
		}

		protected WidgetData GetWidgetData(string actionParameter) {
			WidgetData d;
			if (widgetData.TryGetValue(actionParameter, out d))
				return d;

			d = new WidgetData();
			widgetData[actionParameter] = d;

			LoadData(actionParameter);

			return d;
		}

		protected async void LoadData(string actionParameter) {
			if (actionParameter == null)
				return;

			WidgetData d = GetWidgetData(actionParameter);
			if (d.IsLoading)
				return;

			d.IsLoading = true;

			try {
				string[] args = actionParameter.Split(':');
				if (args.Length < 2)
					return;

				string location = args[0].Trim(), apiKey = args[1].Trim();

				HttpResponseMessage jsonRes = await httpClient.GetAsync($"https://api.weatherapi.com/v1/current.json?key={apiKey}&q={HttpUtility.UrlEncode(location)}&aqi=no");
				JsonNode json = JsonNode.Parse(await jsonRes.Content.ReadAsStringAsync());

				d.Name = json["location"]["name"].GetValue<string>();
				d.Temperature = json["current"]["temp_c"].GetValue<float>();

				string weatherIconUrl = "https:" + json["current"]["condition"]["icon"].GetValue<string>();
				if (!weatherImagesCache.TryGetValue(weatherIconUrl, out d.WeatherIcon)) {
					HttpResponseMessage iconRes = await httpClient.GetAsync(weatherIconUrl);
					d.WeatherIcon = BitmapImage.FromArray(await iconRes.Content.ReadAsByteArrayAsync());
					weatherImagesCache[weatherIconUrl] = d.WeatherIcon;
				}

				d.IconsCache.Clear();
			}
			catch (Exception e) {
				d.Name = e.Message;
				widgetData[actionParameter] = d;
				ActionImageChanged(actionParameter);
			}
			finally {
				d.IsLoading = false;
				d.IsValid = true;
				ActionImageChanged(actionParameter);
			}
		}
	}
}
