using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using SharpAdbClient;
using System.Threading;
using System.Net;
using System.IO;

namespace ADB_Android
{
    class Program
    {
        static void Main()
        {
            int nameNumberVideo = 1;
            int nameNumberImg = 1;
            var adb = SharpAdbClient.AdbClient.Instance;
            int i = 0;
            bool auto = true;
            AdbServer server = new AdbServer();
            var result = server.StartServer(@"./Android-tools/adb.exe", restartServerIfNewer: true);
            while (i == 0) 
            {
                Console.Clear();
                Console.WriteLine("Press C to capture a screenshot.");
                Console.WriteLine("Press V to capture a video file.");
                if(auto == true)
                {
                    Console.WriteLine("Press R to disable auto-naming.");
                } else
                {
                    Console.WriteLine("Press R to enable auto-naming.");
                }
                Console.WriteLine("Press E to exit.");
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.KeyChar.ToString() == "r" || key.KeyChar.ToString() == "R")
                {
                    auto = !auto;
                }

                if (key.KeyChar.ToString() == "c" && auto == false || key.KeyChar.ToString() == "C" && auto == false )
                {
                    var device = adb.GetDevices().First();
                    var receiver = new ConsoleOutputReceiver();
                    adb.ExecuteRemoteCommand("screencap /sdcard/capturescript.png", device, receiver);
                    Console.WriteLine(receiver.ToString());
                    Console.WriteLine("Please enter the name of the PNG image file:");
                    string renameImg = Console.ReadLine().ToString();

                    using (SyncService service = new SyncService(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)), device))
                    using (Stream stream = File.OpenWrite(@"./" + renameImg + ".png"))
                    {
                        service.Pull("/sdcard/capturescript.png", stream, null, CancellationToken.None);
                    }
                    adb.ExecuteRemoteCommand("rm /sdcard/capturescript.png", device, receiver);
                }
                if (key.KeyChar.ToString() == "v" && auto == false || key.KeyChar.ToString() == "V" && auto == false)
                {
                    var device = adb.GetDevices().First();
                    var receiver = new ConsoleOutputReceiver();
                    CancellationTokenSource cancelSource = new CancellationTokenSource();
                    CancellationToken cancel = cancelSource.Token;
                    adb.ExecuteRemoteCommandAsync("screenrecord /sdcard/videocapturescript.mp4", device, receiver, cancel, 0);
                    Console.WriteLine(receiver.ToString());
                    Console.WriteLine("Press T to end recording and transfer the file to the pc.");

                    ConsoleKeyInfo transfer = Console.ReadKey(true);
                    bool transfered = false;
                    while (transfered != true)
                    {
                        
                        if (transfer.KeyChar.ToString() == "t" || transfer.KeyChar.ToString() == "T")
                        {
                            cancelSource.Cancel();
                            Console.WriteLine("Please enter the name of the mp4 video file:");
                            string renameVideo = Console.ReadLine().ToString();
                            using (SyncService service = new SyncService(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)), device))
                            using (Stream stream = File.OpenWrite(@"./" + renameVideo + ".mp4"))
                            {
                                service.Pull("/sdcard/videocapturescript.mp4", stream, null, CancellationToken.None);
                            }
                            adb.ExecuteRemoteCommand("rm /sdcard/videocapturescript.mp4", device, receiver);
                            transfered = true;
                        }
                    }

                }

                if (key.KeyChar.ToString() == "c" && auto == true || key.KeyChar.ToString() == "C" && auto == true)
                {
                    var device = adb.GetDevices().First();
                    var receiver = new ConsoleOutputReceiver();

                    adb.ExecuteRemoteCommand("screencap /sdcard/capturescript.png", device, receiver);
                    Console.WriteLine(receiver.ToString());
                    System.Threading.Thread.Sleep(1000);
                    using (SyncService service = new SyncService(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)), device))
                    using (Stream stream = File.OpenWrite(@"./Image " + nameNumberImg + ".png"))
                    {
                        service.Pull("/sdcard/capturescript.png", stream, null, CancellationToken.None);
                    }

                    adb.ExecuteRemoteCommand("rm /sdcard/capturescript.png", device, receiver);
                    nameNumberImg++;
                }
                if (key.KeyChar.ToString() == "v" && auto == true || key.KeyChar.ToString() == "V" && auto == true)
                {
                    var device = adb.GetDevices().First();
                    var receiver = new ConsoleOutputReceiver();
                    CancellationTokenSource cancelSource = new CancellationTokenSource();
                    CancellationToken cancel = cancelSource.Token;
                    adb.ExecuteRemoteCommandAsync("screenrecord /sdcard/videocapturescript.mp4", device, receiver, cancel, 0);
                    Console.WriteLine(receiver.ToString());
                    Console.WriteLine("Press T to end recording and transfer the file to the pc.");

                    ConsoleKeyInfo transfer = Console.ReadKey(true);
                    bool transfered = false;
                    while (transfered != true)
                    {

                        if (transfer.KeyChar.ToString() == "t" || transfer.KeyChar.ToString() == "T")
                        {
                            cancelSource.Cancel();
                            System.Threading.Thread.Sleep(1000);
                            using (SyncService service = new SyncService(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)), device))
                            using (Stream stream = File.OpenWrite(@"./Video " + nameNumberVideo + ".mp4"))
                            {
                                service.Pull("/sdcard/videocapturescript.mp4", stream, null, CancellationToken.None);
                            }
                            adb.ExecuteRemoteCommand("rm /sdcard/videocapturescript.mp4", device, receiver);
                            transfered = true;
                            nameNumberVideo++;
                        }
                    }

                }

                if (key.KeyChar.ToString() == "e" || key.KeyChar.ToString() == "E")
                {
                    Environment.Exit(0);
                    
                }
            }
        }
            
    }
}
