using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vxlapi_NET;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;

namespace WpfApp1
{
    public class canDevice
    {
        public delegate void RxEventHandler(uint id, UInt64 timestamp, byte[] data);
        public event RxEventHandler RxEvent; // event
        // -----------------------------------------------------------------------------------------------
        // Global variables
        // -----------------------------------------------------------------------------------------------
        // Driver access through XLDriver (wrapper)
        private static XLDriver CanDriver = new XLDriver();
        private static String appName = "FRTUNE";

        // Driver configuration
        private static XLClass.xl_driver_config driverConfig = new XLClass.xl_driver_config();

        // Variables required by XLDriver
        private static XLDefine.XL_HardwareType hwType = XLDefine.XL_HardwareType.XL_HWTYPE_NONE;
        private static uint hwIndex = 0;
        private static uint hwChannel = 0;
        private static int portHandle = -1;
        private static UInt64 accessMask = 0;
        private static UInt64 permissionMask = 0;
        private static UInt64 txMask = 0;
        private static UInt64 rxMask = 0;
        private static int txCi = -1;
        private static int rxCi = -1;
        private static EventWaitHandle xlEvWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, null);

        // RX thread
        public Thread rxThread;
        public bool blockRxThread = false;
        // -----------------------------------------------------------------------------------------------

        private String statusString = "";

        public String getStatusString()
        {
            return statusString;
        }

