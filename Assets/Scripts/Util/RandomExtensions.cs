using System;
using System.Collections.Generic;

namespace DefaultNamespace
{
	public static class RandomExtensions
	{
		public static double NextGaussian(this Random r, double mu = 0, double sigma = 1)
		{
			var u1 = r.NextDouble();
			var u2 = r.NextDouble();

			var rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

			var rand_normal = mu + sigma * rand_std_normal;

			return rand_normal;
		}

		public static void Shuffle<T>(this IList<T> list)
		{
			var n = list.Count;
			while (n > 1)
			{
				n--;
				var k = UnityEngine.Random.Range(0, n + 1);
				(list[k], list[n]) = (list[n], list[k]);
			}
		}
	}
}