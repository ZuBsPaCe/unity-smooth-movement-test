using System;
using System.Diagnostics;
using UnityEngine;

namespace zs
{
    public abstract class Log : IDisposable
    {
        private static Stopwatch _totalStopwatch = Stopwatch.StartNew();

        public Log()
        {
            RestartStopwatch();
        }

        public void RestartStopwatch()
        {
            _totalStopwatch.Restart();
        }

        public abstract void Write(string message);

        protected string PrepareMessage(string message)
        {
            return message;
        }

        protected abstract void Dispose(bool disposing);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
