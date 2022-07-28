using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Wox.Plugin.Runner
{
    class ExplorerPathsService
    {
        public static List<string> GetOpenExplorerPaths()
        {
            Type? type = Type.GetTypeFromProgID("Shell.Application");
            if (type == null) return new List<string>();
            dynamic? shell = Activator.CreateInstance(type);
            if (shell == null) return new List<string>();
            try
            {
                var fullResults = new List<ExplorerResult>();

                var openWindows = shell.Windows();
                for (int i = 0; i < openWindows.Count; i++)
                {
                    var window = openWindows.Item(i);
                    if (window == null) continue;
                    var fileName = Path.GetFileName((string)window.FullName);

                    // Other things which are returned include the internet explorer and the classic control panel
                    // We only want file explorer windows
                    if (fileName.ToLower() == "explorer.exe")
                    {
                        string locationUrl = window.LocationURL;
                        if (!string.IsNullOrEmpty(locationUrl))
                        {
                            fullResults.Add(new ExplorerResult(new IntPtr(window.HWND), new Uri(locationUrl).LocalPath));
                        }
                    }
                }

                int zIndex = fullResults.Count;
                // EnumWindows iterates over windows in ZIndex order, basically the foremost window will be the first one
                EnumWindows((IntPtr hwnd, IntPtr param) =>
                {
                    var result = fullResults.Find(v => v.HWND == hwnd);
                    if(result != null)
                    {
                        result.ZIndex = zIndex;
                        zIndex -= 1;
                    }
                    // zIndex is also used as a counter: how many more windows do we have to find
                    return zIndex > 0;
                }, IntPtr.Zero);

                // sort descending and return the paths
                return fullResults
                    .OrderByDescending(v => v.ZIndex)
                    .Select(v => v.Path)
                    .ToList();
            }
            finally
            {
                Marshal.FinalReleaseComObject(shell);
            }
        }

        private class ExplorerResult
        {
            /// <summary>
            /// Higher values means that the window is closer to the user
            /// </summary>
            public int ZIndex { get; set; } = 0;
            public IntPtr HWND { get; }
            public string Path { get; }

            public ExplorerResult(IntPtr hwnd, string path)
            {
                HWND = hwnd;
                Path = path;
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        // Delegate to filter which windows to include 
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);


    }
}
