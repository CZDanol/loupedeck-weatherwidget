using System;

namespace Loupedeck.WeatherWidgetPlugin
{
	class WeatherWidgetCommand : PluginDynamicCommand
	{
		public WeatherWidgetCommand() : base("Weather Widget", "Shows weather info for a given area", "Weather") {
		}

		protected override void RunCommand(string actionParameter) {
			
		}

	}
}
