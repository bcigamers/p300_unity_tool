using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleFlash : MonoBehaviour
{
    //Main connection to the P300 controller.
    [SerializeField] P300_Controller p300_Controller;

    public bool startFlashes;   //Whether to automatically start the flashes on awake.

    //Variables associated with counting the flashes

    private int counter = 0;
    private List<int> flash_counter = new List<int>();
    private List<int> s_indexes = new List<int>();
    private int numTrials = 0; //CAUTION- THIS MAY BE WHAT IS CAUSING AN ISSUE IF NOT RESET APPROPRIATELY.



    private void Awake()
    {
        p300_Controller = GetComponent<P300_Controller>();
    }

         
    /* Configures array and simulation values for Single flashes */
    public void SetUpSingle()
    {
        /*
                Grid                  Number
        c00     c10     c20         0   1   2

        c01     c11     c21     ->  3   4   5

        c02     c12     c22         6   7   8
        */

        int numRows = p300_Controller.numRows;
        int numCols = p300_Controller.numColumns;
        int numSamples = p300_Controller.numFlashes;
        
        
        //Setting counters for each cube
        for (int i = 0; i < numRows * numCols; i++)
        {
            flash_counter.Add(numSamples);
        }

        numTrials = numSamples * (numRows * numCols);
        //s_trials = numTrials; //THIS IS USED IN WRITING INFO TO LSL. NEED TO THINK ABOUT HOW TO GET THIS BETTER OUT.

        //Set up test single indices
        for (int i = 0; i < (numRows * numCols); i++)
        {
            s_indexes.Add(i);
        }

        print("---------- SINGLE FLASH DETAILS ----------");
        print("Number of Trials will be: " + numTrials);
        print("Number of flashes for each cell: " + numSamples);
        print("--------------------------------------");
        TurnOffSingle();
        startFlashes = !startFlashes;
    }

    //Simple call to run or stop the coroutines based on if the start_flashes call is true or not
    public void SingleFlashes()
    {
        if (startFlashes)
        {
            StartCoroutine("SingleFlashCor");
        }
        else
        {
            StopSingleFlashes();
        }

    }

    public void StopSingleFlashes()
    {
        //Turn off the flash boolean and stop the coroutine.
        startFlashes = false;
        StopCoroutine("SingleFlashCor");
        p300_Controller.WriteMarker("P300 SingleFlash Ends");
        ResetSingleCounters();
        print("Counters Reset! Hit S again to run P300 SingleFlash");
    }
    
    /* Single Flash Operation */
    IEnumerator SingleFlashCor()
    {
        //Write that this coroutine has started
        p300_Controller.WriteMarker("P300 SingleFlash Started");

        while (startFlashes)
        {
            int randomCube;
            int randomIndex;
            //Generate a random number from the list of indices that have non-zero counters
            System.Random random = new System.Random();
            randomIndex = random.Next(s_indexes.Count);
            randomCube = s_indexes[randomIndex];

            //Turn off the cubes to give the flashing image
            TurnOffSingle();

            //If the counter is non-zero, then flash that cube and decrement the flash counter
            if (flash_counter[randomCube] > 0)
            {
                yield return new WaitForSecondsRealtime((1f / p300_Controller.freqHz));

                p300_Controller.object_list[randomCube].GetComponent<Renderer>().material.color = p300_Controller.onColour;

                //Handle events if this is the target cube or not //NEW!
                if (randomCube == p300_Controller.TargetObjectID)
                {
                    OnTargetFlash();
                }
                else
                {
                    OnNonTargetFlash();
                }


                flash_counter[randomCube]--;
                counter++;
                print("OBJECT: " + randomCube.ToString());

                //Write to the LSL Outlet stream
                p300_Controller.WriteMarker("s," + randomCube.ToString());
            }
            else if (numTrials == counter)
            {
                print("Done P300 Single Flash Trials");
                break;
            }
            else
            {
                //If the counter for a specific cube has reached zero, then remove it from the indexes so that the random
                //number generator does not pick it again (to reduce lag)
                if (flash_counter[randomCube] == 0)
                {
                    s_indexes.RemoveAt(randomIndex);
                }
                //Go to the next iteration of the single flash 
                continue;
            }

            yield return new WaitForSecondsRealtime(p300_Controller.dutyCycle);

        }
        ResetSingleCounters();
        //Write to LSL stream to indicate end of P300 SingleFlash
        //This is all things to do on the P300 controller.
        p300_Controller.WriteMarker("P300 SingleFlash Ends");//marker.Write("P300 SingleFlash Ends");
        startFlashes = !startFlashes;
        p300_Controller.LockKeysToggle(KeyCode.S);//keyLocks[KeyCode.S] = !keyLocks[KeyCode.S];

    }

    //Turn off all object values
    public void TurnOffSingle()
    {
        for (int i = 0; i < p300_Controller.object_list.Count; i++)
        {
            p300_Controller.object_list[i].GetComponent<Renderer>().material.color = p300_Controller.offColour;
        }
    }

    /* Resets all counters and clear arrays */
    public void ResetSingleCounters()
    {
        counter = 0;
        flash_counter.Clear();
        s_indexes.Clear();
        numTrials = 0;
    }


    //TODO: Add back in Redraw capabilities for rapid changes.

    public void Redraw()
    {
            print("Redrawing Matrix");
            TurnOffSingle();
            ResetSingleCounters();
            p300_Controller.object_list.Clear();
            SetUpSingle();
       
    }

 

    //Dealing with events
    private void OnTargetFlash()
    {
        P300Events.current.TargetFlashEvent();
    }

    private void OnNonTargetFlash()
    {
        P300Events.current.NonTargetFlashEvent();
    }
}
