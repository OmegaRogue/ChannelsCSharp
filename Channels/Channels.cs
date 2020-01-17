using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using Nito.AsyncEx;

namespace Channels
{
	public struct ChannelResult<T>
	{
		public T Val;
		public bool Ok;
	}
	
	public class Channel<T> : IEnumerable
	{
		private int _capacity;
		private Queue<T> _contents;
		private bool _closed;

		public Channel(int capacity = 1)
		{
			_capacity = capacity;
			_contents = new Queue<T>(capacity);
		}
		
#pragma warning disable 1998
		public async Task<ChannelResult<T>> PullAsync()

		{
			var output = new ChannelResult<T>{};
			
			bool received;
			do
			{
				received = _contents.TryDequeue(out output.Val);
				
				if (_closed && !received)
				{
					output.Ok = false;
					return output;
				}
			}
			while (!received);
			return output;
		}

		public ChannelResult<T> Pull()
		{
			var task = Task.Run(async () => await PullAsync());
			return task.Result;
		}

		public void Push(T value) => Task.Run(async () => await PushAsync(value));

		public async Task PushAsync(T value)
		{
			if(_closed)
				return;
			var done = _contents.Count < _capacity;
			do
			{
				try
				{
					_contents.Enqueue(value);
				}
				catch
				{
					done = false;
				}
			}
			while (!done);
		}
#pragma warning restore 1998

		public void Close() => _closed = true;

		public bool GetClosed() => _closed;
		public IEnumerator<T> GetEnumerator()
		{
			var receiving = true;
			while (receiving)
			{
				var result = Pull();
				yield return result.Val;
				receiving = result.Ok;
			}
		}
 
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}