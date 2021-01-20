using System.Collections.Generic;
using System;
using System.Net;
using System.IO;
using System.Linq;

class ParseFile {
  public List<string> imported = new List<string>();
  public string ParseImports(string file) {
    string newFile = file;
    // Find all places where the import function is called
    List<ScarpetFunction> funcs = ParseScarpet.FindFunctions(file, "import");

    // Loop through the list backwards so nothing will affect anything that happens after it
    for (int i = funcs.Count-1; i >= 0; i--) {
      // If no variables to import were included, import everything
      if (funcs[i].args.Length == 1) {
        newFile = includeFile(newFile, funcs[i]);
      } else {
        // Just import the variables that were included, and do tree shaking, will do later
      }
    }

    return newFile;
  }

  int LineNumber(string str, int pos) {
    // https://stackoverflow.com/questions/7255743/what-is-simpliest-way-to-get-line-number-from-char-position-in-string
    return str.Take(pos).Count(c => c == '\n') + 1;
  }

  string includeFile(string file, ScarpetFunction f) {
    string filePath = f.args[0].Substring(1, f.args[0].Length-2);

    // Add the imported file to the list to prevent circular dependencies from crashing the program
    if (!imported.Contains(filePath)) {
      imported.Add(filePath);
    } else {
      Console.WriteLine("Warning: Ignoring circular dependency "+f.args[0]);
      return file.Remove(f.startPos, f.endPos-f.startPos);
    }

    string importedFile = "";

    // If it's not a valid url, it will catch and read it as a file
    try {
      // https://stackoverflow.com/questions/27108264/how-to-properly-make-a-http-web-get-request
      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(filePath);
      request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
      using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
      using(Stream stream = response.GetResponseStream())
      using(StreamReader reader = new StreamReader(stream))
      {
          importedFile = reader.ReadToEnd();
      }
    } catch {
      // Check if the imported file exists
      if (!File.Exists(filePath)) {
        Console.WriteLine("File '"+filePath+"' doesn't exist  Line "+LineNumber(file, f.startPos));
        System.Environment.Exit(0);
      }

      // Read the imported file
      while (!BundleFile.IsFileReady(filePath)) ;
      using(StreamReader sr = new StreamReader(filePath)) {
        importedFile = sr.ReadToEnd();
      }
    }

    // Recursively parse the file's imports
    importedFile = ParseImports(importedFile);
    
    // Replace the import statement with what was read
    return file.Remove(f.startPos, f.endPos-f.startPos).Insert(f.startPos, importedFile);
  }
}
