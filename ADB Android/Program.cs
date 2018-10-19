//using is how we tell the compiler to include certain peices of code
using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.IO;
//SharpAdbClient is an open source package that allows us to interact with the adb executible
//The adb executible then interacts with the android device
using SharpAdbClient;

//not 100% sure what namespace is. some code is auto generated when you make a new project
namespace ADB_Android
{
    //not 100% sure what class is about
    class Program
    {
        //Main is the function that starts your program
        //Static and void are modifiers for the function
        //void means that it returns nothing
        //not sure what static is for but I believe it has something to do with how visible this code is to other functions
        static void Main()
        {
            //Console.WriteLine is how we show information to our users
            Console.WriteLine("Loading...");

            //here we are setting up variables to be used later on
            //this is called variable initialization
            //we initialize variables with default values
            //this will eventually be moved to a settings file
            //int stands for integer (whole number both positive and negative)
            //each variable needs a declared type and in this case it is int
            //there are quite a few variable types so I will not go into detail here but if you have quetions please ask
            int nameNumberVideo = 1;
            int nameNumberImg = 1;
            var adb = SharpAdbClient.AdbClient.Instance;
            int i = 0;
            bool auto = true;
            var pathValues = Environment.GetEnvironmentVariable("PATH");
            string adbPath = "none";

            //a foreach loop is used to go through a "list" (arrays and objects are two examples) of data and process each item in that list
            //in this instance I am looking for a specific value and I am testing each value in the list individualy
            foreach (var path in pathValues.Split(';'))
            {
                var fullPath = Path.Combine(path, "adb.exe");
                //an if statment is used to determine if something is true and runs a piece of code if the statement is true
                if (File.Exists(fullPath))
                {
                    adbPath = fullPath.ToString();
                }
            }
            AdbServer server = new AdbServer();
            if (adbPath == "none")
            {
                if (File.Exists("./Android-tools/adb.exe"))
                {
                    adbPath = Path.GetFullPath("./Android-tools/adb.exe").ToString();
                }
                //else is used after an if statement to tell the computer to run this piece of code if the if statement is not run
                else
                {
                    Console.Clear();
                    Console.WriteLine("Please enter the full path to an adb executable then press Enter:");
                    adbPath = Console.ReadLine();
                    Console.Clear();
                    Console.WriteLine("Loading...");
                }             
            }
            var result = server.StartServer(adbPath, restartServerIfNewer: true);

            //a while statement is used to tell the computer to run a piece of code while something is true
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
                    nameScreen();
                }
                if (key.KeyChar.ToString() == "v" && auto == false || key.KeyChar.ToString() == "V" && auto == false)
                {
                    nameVideo();
                }

                if (key.KeyChar.ToString() == "c" && auto == true || key.KeyChar.ToString() == "C" && auto == true)
                {
                    autoScreen();
                }
                if (key.KeyChar.ToString() == "v" && auto == true || key.KeyChar.ToString() == "V" && auto == true)
                {
                    //this is how you start a function
                    autoVideo();
                }

                if (key.KeyChar.ToString() == "e" || key.KeyChar.ToString() == "E")
                {
                    adb.KillAdb();
                    Environment.Exit(0); 
                }
            }
            //this is a function
            void nameScreen()
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
            void autoScreen()
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
            void nameVideo()
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
            void autoVideo()
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
        }            
    }
}