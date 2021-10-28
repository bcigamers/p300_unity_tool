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
public class Inlet_P300_Event : AStringInlet
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
    public int objectID;
    private int[] cubeIndices;

    //For Debug purposes only!
    [SerializeField]
    private string selectedString = "s,0";

    //Override the Process call from AInlet.CS
    protected override void Process(string[] newSample, double timeStamp)
    {
        //Avoid doing heavy processing here, use CoRoutines
        input = newSample[0];
        timestamp = timeStamp;

        //OLD
        ////Obtain necessary information from the P300_Flashes.cs file. 
        //GameObject p300Controller = GameObject.FindGameObjectWithTag("BCI");
        //print("CONTROLLER: " + p300Controller.name);
        //P300_Flashes p300Flashes = p300Controller.GetComponent<P300_Flashes>();
        //object_list = p300Flashes.object_list;
        //freqHz = p300Flashes.freqHz;
        //onColour = p300Flashes.onColour;
        //offColour = p300Flashes.offColour;
        //numRows = p300Flashes.numRows;
        //cubeIndices = new int[numRows];

        //NEW
        //Obtain necesary information from the P300_Controller.cs object.
        //POTENTIAL SOURCE OF BUG HERE
        GameObject p300Controller = GameObject.FindGameObjectWithTag("BCI");
        print("CONTROLLER: " + p300Controller.name);
        P300_Controller p300Flashes = p300Controller.GetComponent<P300_Controller>();
        object_list = p300Flashes.object_list;
        freqHz = p300Flashes.freqHz;
        onColour = p300Flashes.onColour;
        offColour = p300Flashes.offColour;
        numRows = p300Flashes.numRows;
        cubeIndices = new int[numRows];

        //Call CoRoutine to do further processing
        StartCoroutine("SelectedCube");
    }

    //Just for testing purposes

    //public void Update()
    //{
    //    if(Input.GetKeyDown(KeyCode.B))
    //    {
    //        Debug.Log("TESTING OUT THE INLET_P300_EVENT CODE");
    //        input = selectedString;
    //        StartCoroutine("SelectedCube");
    //    }
    //}


    IEnumerator SelectedCube(){
        print("Input Received: " + input + " at: " + timestamp);

        //Used for simulation purposes.
        if (input == "P300 SingleFlash Begins" || input == "P300 RCFlash Begins")
        {
            sim_start_time = timestamp;
            yield return new WaitForSecondsRealtime(2);
            
        } 
        else if (input == "P300 SingleFlash Ends" || input == "P300 RCFlash Ends")
        {
            sim_end_time = timestamp;
            yield return new WaitForSecondsRealtime(2);
            
        }
        
        //If not above, then it will be the following actions below.

        //Split the string into it's classifier and value
            string[] input_split = input.Split(',');
            string classifier = input_split[0];
        
            //What to do if the classifier is single target value.
            if(classifier == "s"){
                //print("Single Flash Value");
                objectID = Int32.Parse(input_split[1]);
                print("\tCube Value: " + objectID.ToString());

                //Do something with the selected object ID!

                //Hardcoded example
                //object_list[objectID].GetComponent<Renderer>().material.color = Color.red;

                //Using the P300 Event system!
                P300Events.current.TargetSelectionEvent(objectID);


            } else {
                print("Error: Classifier Value is: " + classifier);
                Debug.Log("Selected Object Id = " + objectID);
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