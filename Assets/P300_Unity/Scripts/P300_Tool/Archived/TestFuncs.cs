using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFuncs : MonoBehaviour
{

    private string m_Path;
    string result = string.Empty;
    private string output;

    private void Start()
    {
        m_Path = Application.dataPath;
    }
    public string RunTestPython()
   {

        try
        {
            using (Process myProcess = new Process())
            {
                myProcess.StartInfo.FileName = "cmd.exe";
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.StartInfo.RedirectStandardInput = true;
                myProcess.StartInfo.RedirectStandardOutput = true;
                myProcess.StartInfo.UseShellExecute = false;
                myProcess.Start();
                myProcess.StandardInput.WriteLine("conda init cmd.exe");
                myProcess.StandardInput.WriteLine("conda --version");
                myProcess.StandardInput.WriteLine("python --version");
                myProcess.StandardInput.WriteLine("conda activate bci_online");
                //myProcess.StandardInput.WriteLine("python Assets/P300_Unity/Python/P300_Python_Backend/erp_offline_test.py");
                myProcess.StandardInput.WriteLine("python Assets/P300_Unity/Python/P300_Python_Backend/test.py");
                myProcess.StandardInput.Flush();
                myProcess.StandardInput.Close();
                UnityEngine.Debug.Log(myProcess.StandardOutput.ReadToEnd());




                //myProcess.StartInfo.Arguments = (argumentsVar[0] + "&" + argumentsVar[1] + " " + pythonScriptName);
                //myProcess.StartInfo.Arguments = "conda activate Unity & python \"" + m_Path+ "/P300_Unity/Python/Test.py\"";

                ////myProcess.StartInfo.FileName = @"D:\\Users\Eli\Anaconda3\python.exe";
                //myProcess.StartInfo.Arguments = @"D:\\Program Files\Unity\BCI_Calgary\P300_Basics_v2\Assets\P300_Unity\Python\Test.py";
                ////myProcess.StartInfo.Arguments = @"D:\\Test.py"; //This is working here, but not sure why it won't work in a local directory. TODO: Fix this.
                //string fileName = "Test.py";
                //string path = Path.Combine(Environment.CurrentDirectory, @"\python\", fileName);
                //string fullPath = m_Path + "\\python\\"+ "fileName";
                //myProcess.StartInfo.Arguments = @m_Path.ToString();
                /*
                UnityEngine.Debug.Log(m_Path);

                myProcess.StartInfo.UseShellExecute = false;
                myProcess.StartInfo.CreateNoWindow = false;
                myProcess.StartInfo.RedirectStandardInput = false;
                myProcess.StartInfo.RedirectStandardOutput = true;
                UnityEngine.Debug.Log("FileName: " + myProcess.StartInfo.Arguments);
                myProcess.Start();
                myProcess.WaitForExit();
                if (myProcess.ExitCode == 0)
                {
                    result = myProcess.StandardOutput.ReadToEnd();
                }
                else
                {
                    result = "Did not work. Exit Code: " + myProcess.ExitCode.ToString();
                    
                }
                */
            }
            
            UnityEngine.Debug.Log("!!!!!!!!!!!!!!!!!!HEY YOU RAN THE SECOND PROCESS!!!!");

            return result;

            
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e);
            result = "Failed";
            return result;

        }

    }



    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            output = RunTestPython();
            UnityEngine.Debug.Log("Output from test: " + output);
        }
    }
}
