using System;
using System.Collections.Generic;
using System.Timers;

namespace Loupedeck.WeatherWidgetPlugin
{
	class ClockCommand : PluginDynamicCommand
	{
		protected Timer timer;

		public ClockCommand() : base() {
			const string group = "Clock";
			AddParameter("HH:mm", "Time (24h)", group);
			AddParameter("d. M.", "Date", group);
			AddParameter("d. M.\ndddd", "Date (with day of week)", group);

			// Update icons periodically
			timer = new Timer(10 * 1000);
			timer.Elapsed += (Object, ElapsedEventArgs) => {
				ActionImageChanged();
			};
			timer.AutoReset = true;
			timer.Enabled = true;
		}

		protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) {
			var img = new BitmapBuilder(imageSize);
			img.Clear(BitmapColor.Black);
			img.DrawText(DateTime.Now.ToString(actionParameter));
			return img.ToImage();
		}
	}
}
