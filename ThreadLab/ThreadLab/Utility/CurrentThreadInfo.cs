using System.Runtime.InteropServices;

namespace ThreadLab.Utility
{
    internal class CurrentThreadInfo
    {
        [DllImport("kernel32.dll")]
        private static extern int GetCurrentThreadId();

        public static int CurrentManagedThreadId => Thread.CurrentThread.ManagedThreadId;

        public static bool IsBackgroundThread => Thread.CurrentThread.IsBackground;

        public static int GetCurrentThreadOsId => GetCurrentThreadId();
    }
}
