using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.KORM.Scratch
{
	class StatStopwatch
	{
		private List<double> measurements;
		private Stopwatch sw;

		private StatStopwatch()
		{
			measurements = new List<double>(20000);
			sw = Stopwatch.StartNew();
		}

		public static StatStopwatch StartNew()
		{
			return new StatStopwatch();
		}

		public void AddMeasurement()
		{
			Stop();
			sw.Restart();
		}

		public void Stop()
		{
			sw.Stop();
			measurements.Add(sw.ElapsedTicks * 1000d / Stopwatch.Frequency);
		}

		public override string ToString()
		{
			var sum = measurements.Sum();
			var avg = measurements.Average();
			var dev = Math.Sqrt(measurements.Select(m => (m - avg) * (m - avg)).Sum());
			measurements.Sort();
			var p0 = measurements[0];
			var p1 = measurements[measurements.Count * 1 / 10];
			var p5 = measurements[measurements.Count * 5 / 10];
			var p7 = measurements[measurements.Count * 7 / 10];
			var p9 = measurements[measurements.Count * 9 / 10];
			int steps = 20;
			var hstart = p0;
			var hend = p7;
			var hstep = (hend - hstart) / steps;
			var hdata = new int[steps];
			var hmax = 1;
			for (int i = 0; i < steps; i++)
			{
				var llim = hstart + i * hstep;
				var ulim = hstart + (i + 1) * hstep;
				var c = measurements.Where(m => m >= llim && m < ulim).Count();
				hdata[i] = c;
				if (c > hmax) hmax = c;
			}
			var hist = "";
			for (int i = 0; i < steps; i++)
			{
				hist += (hstart + hstep * i).ToString("0.000") + "\t" + hdata[i].ToString() + "\t" + "".PadLeft(hdata[i] * 50 / hmax, '*') + "\n";
			}
			return "total\t" + sum.ToString("0.000") + "ms\navg\t" + avg.ToString("0.00") + "ms\ndev\t" + dev.ToString("0.00") + "ms\np1\t" + p1.ToString("0.00") + "ms\np5\t" + p5.ToString("0.00") + "ms\np9\t" + p9.ToString("0.00") + "ms\n" + hist;
		}
	}
}
