//https://dl.google.com/android/repository/platform-tools-latest-windows.zip
//https://developer.android.com/studio/terms

//using is how we tell the compiler to include certain peices of code
using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.ComponentModel;
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
            loading("Settings");


            
            //https://stackoverflow.com/questions/307688/how-to-download-a-file-from-a-url-in-c
            //here we are setting up variables to be used later on
            //this is called variable initialization
            //we initialize variables with default values
            //this will eventually be moved to a settings file
            //int stands for integer (whole number both positive and negative)
            //each variable needs a declared type and in this case it is int
            //there are quite a few variable types so I will not go into detail here but if you have quetions please ask

            Dictionary<string, object> settings = new Dictionary<string, object>();

            settings.Add("nameNumberVideo", 1);
            settings.Add("nameNumberImg", 1);
            settings.Add("adbPath", "none");
            settings.Add("auto", true);


            if (File.Exists("./settings.config"))
            {
                string[] unFiltered = File.ReadAllLines("./settings.config");
                foreach(string sett in unFiltered)
                {
                    var split = sett.Split('=');
                    int conversionInt;
                    bool conversionBool;
                    //string conversionstring;
                    bool success;
                    switch (split[0])
                    {
                        case "nameNumberVideo":
                            success = int.TryParse(split[1], out conversionInt);
                            if (success)
                            {
                                settings["nameNumberVideo"] = conversionInt;
                            }
                            break;
                        case "nameNumberImg":
                            success = int.TryParse(split[1], out conversionInt);
                            if (success)
                            {
                                settings["nameNumberImg"] = conversionInt;
                            }
                            break;
                        case "adbPath":
                            if(File.Exists(split[1].ToString()))
                            {
                                settings["adbPath"] = split[1].ToString();
                            }
                            break;
                        case "auto":
                            success = bool.TryParse(split[1], out conversionBool);
                            if (success)
                            {
                                settings["auto"] = conversionBool;
                            }
                            break;
                        default:
                            break;
                    }
                    saveSettings();
                }
            } else if (!File.Exists("./settings.config"))
            {
                saveSettings();
            }
            else
            {
                saveSettings();
            }

            loading("ADB Instance");
            var adb = SharpAdbClient.AdbClient.Instance;
            int i = 0;
            var pathValues = Environment.GetEnvironmentVariable("PATH");

            //a foreach loop is used to go through a "list" (arrays and objects are two examples) of data and process each item in that list
            //in this instance I am looking for a specific value and I am testing each value in the list individualy
            
            AdbServer server = new AdbServer();
            if ((string)settings["adbPath"] == "none")
            {

                foreach (var path in pathValues.Split(';'))
                {
                    var fullPath = Path.Combine(path, "adb.exe");
                    //an if statment is used to determine if something is true and runs a piece of code if the statement is true
                    if (File.Exists(fullPath))
                    {
                        settings["adbPath"] = fullPath.ToString();
                        saveSettings();
                    }
                }

                if (File.Exists("./platform-tools/adb.exe") && (string)settings["adbPath"] == "none")
                {
                    settings["adbPath"] = Path.GetFullPath("./platform-tools/adb.exe").ToString();
                    saveSettings();
                }
                //else is used after an if statement to tell the computer to run this piece of code if the if statement is not run
                else if((string)settings["adbPath"] == "none")
                {
                    Console.Clear();
                    Console.WriteLine("Download ADB automatically?");
                    Console.WriteLine("Y = Yes");
                    Console.WriteLine("N = No");
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.ToString().ToLower())
                    {
                        case "y":
                            downloadADB();
                            break;
                        case "n":
                            Console.Clear();
                            Console.WriteLine("Please enter the full path to an adb executable then press Enter:");
                            settings["adbPath"] = Console.ReadLine();
                            saveSettings();
                            break;
                        default:
                            redodownloadCheck();
                            break;
                    };

                }             
            }
            var result = server.StartServer((string)settings["adbPath"], restartServerIfNewer: true);

            //a while statement is used to tell the computer to run a piece of code while something is true
            while (i == 0) 
            {
                Console.Clear();
                Console.WriteLine("Press C to capture a screenshot.");
                Console.WriteLine("Press V to capture a video file.");
                if((bool)settings["auto"] == true)
                {
                    Console.WriteLine("Press R to disable auto-naming.");
                } else
                {
                    Console.WriteLine("Press R to enable auto-naming.");
                }
                Console.WriteLine("Press E to exit.");
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.KeyChar.ToString().ToLower() == "r")
                {
                    settings["auto"] = !(bool)settings["auto"];
                }

                if (key.KeyChar.ToString().ToLower() == "c" && (bool)settings["auto"] == false)
                {
                    nameScreen();
                }
                if (key.KeyChar.ToString().ToLower() == "v" && (bool)settings["auto"] == false)
                {
                    nameVideo();
                }

                if (key.KeyChar.ToString().ToLower() == "c" && (bool)settings["auto"] == true)
                {
                    autoScreen();
                }
                if (key.KeyChar.ToString().ToLower() == "v" && (bool)settings["auto"] == true)
                {
                    //this is how you start a function
                    autoVideo();
                }
                if (key.KeyChar.ToString().ToLower() == "d")
                {
                    downloadADB();
                }
                if (key.KeyChar.ToString().ToLower() == "e")
                {
                    adb.KillAdb();
                    Environment.Exit(0); 
                }
            }

            void saveSettings()
            {
                File.WriteAllLines("./settings.config", new string[4]
                {
                    "adbPath=" + settings["adbPath"].ToString(),
                    "auto=" + settings["auto"].ToString(),
                    "nameNumberImg=" + settings["nameNumberImg"].ToString(),
                    "nameNumberVideo=" + settings["nameNumberVideo"].ToString()
                });
            };
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
                using (Stream stream = File.OpenWrite(@"./Image " + (int)settings["nameNumberImg"] + ".png"))
                {
                    service.Pull("/sdcard/capturescript.png", stream, null, CancellationToken.None);
                }

                adb.ExecuteRemoteCommand("rm /sdcard/capturescript.png", device, receiver);
                settings["nameNumberImg"] = (int)settings["nameNumberImg"] + 1;
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
                        using (Stream stream = File.OpenWrite(@"./Video " + (int)settings["nameNumberVideo"] + ".mp4"))
                        {
                            service.Pull("/sdcard/videocapturescript.mp4", stream, null, CancellationToken.None);
                        }
                        adb.ExecuteRemoteCommand("rm /sdcard/videocapturescript.mp4", device, receiver);
                        transfered = true;
                        settings["nameNumberVideo"] = (int)settings["nameNumberVideo"] + 1;
                    }
                }
            }
            void loading(string whatLoad)
            {
                Console.Clear();
                if (whatLoad != null)
                {
                    Console.WriteLine("Loading " + whatLoad + "...");
                }
                else
                {
                    Console.WriteLine("Loading...");
                };
            }
            void downloadADB()
            {
                Console.Clear();
                Console.WriteLine("Start Download");
                Stopwatch printTime = new Stopwatch();
                bool downloadDone = false;
                bool messageDone = false;
                int progress = 0;
                WebClient wc = new WebClient();
                wc.DownloadFileCompleted += Wc_DownloadStringCompleted;
                wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                wc.DownloadFileAsync(new Uri("https://dl.google.com/android/repository/platform-tools-latest-windows.zip"), "./test.zip");
                while (messageDone == false)
                {
                    if (printTime.ElapsedMilliseconds >= 1000)
                    {
                        Console.WriteLine("Download Progress:" + progress);
                        printTime.Restart();
                    }
                    if(downloadDone == true)
                    {
                        messageDone = true;
                        Thread.Sleep(1000);
                    }
                }
                void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
                {
                    if (!downloadDone)
                    {
                        if (!printTime.IsRunning)
                        {
                            Console.WriteLine("Download Progress:" + progress);
                            printTime.Start();
                        }
                        progress = e.ProgressPercentage;
                    }
                    
                }
                void Wc_DownloadStringCompleted (object sender, AsyncCompletedEventArgs e)
                {
                    
                    Console.Clear();
                    Console.WriteLine("Download Complete!");
                    Console.WriteLine("Setting up files for use.");
                    downloadDone = true;
                }
                
            }
            void redodownloadCheck()
            {
                Console.Clear();
                Console.WriteLine("Please press Y or N...");
                Console.WriteLine("Download ADB automatically?");
                Console.WriteLine("Y = Yes");
                Console.WriteLine("N = No");
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.ToString().ToLower())
                {
                    case "y":
                        downloadADB();
                        break;
                    case "n":
                        Console.Clear();
                        Console.WriteLine("Please enter the full path to an adb executable then press Enter:");
                        settings["adbPath"] = Console.ReadLine();
                        saveSettings();
                        break;
                    default:
                        redodownloadCheck();
                        break;
                };
            }
        }
    }
}