using System.Collections.Generic;
using System.Linq;
using System;

class ParseScarpet {
  static char[] specials = new char[] {'(', ')', ':', ';', ' ', '\n', '~', '\'', ',', '\t', '=', '~', '{', '}', '[', ']', '+', '-', '>', '<', '%', '*', '/', '^', '&', '|'};
  static char[] selectors = new char[] {':', '~'};
  static char[] nums = new char[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
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
      if (LineNumber(file, comments[i]) == LineNumber(file, pos)) return true;
    }

    return false;
  }

  static bool IsDefinition(string file, int pos, int symbolLength) {
    int end = pos + symbolLength;

    // Get rid of leading whitespace
    while (file[end] == ' ') {
      end++;
    }

    // If it's function, go through the arguments
    if (file[end] == '(') {
      while (file[end] != ')') {
        end++;
      }

      end++;

      // Go through all the spaces
      while (file[end] == ' ') {
        end++;
      }

      // If what's left is '->' then it's a function assignment
      return file[end] == '-' && file[end + 1] == '>';
    } else {
      // If what's left is '=' then it's a variable assignment
      return file[end] == '=';
    }
  }
  public static int parenthesesAmt(string file, int pos) {
    var took = file.Take(pos);
    // Opening parentheses - Closing parenthese
    return took.Count(c => c == '(')-took.Count(c => c == ')');
  }
  public static List<int> FindDefinitions(string file, string symbol) {
    List<int> comments = SearchString(file, "//");
    return SearchString(file, symbol).Where(x => !CommentedOut(file, comments, x) && IsDefinition(file, x, symbol.Length)).ToList();
  }

  public static HashSet<string> FindSymbolsDefined(string line) {
    List<int> equalSigns = SearchString(line, "=");

    HashSet<string> symbols = new HashSet<string>();

    for (int i = 0; i < equalSigns.Count; i++) {
      // filter out '==', '!=' '>=', '<=', '+=', '-='
      if (line[equalSigns[i]-1] == '=' || line[equalSigns[i]-1] == '!' || line[equalSigns[i]-1] == '>' || line[equalSigns[i]-1] == '<' || line[equalSigns[i]-1] == '+' || line[equalSigns[i]-1] == '-' || line[equalSigns[i]+1] == '=') continue;

      int pos = equalSigns[i]-1;

      // Get rid of the spaces
      while (line[pos] == ' ') {
        pos--;
      }

      // Read out the symbol
      string symbol = "";
      while (pos > 0 && !specials.Contains(line[pos])) {
        symbol = line[pos] + symbol;
        pos--;
      }

      // Get rid of more spaces
      while (line[pos] == ' ') {
        pos--;
      }

      // If this is actually a situation like `v: x = c` then skip this symbol
      if (selectors.Contains(line[pos])) continue;

      symbols.Add(symbol);
    }

    List<int> functions = SearchString(line, "->");

    for (int i = 0; i < functions.Count; i++) {
      int pos = functions[i] - 1;

      // Remove spaces
      while (line[pos] == ' ') {
        pos--;
      }

      // If the position isn't ')', then that means it must be a map instead of a function definition
      if (line[pos] != ')') continue;

      // Remove ')'
      pos--;

      while (line[pos] != '(') {
        string symbol = "";

        // Remove spaces & commas
        while (line[pos] == ' ' || line[pos] == ',') {
          pos--;
        }

        // If this is closing an expression, it's either a syntax error or an outer function, and should be ignored either way
        if (line[pos] == ')') {
          pos--;
          int parenthesesValue = parenthesesAmt(line, pos);
          // Get rid of whatever's in the parentheses
          while (parenthesesAmt(line, pos) >= parenthesesValue) {
            pos--;
          }

          // Get rid of stuff until it's the next symbol or the arguments have ended
          while (line[pos] != ',' || line[pos] != '(') {
            pos--;
          }
        } else {
          // Add the symbol
          while (line[pos] != ' ' && line[pos] != ',' && line[pos] != '(') {
            symbol = line[pos] + symbol;
            pos--;
          }
        }

        if (symbol != "") {
          symbols.Add(symbol);
        }
      }
    }

    return symbols;
  }

  public static HashSet<string> SymbolsReferenced(string line) {
    int pos = 0;
    HashSet<string> symbols = new HashSet<string>();

    int commentPos = line.IndexOf("//");
    if (commentPos != -1) {
      line = line.Substring(0, commentPos);
    }

    while (pos < line.Length) {
      // Remove special characters
      while (pos < line.Length && specials.Contains(line[pos])) {
        pos++;
      }

      if (pos == line.Length) continue;

      // If it starts out as a number or ', it can't be a valid variable name, so skip
      if (nums.Contains(line[pos]) || line[pos] == '\'') {
        while (!specials.Contains(line[pos])) {
          pos++;
        }

        continue;
      }

      string symbol = "";

      // Add the symbol to the list
      while (pos < line.Length && !specials.Contains(line[pos])) {
        symbol += line[pos];
        pos++;
      }

      // Remove spaces
      while (pos < line.Length && line[pos] == ' ') {
        pos++;
      }

      // If the symbol is being defined using '=' or '->', skip it
      if (pos < line.Length && (line[pos] == '=' || (line[pos] == '-' && line[pos] == '>'))) continue;

      // Check if it's a function definition or function call
      if (pos < line.Length && line[pos] == '(') {
        // Copy the pos so we can skip ahead and check for '->'
        int posC = pos;

        int parenthesesValue = parenthesesAmt(line, posC);
        posC++;
        // Skip to the end of the parentheses
        while (posC < line.Length && parenthesesValue < parenthesesAmt(line, posC)) {
          posC++;
        }

        try {
          // Remove spaces
          while (line[posC] == ' ') {
            posC++;
          }

          // Check for '->', and if it is, skip the function arguments
          if (line[posC] == '-' && line[posC+1] == '>') {
            pos = posC;
            continue;
          }
        } catch {}
      }

      symbols.Add(symbol);
    }

    // Deal with outer(), which would be skipped normally
    List<ScarpetFunction> outer = FindFunctions(line, "outer");
    foreach (ScarpetFunction o in outer) {
      symbols.UnionWith(o.args.ToHashSet());
    }

    return symbols;
  }

  public static int LineNumber(string str, int pos) {
    // https://stackoverflow.com/questions/7255743/what-is-simpliest-way-to-get-line-number-from-char-position-in-string
    return str.Take(pos).Count(c => c == '\n') + 1;
  }
  public static int indexFromLine(string str, int line) {
    int amt = line;
    int i = 0;
    while (amt > 0) {
      i++;
      if (str[i] == '\n') amt--;
    }

    return i;
  }

  public static int indexEndFromLine(string str, int line) {
    int amt = line + 1;
    int i = 0;
    while (amt > 0) {
      i++;
      if (str[i] == '\n') amt--;
    }

    return i - 1;
  }

  public static string getLine(string str, int line) {
    return new string(str.Skip(indexFromLine(str, line)+1).TakeWhile(x => x != '\n').ToArray());
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