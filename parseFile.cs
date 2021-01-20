using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

class ParseFile {
  public List<string> imported = new List<string>();
  public string ParseImports(string file) {
    Console.WriteLine("imports");
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

    // Check if the imported file exists
    if (!File.Exists(filePath)) {
      Console.WriteLine("File '"+filePath+"' doesn't exist  Line "+LineNumber(file, f.startPos));
      System.Environment.Exit(0);
    }

    string importedFile;
    // Read the imported file & recursively process it's imports
    using(StreamReader sr = new StreamReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))) {
      importedFile = ParseImports(sr.ReadToEnd());
    }
    


    // Replace the import statement with what was read
    return file.Remove(f.startPos, f.endPos-f.startPos).Insert(f.startPos, importedFile);
  }
}
