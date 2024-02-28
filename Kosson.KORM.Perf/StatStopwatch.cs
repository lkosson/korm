using System.Diagnostics;

namespace Kosson.KORM.Perf;

class StatStopwatch
{
	private readonly List<double> measurements;
	private readonly Stopwatch sw;

	public StatStopwatch()
	{
		measurements = new List<double>(20000);
		sw = new Stopwatch();
	}

	public void Start()
	{
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
		var dev = Math.Sqrt(measurements.Select(m => (m - avg) * (m - avg)).Sum() / measurements.Count);
		measurements.Sort();
		var p0 = measurements[0];
		var p1 = measurements[measurements.Count * 1 / 10];
		var p5 = measurements[measurements.Count * 5 / 10];
		var p7 = measurements[measurements.Count * 7 / 10];
		var p9 = measurements[measurements.Count * 9 / 10];
		int steps = 20;
		var hstart = p0;
		var hend = measurements[measurements.Count * 99 / 100]; // p9;
		var hstep = (hend - hstart) / steps;
		if (hstep < 0.001)
		{
			hstep = 0.001;
			steps = (int)((hend - hstart) / hstep);
			if (steps < 1) steps = 1;
		}
		var hdata = new int[steps];
		var hmax = 1;
		for (int i = 0; i < steps; i++)
		{
			var llim = hstart + i * hstep;
			var ulim = hstart + (i + 1) * hstep;
			var c = measurements.Where(m => m >= llim && (i == steps - 1 || m < ulim)).Count();
			hdata[i] = c;
			if (c > hmax) hmax = c;
		}
		var hist = "";
		for (int i = 0; i < steps; i++)
		{
			hist += (hstart + hstep * i).ToString("0.000").PadLeft(10) + " ms\t" + hdata[i].ToString().PadLeft(3) + "\t" + (hdata[i] == 0 ? "" : hdata[i] * 50 / hmax == 0 ? "." : "".PadLeft(hdata[i] * 50 / hmax, '*')) + "\n";
		}
		return $"total\t{sum,10:0.000} ms\navg\t{avg,10:0.000} ms\ndev\t{dev,10:0.000} ms\np1\t{p1,10:0.000} ms\np5\t{p5,10:0.000} ms\np9\t{p9,10:0.000} ms\n{hist}";
	}
}
