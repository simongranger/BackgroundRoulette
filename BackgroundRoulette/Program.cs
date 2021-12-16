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
        private const string Tracker = "Tracker.txt";

        private static void Main(string[] args)
        {
            var backgroundLibraryPath = DefaultBackgroundLibraryPath;
            if (args.Length > 0) backgroundLibraryPath = args[0];

            var background = GetRandomBackground(backgroundLibraryPath);
            CopyFileToTeams(background, backgroundLibraryPath);

            // Sleep to let user see the selected background.
            Thread.Sleep(3000);
        }

        private static string GetRandomBackground(string backgroundLibraryPath)
        {
            try
            {
                // Only get jpg files.
                var backgrounds = Directory.GetFiles(backgroundLibraryPath, "*.jpg");

                // Display available backgrounds.
                Console.WriteLine("Available backgrounds:");
                for (var i = 0; i < backgrounds.Length; i++)
                    Console.WriteLine("{0}", backgrounds[i].Replace(backgroundLibraryPath + "\\", ""));
                Thread.Sleep(1000);

                // Create anticipation...
                Console.Write("Selecting ...");
                Thread.Sleep(1000);

                // Select random background.
                // We keep track of the last few distributed backgrounds and will not distribute them.
                List<string> backgroundTracker = new List<string>();

                // Read tracker file if it exists and crop it if required.
                if (File.Exists(Tracker))
                {
                   backgroundTracker = File.ReadAllLines(Tracker).ToList();
                   while (backgroundTracker.Count >= backgrounds.Length - 1)
                   {
                       backgroundTracker.RemoveAt(0);
                   }
                }

                var rand = new Random();
                bool success = false;
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
                } while (!success);

                // Update tracker file.
                File.WriteAllLines(Tracker, backgroundTracker);

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
            if (background == null)
            {
                Console.WriteLine("Could not find a jpg background file");
                return;
            }

            var appDataPath = Environment.ExpandEnvironmentVariables("%APPDATA%");
            var destFileName = Path.Combine(appDataPath + TeamsPath, BackgroundFileName);
            var sourceThumbnail = Path.Combine(backgroundLibraryPath + "\\Thumbnail\\", ThumbnailName);
            var destThumbnail = Path.Combine(appDataPath + TeamsPath, BackgroundFileName);

            // Copy background file.
            File.Copy(background, destFileName, true);

            // Copy thumbnail if required.
            if (!File.Exists(destThumbnail)) File.Copy(sourceThumbnail, destThumbnail);
        }
    }
}
