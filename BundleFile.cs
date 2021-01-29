using Woolpack;
using System.IO;
using System;
using System.Linq;
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

    ParseFile.config = config;

    ParseFile.imported = new List<string>() {config.entry};

    Script toWrite = new Script();
    // Wait for the file to finish being written to
    while (!IsFileReady(config.entry)) ;
    using (StreamReader sr = new StreamReader(config.entry)) {
      string text = sr.ReadToEnd();
      toWrite.text = text;
      int dotPos = config.entry.LastIndexOf(".");
      toWrite.type = new string(config.entry.Skip(dotPos).ToArray());

      toWrite = ParseFile.ParseImports(toWrite); // Parse the file
      Console.WriteLine("Done parsing files");
    }

    // Wait for the file to be available for writing
    while (!IsFileReady(config.write_location) && File.Exists(config.write_location)) ;
    Console.WriteLine("Writing");
    File.WriteAllText(config.write_location, toWrite.ToStringy(new List<int> {-1}));
  }

  // https://stackoverflow.com/questions/1406808/wait-for-file-to-be-freed-by-process
  public static bool IsFileReady(string filename) {
    // If the file can be opened for exclusive access it means that the file
    // is no longer locked by another process.
    try
    {
      using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
        return true;
    }
    catch (System.UnauthorizedAccessException) {
      Console.WriteLine("Couldn't access the file '"+filename+"' due to insufficient permissions, are you trying to write to a folder instead of a file?");
      System.Environment.Exit(0);
    }
    catch {
      return false;
    }

    return false;
  }
}

class Script {
  public string text = "";
  public string type = "sc";
  public List<ImportedScript> importedFiles = new List<ImportedScript>();
  public string ToStringy(List<int> includedLines) {
    List<string> newText = new List<string>();

    // Create a list of things which is all the things that need to be written.
    // The zeroth index tells the line number, the first tells whether it's a line (1) or a file to stringify (0), the second tells the index in the importedFiles array (if applicable)
    List<int[]> things = new List<int[]>();
    for (int i = 0; i < importedFiles.Count; i++) {
      things.Add(new int[] {ParseScarpet.LineNumber(text, importedFiles[i].position), 0, i});
    }
    string[] lines = text.Split('\n');
    for (int i = 0; i < lines.Length; i++) {
      if (importedFiles.FindIndex(x => x.position == i) == -1) {
        things.Add(new int[] {i, 1});
      }
    }
    things.Sort((a, b) => a[0]-b[0]);

    // Add all the thingies to the newText list
    for (int i = 0; i < things.Count; i++) {
      if (includedLines[0] != -1 && includedLines.IndexOf(i) == -1) continue; // Skip lines that aren't included

      if (things[i][1] == 1) {
        newText.Add(lines[things[i][0]]);
      } else {
        newText.Add(importedFiles[things[i][2]].file.ToStringy(importedFiles[things[i][2]].includedLines));
      }
    }

    return string.Join('\n', newText);
  }
  static int LineNumber(string str, int pos) {
    // https://stackoverflow.com/questions/7255743/what-is-simpliest-way-to-get-line-number-from-char-position-in-string
    return str.Take(pos).Count(c => c == '\n') + 1;
  }
}

class ImportedScript {
  public Script file = new Script();
  public int position = 0;
  public List<int> includedLines = new List<int>();
}