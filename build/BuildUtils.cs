using Serilog;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Text;

public static class BuildUtils
{
    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public static string GetProjectName()
    {

        // Get the entry assembly (the assembly that contains the Main method)
        Assembly assembly = Assembly.GetEntryAssembly();

        // Get the full path of the assembly (including the file name)
        string assemblyPath = assembly.Location;

        // Get the file name (including the extension)
        string assemblyFileName = System.IO.Path.GetFileName(assemblyPath);

        return assemblyFileName;
    }

    /// <summary>
    /// Replaces . for - to make a Sonarqube-compatible project key.
    /// </summary>
    /// <param name="projectName"></param>
    /// <returns></returns>
    public static string ReplaceDotsToDashes(string projectName) => projectName.Replace(".", "-");

    /// <summary>
    ///
    /// </summary>
    /// <param name="stdOutBuffer"></param>
    /// <param name="stdErrBuffer"></param>
    /// <returns></returns>
    public static string ExtractVersion(StringBuilder stdOutBuffer, StringBuilder stdErrBuffer)
    {

        var stdOut = stdOutBuffer.ToString();
        var stdErr = stdErrBuffer.ToString();

        var withoutSpeechMarks = stdOut.Replace("\"", "");

        // Remove square brackets
        var withoutSquareBrackets = withoutSpeechMarks.Replace("[", "")
            .Replace("]", "");

        var lines = withoutSquareBrackets.Split(',');

        var match = Regex.Match(lines[2], @"\d");

        // Check if a number is found
        if (match.Success)
        {
            // Remove the part up to the first number
            string octopusVersion = lines[2].Substring(match.Index).Trim();
            Log.Information(octopusVersion);
            return octopusVersion;
        }

        return "";

    }
}
