using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Speech.Synthesis;
using System.Net.NetworkInformation;
using System.IO;

namespace SystemMonitor
{
    class Program
    {
        private static SpeechSynthesizer synth = new SpeechSynthesizer();

        // 
        //  WHERE ALL THE MAGIC HAPPENS!
        //  
        static void Main(string[] args)
        {
            // List of messages that will be selected at random when the CPU is hammered!
            List<string> cpuMaxedOutMessages = new List<string>();
            cpuMaxedOutMessages.Add("WARNING: CPU running high, repeat, CPU running high!");
            cpuMaxedOutMessages.Add("WARNING: CPU running high, reduce use!");
            cpuMaxedOutMessages.Add("WARNING: Your CPU is running at over 80% capacity, consider reducing use!");
            

            // The dice! LIKE DND
            Random rand = new Random();

            // This will greet the user in the default voice
            synth.Speak("Welcome to the interactive System Monitor!");

            #region Initialise Performance Counters 
            // This will pull the current CPU load in percentage
            PerformanceCounter perfCpuCount = new PerformanceCounter("Processor Information", "% Processor Time", "_Total");
            System.Threading.Thread.Sleep(1000);//attempting to stop constant 0% reading
            perfCpuCount.NextValue();

            // This will pull the current available memory in Megabytes
            PerformanceCounter perfMemCount = new PerformanceCounter("Memory", "Available MBytes");
            perfMemCount.NextValue();
                       
            // This will get us the system uptime (in seconds)
            PerformanceCounter perfUptimeCount = new PerformanceCounter("System", "System Up Time");
            perfUptimeCount.NextValue();

            /*PerformanceCounter diskcounter = new PerformanceCounter("LogicalDisk", "% Free Space", "_Total");
            diskcounter.NextValue();
            //not implemented
            PerformanceCounter bandwidthcounter = new PerformanceCounter("Network Interface", "Current Bandwidth", "Marvell AVASTAR Wireless-AC Network Controller");
            bandwidthcounter.NextValue();*/
            #endregion


            TimeSpan uptimeSpan = TimeSpan.FromSeconds(perfUptimeCount.NextValue());
            string systemUptimeMessage = string.Format("The current system up time is {0} days {1} hours {2} minutes {3} seconds",
                (int)uptimeSpan.TotalDays,
                (int)uptimeSpan.Hours,
                (int)uptimeSpan.Minutes,
                (int)uptimeSpan.Seconds
                );

            // Tell the user what the current system uptime is
            JerrySpeak(systemUptimeMessage, VoiceGender.Male, 2);
            //tangent
            
            //end tangent


            int speechSpeed = 1;
            bool isChromeOpenedAlready = false;

            // Infinite While Loop
            while(true)
            {
                // Get the current performance counter values
                int currentCpuPercentage = (int)perfCpuCount.NextValue();
                int currentAvailableMemory = (int)perfMemCount.NextValue();
                double speed = downloadSpeed();
                //int diskcount = (int)diskcounter.NextValue();
                //double netspeed = bandwidthcounter.NextValue(); 


                // Every 1 second print the CPU load in percentage to the screen
                Console.Clear();
                Console.WriteLine("--------------------------------------------------------------------------");
                Console.Write("|  CPU Load: {0}%   | ", currentCpuPercentage);
                Console.Write(" Available Memory: {0}MB | ", currentAvailableMemory);
                Console.WriteLine("Download Speed: {0}kb/s |", speed);
                Console.WriteLine("--------------------------------------------------------------------------");
                
                //displays wrong
                /*DriveInfo[] drives = DriveInfo.GetDrives(); 
                foreach (DriveInfo drive in drives)
                {
                    //There are more attributes you can use.
                    //Check the MSDN link for a complete example.
                    Console.WriteLine(drive.Name);
                    if (drive.IsReady) Console.WriteLine(drive.TotalSize/1024);
                    
                }*/


                // Only tell us when the CPU is above 80% usage
                #region Logic
                if ( currentCpuPercentage > 80 )
                {
                    if (currentCpuPercentage == 100)
                    {
                        // This is designed to prevent the speech speed from exceeding 5x normal
                        string cpuLoadVocalMessage = cpuMaxedOutMessages[rand.Next(5)];
                        JerrySpeak(cpuLoadVocalMessage, VoiceGender.Male, speechSpeed);
                    }
                    else
                    {
                        string cpuLoadVocalMessage = String.Format("The current CPU load is {0} percent", currentCpuPercentage);
                        JerrySpeak(cpuLoadVocalMessage, VoiceGender.Female, 5);
                    }
                }
                #endregion

                // Only tell us when memory is below one gigabyte
                if (currentAvailableMemory < 512)
                {
                    // Speak to the user with text to speech to tell them what the current values are
                    string memAvailableVocalMessage = String.Format("You currently have {0} megabytes of memory available", currentAvailableMemory);
                    JerrySpeak(memAvailableVocalMessage, VoiceGender.Male, 10);
                }

                Thread.Sleep(1000);
            } // end of loop
        }

        /// <summary>
        /// Speaks with a selected voice
        /// </summary>
        /// <param name="message"></param>
        /// <param name="voiceGender"></param>
        public static void JerrySpeak(string message, VoiceGender voiceGender)
        {
            synth.SelectVoiceByHints(voiceGender);
            synth.Speak(message);
        }
        //tangent
        public static double downloadSpeed()
        {
            // Create Object Of WebClient
            System.Net.WebClient wc = new System.Net.WebClient();

            //DateTime Variable To Store Download Start Time.
            DateTime dt1 = DateTime.Now;

            //Number Of Bytes Downloaded Are Stored In ‘data’
            byte[] data = wc.DownloadData("http://google.com");

            //DateTime Variable To Store Download End Time.
            DateTime dt2 = DateTime.Now;

            //To Calculate Speed in Kb Divide Value Of data by 1024 And Then by End Time Subtract Start Time To Know Download Per Second.
            return Math.Round((data.Length / 1024) / (dt2 - dt1).TotalSeconds, 1);
        }
        //end tangent
        /// <summary>
        /// Speaks with a selected voice at a selected speed
        /// </summary>
        /// <param name="message"></param>
        /// <param name="voiceGender"></param>
        /// <param name="rate"></param>
        public static void JerrySpeak(string message, VoiceGender voiceGender, int rate)
        {
            synth.Rate = rate;
            JerrySpeak(message, voiceGender);
        }

       
        
    }
}