        public void handleReceivedMsg(XLClass.xl_event receivedEvent)
        {
            statusString = CanDriver.XL_GetEventString(receivedEvent);
            RxEvent?.Invoke(receivedEvent.tagData.can_Msg.id, receivedEvent.timeStamp, receivedEvent.tagData.can_Msg.data);
        }

        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// MAIN
        /// 
        /// Sends and receives CAN messages using main methods of the "XLDriver" class.
        /// This demo requires two connected CAN channels (Vector network interface). 
        /// The configuration is read from Vector Hardware Config (vcanconf.exe).
        /// </summary>
        // -----------------------------------------------------------------------------------------------
        [STAThread]
        public int initCanDriver()
        {
            XLDefine.XL_Status status;

            Trace.WriteLine("-------------------------------------------------------------------");
            Trace.WriteLine("                     xlCanDriver.NET C# V20.30                       ");
            Trace.WriteLine("Copyright (c) 2020 by Vector Informatik GmbH.  All rights reserved.");
            Trace.WriteLine("-------------------------------------------------------------------\n");

            // print .NET wrapper version
            Trace.WriteLine("vxlapi_NET        : " + typeof(XLDriver).Assembly.GetName().Version);

            // Open XL Driver
            status = CanDriver.XL_OpenDriver();
            Trace.WriteLine("Open Driver       : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();


            // Get XL Driver configuration
            status = CanDriver.XL_GetDriverConfig(ref driverConfig);
            Trace.WriteLine("Get Driver Config : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();


            // Convert the dll version number into a readable string
            Trace.WriteLine("DLL Version       : " + CanDriver.VersionToString(driverConfig.dllVersion));


            // Display channel count
            Trace.WriteLine("Channels found    : " + driverConfig.channelCount);


            // Display all found channels
            for (int i = 0; i < driverConfig.channelCount; i++)
            {
                Trace.WriteLine("\n                   [{0}] " + driverConfig.channel[i].name, i.ToString());
                Trace.WriteLine("                    - Channel Mask    : " + driverConfig.channel[i].channelMask);
                Trace.WriteLine("                    - Transceiver Name: " + driverConfig.channel[i].transceiverName);
                Trace.WriteLine("                    - Serial Number   : " + driverConfig.channel[i].serialNumber);
            }

            // If the application name cannot be found in VCANCONF...
            if ((CanDriver.XL_GetApplConfig(appName, 0, ref hwType, ref hwIndex, ref hwChannel, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN) != XLDefine.XL_Status.XL_SUCCESS) ||
                (CanDriver.XL_GetApplConfig(appName, 1, ref hwType, ref hwIndex, ref hwChannel, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN) != XLDefine.XL_Status.XL_SUCCESS))
            {
                //...create the item with two CAN channels
                CanDriver.XL_SetApplConfig(appName, 0, XLDefine.XL_HardwareType.XL_HWTYPE_NONE, 0, 0, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);
                CanDriver.XL_SetApplConfig(appName, 1, XLDefine.XL_HardwareType.XL_HWTYPE_NONE, 0, 0, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);
                //PrintAssignErrorAndPopupHwConf();
            }

            // Request the user to assign channels until both CAN1 (Tx) and CAN2 (Rx) are assigned to usable channels
            while (!GetAppChannelAndTestIsOk(0, ref txMask, ref txCi) || !GetAppChannelAndTestIsOk(1, ref rxMask, ref rxCi))
            {
                //PrintAssignErrorAndPopupHwConf();
            }

            PrintConfig();

            accessMask = txMask | rxMask;
            permissionMask = accessMask;

            // Open port
            status = CanDriver.XL_OpenPort(ref portHandle, appName, accessMask, ref permissionMask, 1024, XLDefine.XL_InterfaceVersion.XL_INTERFACE_VERSION, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);
            Trace.WriteLine("\n\nOpen Port             : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();

            // Check port
            status = CanDriver.XL_CanRequestChipState(portHandle, accessMask);
            Trace.WriteLine("Can Request Chip State: " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();

            // Activate channel
            status = CanDriver.XL_ActivateChannel(portHandle, accessMask, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN, XLDefine.XL_AC_Flags.XL_ACTIVATE_NONE);
            Trace.WriteLine("Activate Channel      : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();

            // Initialize EventWaitHandle object with RX event handle provided by DLL
            int tempInt = -1;
            status = CanDriver.XL_SetNotification(portHandle, ref tempInt, 1);
            xlEvWaitHandle.SafeWaitHandle = new SafeWaitHandle(new IntPtr(tempInt), true);

            Trace.WriteLine("Set Notification      : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();

            // Reset time stamp clock
            status = CanDriver.XL_ResetClock(portHandle);
            Trace.WriteLine("Reset Clock           : " + status + "\n\n");
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();

            // Run Rx Thread
            Trace.WriteLine("Start Rx thread...");
            rxThread = new Thread(new ThreadStart(RXThread));
            rxThread.Start();

            // User information
            Trace.WriteLine("Press <ENTER> to transmit CAN messages \n  <b>, <ENTER> to block Rx thread for rx-overrun-test \n  <B>, <ENTER> burst of CAN TX messages \n  <x>, <ENTER> to exit");

            // Transmit CAN data



            // Kill Rx thread
            //rxThread.Abort();
            //Trace.WriteLine("Close Port                     : " + CanDriver.XL_ClosePort(portHandle));
            // Trace.WriteLine("Close Driver                   : " + CanDriver.XL_CloseDriver());

            return 0;
        }
        // -----------------------------------------------------------------------------------------------




        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// Error message/exit in case of a functional call does not return XL_SUCCESS
        /// </summary>
        // -----------------------------------------------------------------------------------------------
        private static int PrintFunctionError()
        {
            Trace.WriteLine("\nERROR: Function call failed!\nPress any key to continue...");
            //Trace.ReadKey();
            return -1;
        }
        // -----------------------------------------------------------------------------------------------




        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// Displays the Vector Hardware Configuration.
        /// </summary>
        // -----------------------------------------------------------------------------------------------
        private static void PrintConfig()
        {
            Trace.WriteLine("\n\nAPPLICATION CONFIGURATION");

            foreach (int channelIndex in new int[] { txCi, rxCi })
            {
                Trace.WriteLine("-------------------------------------------------------------------");
                Trace.WriteLine("Configured Hardware Channel : " + driverConfig.channel[channelIndex].name);
                Trace.WriteLine("Hardware Driver Version     : " + CanDriver.VersionToString(driverConfig.channel[channelIndex].driverVersion));
                Trace.WriteLine("Used Transceiver            : " + driverConfig.channel[channelIndex].transceiverName);
            }

            Trace.WriteLine("-------------------------------------------------------------------\n");
        }
        // -----------------------------------------------------------------------------------------------




        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// Error message if channel assignment is not valid and popup VHwConfig, so the user can correct the assignment
        /// </summary>
        // -----------------------------------------------------------------------------------------------
        private static void PrintAssignErrorAndPopupHwConf()
        {
            Trace.WriteLine("\nPlease check application settings of \"" + appName + " CAN1/CAN2\",\nassign them to available hardware channels and press enter.");
            CanDriver.XL_PopupHwConfig();
            //Trace.ReadKey();
        }
        // -----------------------------------------------------------------------------------------------

        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieve the application channel assignment and test if this channel can be opened
        /// </summary>
        // -----------------------------------------------------------------------------------------------
        private static bool GetAppChannelAndTestIsOk(uint appChIdx, ref UInt64 chMask, ref int chIdx)
        {
            XLDefine.XL_Status status = CanDriver.XL_GetApplConfig(appName, appChIdx, ref hwType, ref hwIndex, ref hwChannel, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);
            if (status != XLDefine.XL_Status.XL_SUCCESS)
            {
                Trace.WriteLine("XL_GetApplConfig      : " + status);
                PrintFunctionError();
            }

            chMask = CanDriver.XL_GetChannelMask(hwType, (int)hwIndex, (int)hwChannel);
            chIdx = CanDriver.XL_GetChannelIndex(hwType, (int)hwIndex, (int)hwChannel);
            if (chIdx < 0 || chIdx >= driverConfig.channelCount)
            {
                // the (hwType, hwIndex, hwChannel) triplet stored in the application configuration does not refer to any available channel.
                return false;
            }

            // test if CAN is available on this channel
            return (driverConfig.channel[chIdx].channelBusCapabilities & XLDefine.XL_BusCapabilities.XL_BUS_ACTIVE_CAP_CAN) != 0;
        }
        // -----------------------------------------------------------------------------------------------




        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// Sends some CAN messages.
        /// </summary>
        // ----------------------------------------------------------------------------------------------- 
       
        public void TransmitMessage(uint id, byte[] data, int dlc)
        {
            XLDefine.XL_Status txStatus;
            XLClass.xl_event_collection xlEventCollection = new XLClass.xl_event_collection(1);

            xlEventCollection.xlEvent[0].tagData.can_Msg.id = id;
            xlEventCollection.xlEvent[0].tagData.can_Msg.dlc = 8;

            for(int i = 0; i < dlc; i++)
            {
                xlEventCollection.xlEvent[0].tagData.can_Msg.data[i] = data[i];
                Trace.WriteLine(data[i]);
            }
            xlEventCollection.xlEvent[0].tag = XLDefine.XL_EventTags.XL_TRANSMIT_MSG;

            txStatus = CanDriver.XL_CanTransmit(portHandle, txMask, xlEventCollection);
            Trace.WriteLine("Transmit Message      : " + txStatus);
        }


        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// EVENT THREAD (RX)
        /// 
        /// RX thread waits for Vector interface events and displays filtered CAN messages.
        /// </summary>
        // ----------------------------------------------------------------------------------------------- 
        public void RXThread()
        {
            // Create new object containing received data 
            XLClass.xl_event receivedEvent = new XLClass.xl_event();

            // Result of XL Driver function calls
            XLDefine.XL_Status xlStatus = XLDefine.XL_Status.XL_SUCCESS;


            // Note: this thread will be destroyed by MAIN
            while (true)
            {
                // Wait for hardware events
                if (xlEvWaitHandle.WaitOne(1000))
                {
                    // ...init xlStatus first
                    xlStatus = XLDefine.XL_Status.XL_SUCCESS;

                    // afterwards: while hw queue is not empty...
                    while (xlStatus != XLDefine.XL_Status.XL_ERR_QUEUE_IS_EMPTY)
                    {
                        // ...block RX thread to generate RX-Queue overflows
                        while (blockRxThread) { Thread.Sleep(1000); }

                        // ...receive data from hardware.
                        xlStatus = CanDriver.XL_Receive(portHandle, ref receivedEvent);

                        //  If receiving succeed....
                        if (xlStatus == XLDefine.XL_Status.XL_SUCCESS)
                        {
                            if ((receivedEvent.flags & XLDefine.XL_MessageFlags.XL_EVENT_FLAG_OVERRUN) != 0)
                            {
                                Trace.WriteLine("-- XL_EVENT_FLAG_OVERRUN --");
                            }

                            // ...and data is a Rx msg...
                            if (receivedEvent.tag == XLDefine.XL_EventTags.XL_RECEIVE_MSG)
                            {
                                if ((receivedEvent.tagData.can_Msg.flags & XLDefine.XL_MessageFlags.XL_CAN_MSG_FLAG_OVERRUN) != 0)
                                {
                                    Trace.WriteLine("-- XL_CAN_MSG_FLAG_OVERRUN --");
                                }

                                // ...check various flags
                                if ((receivedEvent.tagData.can_Msg.flags & XLDefine.XL_MessageFlags.XL_CAN_MSG_FLAG_ERROR_FRAME)
                                    == XLDefine.XL_MessageFlags.XL_CAN_MSG_FLAG_ERROR_FRAME)
                                {
                                    Trace.WriteLine("ERROR FRAME");
                                }

                                else if ((receivedEvent.tagData.can_Msg.flags & XLDefine.XL_MessageFlags.XL_CAN_MSG_FLAG_REMOTE_FRAME)
                                    == XLDefine.XL_MessageFlags.XL_CAN_MSG_FLAG_REMOTE_FRAME)
                                {
                                    Trace.WriteLine("REMOTE FRAME");
                                }

                                else
                                {
                                    Trace.WriteLine(CanDriver.XL_GetEventString(receivedEvent));
                                    handleReceivedMsg(receivedEvent);
                                }
                            }
                        }
                    }
                }
                // No event occurred
            }
        }
    }


}

