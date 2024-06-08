using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hellfighter;

public static class TheUtils {

    public static string CurrentPath {
        get {
            // This will get the current WORKING directory (i.e. \bin\Debug)
            var workingDirectory = Environment.CurrentDirectory;
            // or: Directory.GetCurrentDirectory() gives the same result

            // This will get the current PROJECT directory
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;

            return projectDirectory;
        }
    }
}
