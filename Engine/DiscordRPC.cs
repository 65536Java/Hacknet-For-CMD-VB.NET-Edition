using System;
using System.Runtime.InteropServices;

namespace BCEngine
{
    // =========================================================
    // 1. 結構體 (Structs): 定義 Rich Presence 結構
    // *** FIX: 將 LPStr 改為 ByValTStr 以符合原生 DLL 結構佈局 ***
    // =========================================================

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct RichPresence
    {
        // FIX: 必須使用 ByValTStr (By-Value-T-String)
        // SizeConst 必須與原生 C 結構體的長度匹配 (Details/State = 128)
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string state; 
        
        // FIX: ByValTStr, SizeConst = 128
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string details;
        
        // FIX: ByValTStr, SizeConst = 256 (Image Key/Text = 256)
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string largeImageKey;
        
        // FIX: ByValTStr, SizeConst = 256
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string largeImageText;
    }
    // =========================================================
    // 2. P/Invoke 宣告: 宣告原生 DLL 中的 C 函式 (保持不變)
    // =========================================================
    
    public static class DiscordRPC
    {
        private const string DLL_NAME = "discord-rpc.dll";

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Discord_Initialize")]
        public static extern void Initialize(
            [MarshalAs(UnmanagedType.LPStr)] string applicationID,
            IntPtr handlers,
            bool autoRegister,
            [MarshalAs(UnmanagedType.LPStr)] string optionalSteamID);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Discord_Shutdown")]
        public static extern void Shutdown();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Discord_UpdatePresence")]
        public static extern void UpdatePresence(ref RichPresence presence);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Discord_RunCallbacks")]
        public static extern void RunCallbacks();
        
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Discord_ClearPresence")]
        public static extern void ClearPresence();
    }
}