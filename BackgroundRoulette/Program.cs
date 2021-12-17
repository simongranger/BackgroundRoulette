using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace BackgroundRoulette
{
    internal class Program
    {
        private const string DefaultBackgroundLibraryPath = "\\\\eng\\eng\\Cloud\\BackgroundRoulette\\Backgrounds";
        private const string TeamsPath = "\\Microsoft\\Teams\\Backgrounds\\Uploads";
        private const string BackgroundFileName = "BackgroundRoulette.jpg";
        private const string ThumbnailName = "BackgroundRoulette_thumb.jpg";
        private const string TrackerFileName = "Tracker.txt";

        private static void Main(string[] args)
        {
            var backgroundLibraryPath = DefaultBackgroundLibraryPath;
            if (args.Length > 0) backgroundLibraryPath = args[0];

            var background = GetRandomBackground(backgroundLibraryPath);
            if (background != null)
            {
                CopyFileToTeams(background, backgroundLibraryPath);
            }

            // Sleep to let user see the selected background.
            Thread.Sleep(3000);
        }

        private static string GetRandomBackground(string backgroundLibraryPath)
        {
            try
            {
                if (!Directory.Exists(backgroundLibraryPath))
                {
                    Console.WriteLine("********");
                    Console.WriteLine("Error: Cannot access target folder \'{0}\'", backgroundLibraryPath);
                    Console.WriteLine("********");
                    Thread.Sleep(3000);
                    return null;
                }

                // Only get jpg files.
                var backgrounds = Directory.GetFiles(backgroundLibraryPath, "*.jpg");

                if (backgrounds.Length == 0)
                {
                    Console.WriteLine("********");
                    Console.WriteLine("Error: Could not find any jpg file in \'{0}\'", backgroundLibraryPath);
                    Console.WriteLine("********");
                    Thread.Sleep(3000);
                    return null;
                }

                // Display available backgrounds.
                Console.WriteLine("Available backgrounds:");
                for (var i = 0; i < backgrounds.Length; i++)
                    Console.WriteLine("{0}", backgrounds[i].Replace(backgroundLibraryPath + "\\", ""));
                Thread.Sleep(500);

                // Create anticipation...
                Console.Write("Selecting ...");
                Thread.Sleep(1000);

                // Select random background.
                // We keep track of the last few distributed backgrounds and will not distribute them.
                var backgroundTracker = new List<string>();
                var trackerLocation = Path.Combine(backgroundLibraryPath, TrackerFileName);

                // Read tracker file if it exists and crop it if required.
                if (File.Exists(trackerLocation))
                {
                    backgroundTracker = File.ReadAllLines(trackerLocation).ToList();
                    while (backgroundTracker.Count >= backgrounds.Length - 2) backgroundTracker.RemoveAt(0);
                }

                var rand = new Random();
                var success = false;
                int index;
                string backgroundName;

                // Get random background until we find one not in the list.
                do
                {
                    index = rand.Next(backgrounds.Length);
                    backgroundName = backgrounds[index].Replace(backgroundLibraryPath + "\\", "");
                    if (!backgroundTracker.Contains(backgroundName))
                    {
                        backgroundTracker.Add(backgroundName);
                        success = true;
                    }

                    Console.Write("...");
                    Thread.Sleep(100);
                } while (!success);

                // Update tracker file.
                File.WriteAllLines(trackerLocation, backgroundTracker);

                Console.WriteLine("");
                Console.WriteLine("The selected background was {0}!", backgroundName);

                return backgrounds[index];
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e);
            }

            return null;
        }

        private static void CopyFileToTeams(string background, string backgroundLibraryPath)
        {
            var appDataPath = Environment.ExpandEnvironmentVariables("%APPDATA%");
            var destFileName = Path.Combine(appDataPath + TeamsPath, BackgroundFileName);
            var sourceThumbnail = "images\\" + ThumbnailName;
            var destThumbnail = Path.Combine(appDataPath + TeamsPath, ThumbnailName);

            // Copy background file.
            File.Copy(background, destFileName, true);

            // Copy thumbnail if required.
            if (!File.Exists(destThumbnail)) File.Copy(sourceThumbnail, destThumbnail);
        }
    }
}
