using System;

namespace Loupedeck.WeatherWidgetPlugin
{
	class WeatherWidgetCommand : PluginDynamicCommand
	{
		public WeatherWidgetCommand() {
			this.DisplayName = "Weather Widget";
			this.Description = "Shows information about weather in a given area";
			this.GroupName = "Weather";
		}

		protected override bool ProcessButtonEvent(string actionParameter, DeviceButtonEvent buttonEvent) {
			Console.WriteLine($"Button event {actionParameter} - {buttonEvent}");
			return true;
		}
	}
}
