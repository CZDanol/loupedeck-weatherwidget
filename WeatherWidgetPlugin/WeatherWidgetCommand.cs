using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Web;

namespace Loupedeck.WeatherWidgetPlugin
{
	class WeatherWidgetCommand : PluginDynamicCommand
	{
		protected HttpClient httpClient = new HttpClient();
		protected IDictionary<string, WidgetData> widgetData = new Dictionary<string, WidgetData>();

		protected class WidgetData
		{
			public string Name { get; set; }
			public float Temperature { get; set; }

			public BitmapImage Icon { get; set; }
		}

		public WeatherWidgetCommand() : base("_", "_", "_") {
			this.DisplayName = "Weather Widget";
			this.Description = "Shows weather info for a given area";
			this.GroupName = "Weather";
			this.MakeProfileAction("text;Location and Weather API key (separate with ':', for example 'New York:10245'). You can get a free key on weatherapi.com.");
		}

		protected override void RunCommand(string actionParameter) {
			LoadData(actionParameter);
		}

		protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) {
			if (actionParameter == null)
				return base.GetCommandImage(actionParameter, imageSize);

			WidgetData d;

			// No widget data -> request load
			if (!widgetData.TryGetValue(actionParameter, out d)) {
				LoadData(actionParameter);
				return base.GetCommandImage(actionParameter, imageSize);
			}

			// Data is loading -> wait for load, this function will get called again
			if (d == null)
				return base.GetCommandImage(actionParameter, imageSize);

			var img = new BitmapBuilder(imageSize);
			img.Clear(BitmapColor.Black);
			img.DrawImage(d.Icon);
			img.FillRectangle(0, 0, img.Width, img.Height, new BitmapColor(0, 0, 0, 96));
			img.DrawText($"{d.Name}\n\u00A0\n{d.Temperature} °C"); // NBSP on the middle line to prevent collation
			return img.ToImage();
		}

		protected async void LoadData(string actionParameter) {
			if (actionParameter == null)
				return;

			WidgetData d;
			// Data exists and is null -> data is loading
			if (widgetData.TryGetValue(actionParameter, out d) && d == null)
				return;

			// Mark that the data is loading
			widgetData[actionParameter] = null;
			d = new WidgetData();

			try {
				string[] args = actionParameter.Split(':');
				if (args.Length < 2)
					return;

				string location = args[0].Trim(), apiKey = args[1].Trim();

				HttpResponseMessage jsonRes = await httpClient.GetAsync($"https://api.weatherapi.com/v1/current.json?key={apiKey}&q={HttpUtility.UrlEncode(location)}&aqi=no");
				JsonNode json = JsonNode.Parse(await jsonRes.Content.ReadAsStringAsync());

				d.Name = json["location"]["name"].GetValue<string>();
				d.Temperature = json["current"]["temp_c"].GetValue<float>();

				HttpResponseMessage iconRes = await httpClient.GetAsync("https:" + json["current"]["condition"]["icon"].GetValue<string>());
				d.Icon = BitmapImage.FromArray(await iconRes.Content.ReadAsByteArrayAsync());

				widgetData[actionParameter] = d;
				ActionImageChanged(actionParameter);
			}
			catch(Exception e) {
				d.Name = e.Message;
				widgetData[actionParameter] = d;
				ActionImageChanged(actionParameter);
			}
		}

	}
}
