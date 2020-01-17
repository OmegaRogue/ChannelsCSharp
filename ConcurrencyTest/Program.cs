using System;
using System.Threading.Tasks;

using Channels;

namespace ConcurrencyTest
{
	class Program
	{
		public static Channel<string> StringBuffer;
		static void Main()
		{
			var startTime = DateTime.Now;
			StringBuffer = new Channel<string>(100);
			StringBuffer.Push("Start Time: " + startTime);
			Work();
			var secondTime = DateTime.Now;
			var duration = secondTime - startTime;
			StringBuffer.Push("Time After Starting Job: " + secondTime + " Duration: " + duration);

			foreach (var received in StringBuffer)
			{
				Console.WriteLine(received);
			}
		}


		static async Task Work()
		{
			var startTime = DateTime.Now;
			await StringBuffer.PushAsync("Task Start Time: " + startTime);
			await Task.Delay(5000);
			var endTime = DateTime.Now;
			var duration = endTime - startTime;
			await StringBuffer.PushAsync("Task End Time: " + endTime +" Duration: " + duration);
			StringBuffer.Close();
		}
	}
}