using System;
using UnityEngine;

namespace zs
{
    public class DebugLog : Log
    {
        public DebugLog()
        {
            #if !UNITY_STANDALONE && !UNITY_EDITOR
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
                Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            #endif
        }

        public override void Write(string message)
        {
            #if UNITY_EDITOR
            Debug.Log(PrepareMessage(message) + Environment.NewLine);
            #else
            Debug.Log(PrepareMessage(message));
            #endif
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}