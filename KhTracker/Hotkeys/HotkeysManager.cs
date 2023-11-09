using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

/*
    Credits to Kettlesimulator's YouTube video for the simple solution - https://www.youtube.com/watch?v=qLxqoh1JLnM
*/

namespace KhTracker.Hotkeys;

public static class HotkeysManager
{
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private static readonly LowLevelKeyboardProc LowLevelProc = HookCallback;

    private static List<GlobalHotkey> Hotkeys { get; set; }

    private const int WhKeyboardLl = 13;

    private static IntPtr _hookId = IntPtr.Zero;

    public static bool IsHookSetup { get; set; }

    static HotkeysManager()
    {
        Hotkeys = new List<GlobalHotkey>();
    }

    public static void SetupSystemHook()
    {
        if (!IsHookSetup)
        {
            _hookId = SetHook(LowLevelProc);
            IsHookSetup = true;
        }
    }

    public static void ShutdownSystemHook()
    {
        if (IsHookSetup)
        {
            UnhookWindowsHookEx(_hookId);
            IsHookSetup = false;
        }
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using var currentProcess = Process.GetCurrentProcess();
        using var currentModule = currentProcess.MainModule;
        return SetWindowsHookEx(WhKeyboardLl, proc, GetModuleHandle(currentModule!.ModuleName), 0);
    }

    public static void AddHotkey(GlobalHotkey hotkey)
    {
        Hotkeys.Add(hotkey);
    }

    public static void RemoveHotkey(GlobalHotkey hotkey)
    {
        Hotkeys.Remove(hotkey);
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            foreach (var hotkey in Hotkeys)
            {
                try
                {
                    if (Keyboard.Modifiers == hotkey.Modifier && Keyboard.IsKeyDown(hotkey.Key))
                    {
                        if (hotkey.CanExecute)
                        {
                            hotkey.Callback?.Invoke();
                        }
                    }
                }
                catch
                {
                    MessageBox.Show(
                        "Error with hotkey!\nPost your hotkey in the rando discord for help."
                    );
                }
            }
        }

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(
        int idHook,
        LowLevelKeyboardProc lpfn,
        IntPtr hMod,
        uint dwThreadId
    );

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(
        IntPtr hhk,
        int nCode,
        IntPtr wParam,
        IntPtr lParam
    );

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}
