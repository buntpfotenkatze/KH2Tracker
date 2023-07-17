using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KhTracker;

public class MemoryReader
{
    private const int ProcessWmRead = 0x0010;

    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(
        int dwDesiredAccess,
        bool bInheritHandle,
        int dwProcessId
    );

    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(
        int hProcess,
        long lpBaseAddress,
        byte[] lpBuffer,
        int dwSize,
        ref int lpNumberOfBytesRead
    );

    private readonly Process process;
    private readonly IntPtr processHandle;
    public readonly bool Hooked;

    public MemoryReader()
    {
        try
        {
            process = Process.GetProcessesByName("KINGDOM HEARTS II FINAL MIX")[0];
            processHandle = OpenProcess(ProcessWmRead, false, process.Id);
        }
        catch (IndexOutOfRangeException)
        {
            Hooked = false;
            return;
        }
        Hooked = true;
    }

    public byte[] ReadMemory(int address, int bytesToRead)
    {
        if (process.HasExited)
        {
            throw new Exception();
        }
        var bytesRead = 0;
        var buffer = new byte[bytesToRead];

        var processModule = process.MainModule;

        ReadProcessMemory(
            (int)processHandle,
            processModule!.BaseAddress.ToInt64() + address,
            buffer,
            buffer.Length,
            ref bytesRead
        );

        return buffer;
    }
}
