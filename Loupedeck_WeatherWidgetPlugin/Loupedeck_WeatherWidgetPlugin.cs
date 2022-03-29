namespace Loupedeck.Loupedeck_WeatherWidgetPlugin
{
	using System;

	public class Loupedeck_WeatherWidgetPlugin : Plugin
	{
		public override bool HasNoApplication => true;

		public override bool UsesApplicationApiOnly => true;

		public override void Load() {
			this.Info.Icon16x16 = EmbeddedResources.ReadImage(EmbeddedResources.FindFile("partly_cloudy_day_16px.png"));
			this.Info.Icon32x32 = EmbeddedResources.ReadImage(EmbeddedResources.FindFile("partly_cloudy_day_32px.png"));
			this.Info.Icon48x48 = EmbeddedResources.ReadImage(EmbeddedResources.FindFile("partly_cloudy_day_48px.png"));
			this.Info.Icon256x256 = EmbeddedResources.ReadImage(EmbeddedResources.FindFile("partly_cloudy_day_96px.png"));
		}

		public override void RunCommand(string commandName, string parameter) {
		}

		public override void ApplyAdjustment(string adjustmentName, string parameter, int diff) {
		}
	}
}
