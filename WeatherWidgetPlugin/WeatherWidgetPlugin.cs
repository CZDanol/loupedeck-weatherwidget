namespace Loupedeck.WeatherWidgetPlugin
{
	using System;

	public class WeatherWidgetPlugin : Plugin
	{
		public override bool HasNoApplication => true;

		public override bool UsesApplicationApiOnly => true;

		public override void RunCommand(string commandName, string parameter) {
		}

		public override void ApplyAdjustment(string adjustmentName, string parameter, int diff) {
		}
	}
}
