using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Assets.LSL4Unity.Scripts.AbstractInlets;
using LSL;

/// <summary>
/// P300 Unity Inlet
/// Gathers information from Matlab and lights up the corresponding cube
/// </summary>
public class Inlet_P300 : AStringInlet
{
    private string input = "";
    private double timestamp;
    private float freqHz;
    private List<GameObject> object_list;
    private Color onColour;
    private Color offColour;
    private double sim_start_time;
    private double sim_end_time;
    private int numRows;
    public int cubeIndex;
    private int[] cubeIndices;
    protected override void Process(string[] newSample, double timeStamp)
    {
        //Avoid doing heavy processing here, use CoRoutines
        input = newSample[0];
        timestamp = timeStamp;

        //OLD
        //Obtain necessary information from the P300_Flashes.cs file. 
        GameObject p300Controller = GameObject.FindGameObjectWithTag("BCI");
        print("CONTROLLER: " + p300Controller.name);
        P300_Flashes p300Flashes = p300Controller.GetComponent<P300_Flashes>();
        object_list = p300Flashes.cube_list;
        freqHz = p300Flashes.freqHz;
        onColour = p300Flashes.onColour;
        offColour = p300Flashes.offColour;
        numRows = p300Flashes.numRows;
        cubeIndices = new int[numRows];

        //NEW
        //Obtain necesary information from the P300_Controller.cs object.
        //POTENTIAL SOURCE OF BUG HERE
        //GameObject p300Controller = GameObject.FindGameObjectWithTag("BCI");
        //print("CONTROLLER: " + p300Controller.name);
        //P300_Controller p300Flashes = p300Controller.GetComponent<P300_Controller>();
        //object_list = p300Flashes.object_list;
        //freqHz = p300Flashes.freqHz;
        //onColour = p300Flashes.onColour;
        //offColour = p300Flashes.offColour;
        //numRows = p300Flashes.numRows;
        //cubeIndices = new int[numRows];

        //Call CoRoutine to do further processing
        StartCoroutine("SelectedCube");
    }

    IEnumerator SelectedCube(){
        print("Input Received: " + input + " at: " + timestamp);
        if (input == "P300 SingleFlash Begins" || input == "P300 RCFlash Begins"){
            sim_start_time = timestamp;
        } else if (input == "P300 SingleFlash Ends" || input == "P300 RCFlash Ends"){
            sim_end_time = timestamp;
        } else {
            //Split the string into it's classifier and value
            string[] input_split = input.Split(',');
            string classifier = input_split[0];
        
            if(classifier == "s"){
                //print("Single Flash Value");
                cubeIndex = Int32.Parse(input_split[1]);
                print("\tCube Value: " + cubeIndex.ToString());
                object_list[cubeIndex].GetComponent<Renderer>().material.color = Color.red;
            } else if(classifier == "r"){
                //print("Row Flash Value");
                 
                //Removing the classifier from the array and converting to ints
                int[] rowValues = new int[input_split.Length-1];
                for(int i = 0; i < input_split.Length-1; i++){
                    rowValues[i] = Int32.Parse(input_split[i+1]);
                    object_list[rowValues[i]].GetComponent<Renderer>().material.color = Color.green;
                    cubeIndex = rowValues[i];
                }
            } else if(classifier == "c"){
                //print("Column Flash Value");
                 
                //Removing the classifier from the array and converting to ints
                int[] columnValues = new int[input_split.Length-1];
                for(int i = 0; i < input_split.Length-1; i++){
                    columnValues[i] = Int32.Parse(input_split[i+1]);
                    object_list[columnValues[i]].GetComponent<Renderer>().material.color = Color.green;
                    cubeIndex = columnValues[i];
                }
            } else {
                print("Error: Classifier Value is: " + classifier);
                Debug.Log("Cube Index = " + cubeIndex);
            }    
        }
        yield return new WaitForSecondsRealtime(2);
        TurnOff();


    }

    public void TurnOff(){
        for(int i = 0; i < object_list.Count; i++){
            object_list[i].GetComponent<Renderer>().material.color = offColour;
        }
    }

    public void ResolveOnRequest()
    {
        liblsl.StreamInfo[] results;
        results = liblsl.resolve_streams();
        StartCoroutine("ResolveExpectedStream");
    }
}