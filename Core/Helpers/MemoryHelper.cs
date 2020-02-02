using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using MhwOverlay.Log;

namespace MhwOverlay.Core.Helpers
{
    public static class MemoryHelper
    {
        static WindowsApi.RegionPageProtection[] ProtectionExclusions
        {
            get
            {
                return new WindowsApi.RegionPageProtection[]
                {
                    WindowsApi.RegionPageProtection.PAGE_GUARD,
                    WindowsApi.RegionPageProtection.PAGE_NOACCESS
                };
            }
        }

        static bool CheckProtection(BytePattern pattern, uint flags)
        {
            var protectionFlags = (WindowsApi.RegionPageProtection)flags;

            foreach (var protectionExclusion in ProtectionExclusions)
            {
                if (protectionFlags.HasFlag(protectionExclusion))
                {
                    return false;
                }
            }

            return true;
        }

        public static ulong? FindPatternAddresses(Process process, BytePattern pattern)
        {
            List<ulong> matchAddresses = new List<ulong>();
            var addressRange = pattern.AddressRange;
            ulong currentAddress = addressRange.Start;

            while (currentAddress < addressRange.End && !process.HasExited)
            {
                WindowsApi.MEMORY_BASIC_INFORMATION64 memoryRegion;
                if (WindowsApi.VirtualQueryEx(process.Handle, (IntPtr)currentAddress, out memoryRegion, (uint)Marshal.SizeOf(typeof(WindowsApi.MEMORY_BASIC_INFORMATION64))) > 0
                    && memoryRegion.RegionSize > 0
                    && memoryRegion.State == (uint)WindowsApi.RegionPageState.MEM_COMMIT
                    && CheckProtection(pattern, memoryRegion.Protect))
                {
                    var regionStartAddress = memoryRegion.BaseAddress;
                    if (addressRange.Start > regionStartAddress)
                    {
                        regionStartAddress = addressRange.Start;
                    }

                    var regionEndAddress = memoryRegion.BaseAddress + memoryRegion.RegionSize;
                    if (addressRange.End < regionEndAddress)
                    {
                        regionEndAddress = addressRange.End;
                    }

                    ulong regionBytesToRead = regionEndAddress - regionStartAddress;
                    byte[] regionBytes = new byte[regionBytesToRead];

                    if (process.HasExited)
                    {
                        break;
                    }

                    int lpNumberOfBytesRead = 0;
                    WindowsApi.ReadProcessMemory(process.Handle, (IntPtr)regionStartAddress, regionBytes, regionBytes.Length, ref lpNumberOfBytesRead);

                    var matchIndices = FindPatternMatchIndices(regionBytes, pattern);

                    foreach (var matchIndex in matchIndices)
                    {
                        var matchAddress = regionStartAddress + (ulong)matchIndex;
                        matchAddresses.Add(matchAddress);

                        Logger.Log(LogLevel.Info, $"Found '{pattern.Config.Name}' at address 0x{matchAddress.ToString("X8")}");

                        break;

                    }
                }

                if (matchAddresses.Any())
                {
                    break;
                }

                currentAddress = memoryRegion.BaseAddress + memoryRegion.RegionSize;
            }

            return matchAddresses.Count > 0 ? matchAddresses[0] : (ulong?)null;
        }

