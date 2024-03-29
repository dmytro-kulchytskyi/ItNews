﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ItNews.FileProvider
{
    public class MyAsyncAutoResetEvent
    {
        private readonly Queue<TaskCompletionSource<bool>> queue = new Queue<TaskCompletionSource<bool>>();
        private bool signaled = true;

        public Task Wait()
        {
            lock (queue)
            {
                if (signaled)
                {
                    signaled = false;
                    return Task.FromResult(true);
                }
                else
                {
                    var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.PreferFairness);
                    queue.Enqueue(tcs);
                    return tcs.Task;
                }
            }
        }

        public void Set()
        {
            lock (queue)
            {
                if (queue.Count > 0)
                    queue.Dequeue().SetResult(true);
                else
                    signaled = true;
            }
        }
    }
}
