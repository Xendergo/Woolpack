using System.Collections.Generic;
using System;
using System.Net;
using System.IO;
using System.Linq;

class ParseFile {
  public List<string> imported = new List<string>();
  public Script ParseImports(Script file) {
    int moveBackwards = 0;
    Script newFile = file;
    // Find all places where the import function is called
    List<ScarpetFunction> funcs = ParseScarpet.FindFunctions(file.text, "import");

    for (int i = 0; i < funcs.Count; i++) {
      funcs[i].startPos -= moveBackwards;
      funcs[i].endPos -= moveBackwards;
      newFile = includeFile(newFile, funcs[i]);
      moveBackwards += funcs[i].endPos-funcs[i].startPos;
    }

    return newFile;
  }

  int LineNumber(string str, int pos) {
    // https://stackoverflow.com/questions/7255743/what-is-simpliest-way-to-get-line-number-from-char-position-in-string
    return str.Take(pos).Count(c => c == '\n') + 1;
  }

  Script includeFile(Script file, ScarpetFunction f) {
    string filePath = f.args[0].Substring(1, f.args[0].Length-2);

    if (filePath == "./example/formatType.sc") {

    }

    // Add the imported file to the list to prevent circular dependencies from crashing the program
    if (!imported.Contains(filePath)) {
      imported.Add(filePath);
    } else {
      Console.WriteLine("Warning: Ignoring circular dependency "+f.args[0]);
      file.text = file.text.Remove(f.startPos, f.endPos-f.startPos);
      return file;
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
        Console.WriteLine("File '"+filePath+"' doesn't exist  Line "+LineNumber(file.text, f.startPos));
        System.Environment.Exit(0);
      }

      // Read the imported file
      while (!BundleFile.IsFileReady(filePath)) ;
      using(StreamReader sr = new StreamReader(filePath)) {
        importedFile = sr.ReadToEnd();
      }
    }

    ImportedScript importedScript = new ImportedScript();
    importedScript.file.text = importedFile;

    int dotPos = filePath.LastIndexOf(".");
    importedScript.file.type = new string(filePath.Skip(dotPos).ToArray());

    // Recursively parse the file's imports
    importedScript.file = ParseImports(importedScript.file);

    importedScript.position = f.startPos;

    if (f.args.Length == 1) {
      // Include all the lines
      int lines = LineNumber(importedScript.file.text, importedScript.file.text.Length-1);
      for (int i = 0; i < lines; i++) {
        importedScript.includedLines.Add(i);
      }
      Console.WriteLine(filePath);
      Console.WriteLine(importedScript.includedLines.Count);
    } else {
      // Tree shaking, will do later
    }

    // Add the imported script to the list
    file.importedFiles.Add(importedScript);
    
    // Remove the import statement
    file.text = file.text.Remove(f.startPos, f.endPos-f.startPos);
    return file;
  }
}
