using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

// Runs python backend and saves data
public class RunPython : MonoBehaviour
{
    // Settings for python backend
    public string p300Path = @"C:\Users\brian\Documents\p300\p300_python_fresh\src";

    public bool saveData = true;
    public string user = "test";

    private string todaysDate;
    private int dailyFileCount;
    public string fileName;

    // 
    private float window_start;
    private float window_end;
    private float eeg_start;
    private float buffer;
    private int max_windows;
    private int max_decisions;
    private int max_loops;

    // Signal Processing Settings

    // Classification Settings

    // Run python
    /*
    ProcessStartInfo start = new ProcessStartInfo();
    start.Filename = "cmd.exe";
    start.UseShellExecute = false;
    start.RedirectStandardOutput  true;
    using(Process process = Process.Start(start))
    {
        using(StreamReader reader = process.StandardOutput)
        {
            
        }
    }
    */
    //2
    public void RunP300Python()
    {
        // Generate the filename from todays date and the filecount

        // Check to make sure that there is not something saved in the given location

        Process cmd = new Process();
        cmd.StartInfo.FileName = "cmd.exe";
        cmd.StartInfo.RedirectStandardInput = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.UseShellExecute = false;
        cmd.Start();

        cmd.StandardInput.WriteLine("cd " + p300Path);
        cmd.StandardInput.WriteLine("conda activate bci_online");
        cmd.StandardInput.WriteLine("python --version");
        cmd.StandardInput.WriteLine("conda --version");
        cmd.StandardInput.WriteLine("python erp_online_test.py");

        cmd.StandardInput.Flush();
        cmd.StandardInput.Close();
        cmd.WaitForExit();
        Console.WriteLine(cmd.StandardOutput.ReadToEnd());

    }



}
