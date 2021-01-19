using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Woolpack
{
    class Config {
        public string write_location = "";
        public string entry = "";
        public string autoreload = "";
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (!File.Exists("woolpack_config.json")) {
                Console.WriteLine("Couldn't get the config file");
                return;
            }

            // Read the config file
            StreamReader sr = new StreamReader("woolpack_config.json");
            Config config;

            // Try to parse the json file
            try {
                config = JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
            } catch {
                Console.WriteLine("There was an error parsing the config file. Are you sure everything's the correct type?");
                return;
            }
            sr.Close();
            
            if (config.write_location == "") {
                Console.WriteLine("write_location is missing from the config");
                return;
            }

            if (config.entry == "") {
                Console.WriteLine("entry is missing from the config");
                return;
            }

            if (!File.Exists(config.entry)) {
                Console.WriteLine("Entry file doesn't exist");
            }

            BundleFile bundler = new BundleFile() { config = config };
            bundler.InitListener(); // Listen to the files
            bundler.update(); // Compile the files once

            if (config.autoreload != "") {
                // Leave the program running so it can listen to files
                Console.WriteLine($"Listening for changes in {config.autoreload}, press any key to exit");
                Console.ReadLine();
            }
        }
    }
}