        // KMP algorithm modified to search bytes with a nullable/wildcard search
        static List<int> FindPatternMatchIndices(byte[] bytes, BytePattern pattern)
        {
            List<int> matchedIndices = new List<int>();

            int textLength = bytes.Length;
            int patternLength = pattern.Bytes.Length;

            if (textLength == 0 || patternLength == 0 || textLength < patternLength)
            {
                return matchedIndices;
            }

            int[] longestPrefixSuffices = new int[patternLength];

            GetLongestPrefixSuffices(pattern, ref longestPrefixSuffices);

            int textIndex = 0;
            int patternIndex = 0;

            while (textIndex < textLength)
            {
                if (!pattern.Bytes[patternIndex].HasValue // Ignore compare if the pattern index is nullable - this treats it like a * wildcard
                    || bytes[textIndex] == pattern.Bytes[patternIndex])
                {
                    textIndex++;
                    patternIndex++;
                }

                if (patternIndex == patternLength)
                {
                    matchedIndices.Add(textIndex - patternIndex);
                    patternIndex = longestPrefixSuffices[patternIndex - 1];

                    break;

                }
                else if (textIndex < textLength
                    && (pattern.Bytes[patternIndex].HasValue // Only compare disparity if the pattern byte isn't a null wildcard
                    && bytes[textIndex] != pattern.Bytes[patternIndex]))
                {
                    if (patternIndex != 0)
                    {
                        patternIndex = longestPrefixSuffices[patternIndex - 1];
                    }
                    else
                    {
                        textIndex++;
                    }
                }
            }

            return matchedIndices;
        }

        static void GetLongestPrefixSuffices(BytePattern pattern, ref int[] longestPrefixSuffices)
        {
            int patternLength = pattern.Bytes.Length;
            int length = 0;
            int patternIndex = 1;

            longestPrefixSuffices[0] = 0;

            while (patternIndex < patternLength)
            {
                if (pattern.Bytes[patternIndex] == pattern.Bytes[length])
                {
                    length++;
                    longestPrefixSuffices[patternIndex] = length;
                    patternIndex++;
                }
                else
                {
                    if (length == 0)
                    {
                        longestPrefixSuffices[patternIndex] = 0;
                        patternIndex++;
                    }
                    else
                    {
                        length = longestPrefixSuffices[length - 1];
                    }
                }
            }
        }

        public static T Read<T>(Process process, ulong address) where T : struct
        {
            byte[] bytes = new byte[Marshal.SizeOf(typeof(T))];

            int lpNumberOfBytesRead = 0;
            WindowsApi.ReadProcessMemory(process.Handle, (IntPtr)address, bytes, bytes.Length, ref lpNumberOfBytesRead);

            T result;
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            try
            {
                result = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }

            return result;
        }

        public static string ReadString(Process process, ulong address, uint length)
        {
            byte[] bytes = new byte[length];

            int lpNumberOfBytesRead = 0;
            WindowsApi.ReadProcessMemory(process.Handle, (IntPtr)address, bytes, bytes.Length, ref lpNumberOfBytesRead);

            int nullTerminatorIndex = Array.FindIndex(bytes, (byte b) => b == 0);
            if (nullTerminatorIndex >= 0)
            {
                Array.Resize(ref bytes, nullTerminatorIndex);
                return System.Text.Encoding.UTF8.GetString(bytes);
            }

            return null;
        }

        static List<PointerTrace> s_UniquePointerTraces = new List<PointerTrace>();
        public static ulong ReadMultiLevelPointer(bool traceUniquePointers, Process process, ulong address, params long[] offsets)
        {
            PointerTrace trace = new PointerTrace();

            ulong result = address;
            foreach (var offset in offsets)
            {
                var readResult = Read<ulong>(process, address);
                result = (ulong)((long)readResult + offset);

                trace.Levels.Add(new PointerTraceLevel(address, readResult, offset, result));

                address = result;
            }

            if (traceUniquePointers && !s_UniquePointerTraces.Contains(trace))
            {
                s_UniquePointerTraces.Add(trace);
                Logger.Log(LogLevel.Info, $"Unique Pointer Trace:\r\n{trace}");
            }

            return result;
        }

        public static ulong LoadEffectiveAddressRelative(Process process, ulong address)
        {
            const uint opcodeLength = 3;
            const uint paramLength = 4;
            const uint instructionLength = opcodeLength + paramLength;

            uint operand = Read<uint>(process, address + opcodeLength);
            ulong operand64 = operand;

            // 64 bit relative addressing 
            if (operand64 > Int32.MaxValue)
            {
                operand64 = 0xffffffff00000000 | operand64;
            }

            return address + operand64 + instructionLength;
        }
    }
}