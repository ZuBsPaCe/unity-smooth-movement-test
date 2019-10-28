using System.IO;
using UnityEngine;

namespace zs
{
    public class FileLog : Log
    {
        private TextWriter _textWriter;

        public FileLog()
        {
            string logPath = Path.Combine(Application.dataPath, @"..\SmoothMovement.log");
            _textWriter = File.CreateText(logPath);
        }

        public override void Write(string message)
        {
            _textWriter.WriteLine(PrepareMessage(message));
        }

        protected override void Dispose(bool disposing)
        {
            _textWriter?.Dispose();
            _textWriter = null;
        }
    }
}