using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class ExecCommand {

    public static string ExecuteCommand(string command)
    {
        int exitCode;
        ProcessStartInfo processInfo;
        Process process;

#if UNITY_EDITOR_OSX
        command = command.Replace("curl", "");
        processInfo = new ProcessStartInfo("curl", command);
#elif UNITY_EDITOR_WIN
        processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
#endif

        processInfo.CreateNoWindow = true;
        processInfo.UseShellExecute = false;
        // *** Redirect the output ***
        processInfo.RedirectStandardError = true;
        processInfo.RedirectStandardOutput = true;

        process = Process.Start(processInfo);
        process.WaitForExit();

        // *** Read the streams ***
        // Warning: This approach can lead to deadlocks, see Edit #2
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        exitCode = process.ExitCode;

        
        process.Close();

        return output + " : Errors " + error + " finished";
    }
}
