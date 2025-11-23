using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace BCEngine
{
    // ------------------------------------------------------------------
    // I. NativeMethods: 封裝所有 Windows API 宣告
    // ------------------------------------------------------------------
    // 必須使用 public static class
    public static class NativeMethods
    {
        // 必須是 public
        public const int STD_OUTPUT_HANDLE = -11;

        // 必須是 public
        [StructLayout(LayoutKind.Sequential)]
        public struct COORD
        {
            public short X;
            public short Y;
        }

        // 必須是 public
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct CONSOLE_FONT_INFOEX
        {
            public uint cbSize;
            public uint nFont;
            public COORD dwFontSize;
            public int FontFamily;
            public int FontWeight;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string FaceName;
        }

        // 必須是 public
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        // 必須是 public
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCurrentConsoleFontEx(
            IntPtr hConsoleOutput,
            bool bMaximumWindow,
            ref CONSOLE_FONT_INFOEX lpConsoleCurrentFontEx);
    }
    public class ConsoleUtil
    {
        public static void PrintWithDelay(string str)
        {
            for (int index = 0; index < str.Length; index++)
            {
                Console.Write(str[index]);
                Thread.Sleep(30);
            }
            Console.WriteLine("");
        }
    }
    public class ConsoleFont
    {
        /// <summary>
        /// 嘗試設定 Console 視窗的字體和大小。
        /// </summary>
        public static bool SetFont(string fontName, int fontSizeY)
        {
            try
            {
                // 這裡調用 NativeMethods 內部的 public 成員
                IntPtr hConsole = NativeMethods.GetStdHandle(NativeMethods.STD_OUTPUT_HANDLE);
                if (hConsole == IntPtr.Zero) return false;

                NativeMethods.CONSOLE_FONT_INFOEX fontInfo = new NativeMethods.CONSOLE_FONT_INFOEX();
                
                fontInfo.cbSize = (uint)Marshal.SizeOf(fontInfo);
                fontInfo.dwFontSize.Y = (short)fontSizeY;
                fontInfo.FaceName = fontName;             
                
                return NativeMethods.SetCurrentConsoleFontEx(hConsole, false, ref fontInfo);
            }
            catch (Exception)
            {
                // 捕捉任何潛在的 .NET 例外
                return false; 
            }
        }
        public static bool SetColor(ConsoleColor foreground, ConsoleColor background, bool clear = false)
        {
            try
            {
                Console.ForegroundColor = foreground;
                Console.BackgroundColor = background;
                if (clear)
                {
                    Console.Clear();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}