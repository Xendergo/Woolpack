using System.Collections.Generic;
using System.Linq;
class ParseScarpet {
  public static List<ScarpetFunction> FindFunctions(string file, string name) {
    List<ScarpetFunction> ret = new List<ScarpetFunction>();
    // Find all instances of the function being called
    List<int> comments = SearchString(file, "//");
    List<int> instances = SearchString(file, name+"(").Where(v => !CommentedOut(file, comments, v)).ToList();

    // Loop through all the instances
    for (int i = 0; i < instances.Count; i++) {
      ScarpetFunction f = new ScarpetFunction();
      f.startPos = instances[i];
      f.name = name;

      // Set the pos to the start of the arguments given
      int pos = f.startPos+name.Length+1;
      List<string> args = new List<string>();

      // Loop through all the arguments
      while (file[pos] != ')') {
        string arg = "";
        // Go through the argument and add characters to the argument until a stopping character is hit
        while (file[pos] != ',' && file[pos] != ')') {
          // Ignore escape characters
          if (file[pos] == '\\') {
            pos++;
          }

          arg+=file[pos];
          pos++;
        }

        args.Add(arg);

        // If there are spaces ahead, get rid of them
        if (file[pos] != ')') {
          pos++;
          while (file[pos] == ' ') {
            pos++;
          }
        }
      }

      f.args = args.ToArray();

      // Get rid of any semicolons
      if (file[pos+1] == ';') {
        pos++;
      }

      f.endPos = pos+1;

      ret.Add(f);
    }

    return ret;
  }

  static bool CommentedOut(string file, List<int> comments, int pos) {
    for (int i = 0; i < comments.Count; i++) {
      if (comments[i] > pos) return false;
      if (LineNumber(file, comments[i]) == LineNumber(file, comments[i])) return true;
    }

    return false;
  }

  static int LineNumber(string str, int pos) {
    // https://stackoverflow.com/questions/7255743/what-is-simpliest-way-to-get-line-number-from-char-position-in-string
    return str.Take(pos).Count(c => c == '\n') + 1;
  }

  static List<int> SearchString(string str, string searchFor) {
    List<int> ret = new List<int>();

    // Use IndexOf to find the first instance after i, then set i to the index so it doesn't find the same thing over again, repeat until nothing else was found
    int i = 0;
    while (i != -1) {
      i = str.IndexOf(searchFor, i);

      if (i != -1) {
        ret.Add(i);
        i++;
      }
    }

    return ret;
  }
}

class ScarpetFunction {
  public string name;
  public int startPos;
  public int endPos;
  public string[] args;
}