﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Principal;
using Tx.Windows;
using Tx.Windows.Microsoft_Windows_Kernel_Network;

namespace TxSamples
{
    class Program
    {
        static void Main()
        {
            StartSession();
            Playback playback = new Playback();
            playback.AddEtlFiles("tcp.etl");
            playback.AddRealTimeSession("tcp");

            var recv = from req in playback.GetObservable<KNetEvt_RecvIPV4>()
                        select new
                        {
                            Time = req.OccurenceTime,
                            Size = req.size,
                            Address = new IPAddress(req.daddr)
                        };


            using (recv.Subscribe(e => Console.WriteLine("{0} : Received {1,5} bytes from {2}", e.Time, e.Size, e.Address)))
            {
                playback.Start();

                Console.ReadLine();
            }
        }

        static void StartSession()
        {
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                throw new Exception("To use ETW real-time session you must be administrator");

            string sessionName = "tcp";
            Guid providerId = new Guid("{7dd42a49-5329-4832-8dfd-43d979153a88}"); 
            
            Process logman = Process.Start("logman.exe", "stop " + sessionName + " -ets");
            logman.WaitForExit();

            logman = Process.Start("logman.exe", "create trace " + sessionName + " -rt -nb 2 2 -bs 1024 -p {" + providerId + "} 0xffffffffffffffff -ets");
            logman.WaitForExit();
        }
    }
}
