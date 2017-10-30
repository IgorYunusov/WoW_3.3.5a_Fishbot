using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Project_5
{
    static class Logic
    {
        static int processHandle;
        static IntPtr mainWindowHandle;

        static IntPtr firstObject;

        static int bytesRead = 0;

        static List<ulong> lastBobberGUIDs;

        static uint virtualKey_Fishing;
        static uint virtualKey_InteractWithMouseover;
        public static void initAndRun(int virtualKey_Fishing, int virtualKey_InteractWithMouseover)
        {

            Logic.virtualKey_Fishing = (uint)virtualKey_Fishing;
            Logic.virtualKey_InteractWithMouseover = (uint)virtualKey_InteractWithMouseover;

            try
            {
                Process wowProcess = Process.GetProcessesByName("WoW")[0];
                //needed for sending messages to window
                mainWindowHandle = wowProcess.MainWindowHandle;
                //needed for memory operations
                processHandle = (int)WindowsNative.OpenProcess(WindowsNative.PROCESS_ALL_ACCESS, false, wowProcess.Id);
            }
            catch (Exception)
            {
                return;
            }

            // read client connection location
            byte[] clientConnBuffer = new byte[IntPtr.Size];
            WindowsNative.ReadProcessMemory(processHandle, Offsets.clientConnectionOffset.ToInt32(), clientConnBuffer, clientConnBuffer.Length, ref bytesRead);
            IntPtr clientConn = new IntPtr(BitConverter.ToInt32(clientConnBuffer, 0));

            // from that address figure out object manager location
            IntPtr objManagerAddress = IntPtr.Add(clientConn, (int)Offsets.objectManagerOffset);
            byte[] objManagerBuffer = new byte[IntPtr.Size];
            WindowsNative.ReadProcessMemory(processHandle, objManagerAddress.ToInt32(), objManagerBuffer, objManagerBuffer.Length, ref bytesRead);
            IntPtr objManager = new IntPtr(BitConverter.ToInt32(objManagerBuffer, 0));

            // from object manager figure out first object location, save it in field
            IntPtr firstObjectAddress = IntPtr.Add(objManager, (int)Offsets.firstObjectOffset);
            byte[] firstObjectBuffer = new byte[IntPtr.Size];
            WindowsNative.ReadProcessMemory(processHandle, firstObjectAddress.ToInt32(), firstObjectBuffer, firstObjectBuffer.Length, ref bytesRead);
            firstObject = new IntPtr(BitConverter.ToInt32(firstObjectBuffer, 0));

            // run main logic loop
            fishingOps();
        }

        private static void fishingOps()
        {
            lastBobberGUIDs = new List<ulong>();

            //type of object found in memory
            int type;
            byte[] typeBuffer = new byte[32];

            //GLOBAL UNIQUE IDENTIFIER- every object has one
            ulong GUID;
            byte[] GUIDBuffer = new byte[64];

            IntPtr currentObject;

            //press fishing keybind for the first time
            WindowsNative.SendMessage(mainWindowHandle, 256, virtualKey_Fishing, 0);

            while (MainPanel.enabled)
            {
                Application.DoEvents();

                currentObject = firstObject;

                while (currentObject.ToInt64() != 0 && (currentObject.ToInt64() & 1) == 0)
                {

                    WindowsNative.ReadProcessMemory(processHandle, (currentObject + 0x14).ToInt32(), typeBuffer, typeBuffer.Length, ref bytesRead);
                    type = BitConverter.ToInt32(typeBuffer, 0);

                    WindowsNative.ReadProcessMemory(processHandle, (currentObject + 0x30).ToInt32(), GUIDBuffer, GUIDBuffer.Length, ref bytesRead);
                    GUID = BitConverter.ToUInt64(GUIDBuffer, 0);

                    //if (type == 5)
                    //{
                    //    Console.WriteLine("type: " + type);
                    //    Console.WriteLine("cGUID: " + GUID);
                    //}


                    if (lastBobberGUIDs.Count == 5)
                    {
                        lastBobberGUIDs.RemoveAt(0);
                        lastBobberGUIDs.TrimExcess();
                    }

                    if ((type == 5) && !lastBobberGUIDs.Contains(GUID))
                    {
                        IntPtr objectNameAddress2 = currentObject + 0x1A4;
                        byte[] objectNameBuffer2 = new byte[IntPtr.Size];
                        WindowsNative.ReadProcessMemory(processHandle, objectNameAddress2.ToInt32(), objectNameBuffer2, objectNameBuffer2.Length, ref bytesRead);
                        IntPtr objectName2 = new IntPtr(BitConverter.ToInt32(objectNameBuffer2, 0));

                        IntPtr objectNameAddress1 = objectName2 + 0x90;
                        byte[] objectNameBuffer1 = new byte[IntPtr.Size];
                        WindowsNative.ReadProcessMemory(processHandle, objectNameAddress1.ToInt32(), objectNameBuffer1, objectNameBuffer1.Length, ref bytesRead);
                        IntPtr objectName1 = new IntPtr(BitConverter.ToInt32(objectNameBuffer1, 0));

                        byte[] objectNameBuffer = new byte[14];
                        WindowsNative.ReadProcessMemory(processHandle, objectName1.ToInt32(), objectNameBuffer, objectNameBuffer.Length, ref bytesRead);

                        if (Encoding.UTF8.GetString(objectNameBuffer).Equals("Fishing Bobber"))
                        {

                            byte[] bobberStateBuffer = new byte[8];
                            WindowsNative.ReadProcessMemory(processHandle, (currentObject + 0xBC).ToInt32(), bobberStateBuffer, bobberStateBuffer.Length, ref bytesRead);
                            short bobberState = BitConverter.ToInt16(bobberStateBuffer, 0);

                            if (bobberState == 1)
                            {
                                WindowsNative.WriteProcessMemory(processHandle, Offsets.mouseoverLoc.ToInt32(), BitConverter.GetBytes(GUID), BitConverter.GetBytes(GUID).Length, ref bytesRead);
                                WindowsNative.SendMessage(mainWindowHandle, 6, 0, 0);
                                Thread.Sleep(150);

                                //press 'interact with mouseover' keybind
                                WindowsNative.SendMessage(mainWindowHandle, 256, virtualKey_InteractWithMouseover, 0);

                                lastBobberGUIDs.Add(GUID);

                                Thread.Sleep(200);

                                //press fishing keybind
                                WindowsNative.SendMessage(mainWindowHandle, 256, virtualKey_Fishing, 0);
                                break;
                            }
                        }
                    }

                    IntPtr nextObjAddress = currentObject + (int)Offsets.nextObjectOffset;
                    byte[] nextObjBuffer = new byte[IntPtr.Size];
                    WindowsNative.ReadProcessMemory((int)processHandle, nextObjAddress.ToInt32(), nextObjBuffer, nextObjBuffer.Length, ref bytesRead);
                    IntPtr nextObj = new IntPtr(BitConverter.ToInt32(nextObjBuffer, 0));

                    if (nextObj == currentObject)
                        break;

                    currentObject = nextObj;
                }
            }
        }
    }
}