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
			public string Region { get; set; }
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
			if(d == null)
				return base.GetCommandImage(actionParameter, imageSize);

			var img = new BitmapBuilder(imageSize);
			img.DrawText($"{d.Name}\n{d.Region}");
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

			string[] args = actionParameter.Split(':');
			if (args.Length < 2)
				return;

			string location = args[0].Trim(), apiKey = args[1].Trim();

			HttpResponseMessage res = await httpClient.GetAsync($"https://api.weatherapi.com/v1/current.json?key={apiKey}&q={HttpUtility.UrlEncode(location)}&aqi=no");
			JsonNode json = JsonNode.Parse(await res.Content.ReadAsStringAsync());

			d = new WidgetData();
			d.Name = json["location"]["name"].GetValue<string>();
			d.Region = json["location"]["region"].GetValue<string>();

			widgetData[actionParameter] = d;
			ActionImageChanged(actionParameter);
		}

	}
}
