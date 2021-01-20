using Woolpack;
using System.IO;
using System;
using System.Collections.Generic;
class BundleFile {
  public Config config;
  ParseFile ParseFile = new ParseFile();
  FileSystemWatcher watcher;
  public void InitListener() {
    if (config.autoreload != "") {
      try {
        // Watch for when a file changes if autoreload is set
        watcher = new FileSystemWatcher();
        watcher.Path = config.autoreload;

        watcher.NotifyFilter = NotifyFilters.LastWrite;

        watcher.Changed += (a, b) => {update();};

        watcher.EnableRaisingEvents = true;
      } catch {
        Console.WriteLine("Error watching autoreload folder, are you sure the folder given exists?");
      }
    }
  }

  public void update() {
    Console.WriteLine("Bundling scarpet scripts");

    ParseFile.imported = new List<string>() {config.entry};

    string toWrite;
    // Wait for the file to finish being written to
    while (!IsFileReady(config.entry)) ;
    using (StreamReader sr = new StreamReader(config.entry)) {
      toWrite = ParseFile.ParseImports(sr.ReadToEnd()); // Parse the file
    }

    // Wait for the file to be available for writing
    while (!IsFileReady(config.write_location) && File.Exists(config.write_location)) ;
    using (StreamWriter sw = new StreamWriter(config.write_location)) {
      Console.WriteLine("Writing");
      sw.Write(toWrite); // Write the file
    }
  }

  // https://stackoverflow.com/questions/1406808/wait-for-file-to-be-freed-by-process
  public static bool IsFileReady(string filename) {
    // If the file can be opened for exclusive access it means that the file
    // is no longer locked by another process.
    try
    {
        using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
            return inputStream.Length > 0;
    }
    catch (Exception)
    {
        return false;
    }
  }
}