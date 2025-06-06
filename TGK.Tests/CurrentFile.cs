using System;
using System.Runtime.CompilerServices;

namespace TGK.Tests;

public static class CurrentFile
{
    public static string Path([CallerFilePath] string file = "") => file;

    public static string Directory([CallerFilePath] string file = "") => System.IO.Path.GetDirectoryName(file)!;

    public static string Relative(string relative, [CallerFilePath] string file = "")
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(relative);

        string directory = System.IO.Path.GetDirectoryName(file)!;
        return System.IO.Path.Combine(directory, relative);
    }
}