using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Offline_Support
{
    class WinImported
    {
        // ### functions imported from windows dll and turned into c#
        // puts found address to provided variable
        // returns true or false whether it found the address or not
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
        IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesRead);

        // puts found address to provided variable
        // returns true or false whether it found the address or not
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
        IntPtr hProcess, IntPtr lpBaseAddress, out IntPtr lpBuffer, int nSize, out IntPtr lpNumberOfBytesRead);

        // checking information about memory, used for faster signature scan
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress,
        out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        // structure that is used to hold memory information for use in signature scanning
        public struct MEMORY_BASIC_INFORMATION
        {
            public int BaseAddress; public int AllocationBase; public int AllocationProtect;
            public int RegionSize; public int State; public int Protect; public int lType;
        }
    }
}
