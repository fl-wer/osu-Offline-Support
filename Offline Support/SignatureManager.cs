using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Offline_Support
{
    class SignatureManager
    {
        // scans process for byte signature
        // signature is just a signature in this type "8B CF 8B D0 E8 ?? ?? ?? ?? 85 C0 74 19 A1"
        // process handle is handle of a process really nothing to say about it
        // foundAddressOffset is being added to the returned address, if it's on minus obv it will substract
        // foundAddressOffset helps with finding and returning pointers easier
        // start address is the address when it starts scanning from, try using it with blocks of 0x1000
        // maxAddress is the last address it will try scanning in, if it goes above below function will return 0
        public IntPtr signatureScan(string signature, IntPtr processHandle, IntPtr foundAddressOffset, IntPtr startAddress,
        IntPtr maxAddress, int protectionType)
        {
            // this is a first memory address of currently used block of 0x1000 addresses
            IntPtr lastAddressBlock = (IntPtr)0;

            // amount of successful bytes found, like 8B or CF, if all of them found
            // meaning it exceeds certain number then we found address of signature
            int foundBytes = 0;

            // this is offset used for for specific indication where signature was found
            // for example we know block is 0x5000 but it could be then between 0x5000 and 0x6000
            // if we know the byte offset we can then add it to block and have eg 0x5202
            int blockReturnOffset = 0;

            // we are splitting signature to bytes in string array
            string[] explodedSignature = signature.Split(' ');

            // array of bytes that will hold 
            byte[] byteSignature = new byte[explodedSignature.Length];

            // mask can have either "?" or "x" type of characters, if it's "?" it means we skip this byte and
            // act like we found it, where for "x" the byte actually has to match, it's used for signatures
            // where some bytes inside might be changing per game session and we want to skip them
            // mask will look like this "xxxxxxxxxx?xxx?"
            string mask = "";

            // memory basic information for region access level information
            WinImported.MEMORY_BASIC_INFORMATION memoryInfo = new WinImported.MEMORY_BASIC_INFORMATION();

            // converting string byte array to just normal byte array above
            for (int i = 0; i < byteSignature.Length; i++)
            {
                if (explodedSignature[i] != "??") byteSignature[i] = byte.Parse(explodedSignature[i], System.Globalization.NumberStyles.HexNumber);
                else byteSignature[i] = 0; // doesn't matter if it's 0 because mask will make it skip this byte anyway
            }

            // creating mask based on provided signatures and question marks instead of bytes
            // for full explanation read above initialization of "mask" variable 
            foreach (string character in explodedSignature)
            {
                if (character != "??") mask += "x";
                else mask += "?";
            }

            // changing starting point for scanning
            lastAddressBlock = startAddress;

            // removing 0x1000 because our block size is 0x1000
            while ((long)lastAddressBlock < (long)maxAddress - 0x3000)
            {
                // chunk of returned bytes from read process memory function
                // it take program a lot longer for program to read memory bytes one by one
                // instead of reading it through array of bytes, it's just for optimization
                byte[] blockReturn = new byte[0x1000];

                try // we use try here because virtualquery might throw exception if something goes wrong
                {
                    // checking memory info of the block that we want to go through
                    // if the block is with incorrect state, protection or region size it will not proceed
                    // to basically save time and speed up the signature scanning process
                    WinImported.VirtualQueryEx(processHandle, lastAddressBlock, out memoryInfo, 0x1000);

                    if (memoryInfo.State == 0x1000 && memoryInfo.Protect == 0x40 && memoryInfo.RegionSize != 0)
                    {
                        // below reads 0x1000 bytes and puts them in an array variable "blockReturn"
                        if (WinImported.ReadProcessMemory(processHandle, lastAddressBlock, blockReturn, 0x1000, out IntPtr nullification))
                        {
                            // restarting offset to 0 with every new block of bytes
                            blockReturnOffset = 0;

                            foreach (byte singleByte in blockReturn)
                            {
                                // "foundBytes" is also being used as offset for signature & mask arrays
                                // because every time it succeds we want to look for next byte in an array
                                // and every time we find one we add 1 to "foundBytes"

                                // adding offset to narrow down when returning to address and not block of 0x1000
                                blockReturnOffset += 1;

                                // we don't skip the byte and act like we found it if mask doesn't say so
                                if (mask[foundBytes] != '?')
                                {
                                    // if byte matches byte in signature (then goes to next one on next iteration)
                                    if (singleByte == byteSignature[foundBytes])
                                        foundBytes += 1;

                                    // we reset to 0 because if found signature breaks it's no longer the signature we're looking for
                                    else foundBytes = 0;

                                    // if number of successfully found bytes in order reaches signature length it means we found the signature
                                    // so we're returning block + block offset + found address offset and minus signature length
                                    if (foundBytes == byteSignature.Length)
                                        return (IntPtr)((long)lastAddressBlock + blockReturnOffset + (long)foundAddressOffset - byteSignature.Length);
                                }
                                // if mask says to skip byte and act like we found them without even searching => we do
                                else foundBytes += 1;
                            }
                        }
                    }
                }
                catch { }

                // setting up variable for reading next block when done with current one
                lastAddressBlock += 0x1000;
            }

            // it went through all of the addresses in provided range startAddress <-> maxAddress
            // and couldn't find anything so returning 0 meaning it failed to find signature
            return (IntPtr)0;
        }
    }
}
