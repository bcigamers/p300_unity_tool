using System.Net.Mime;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Assets.LSL4Unity.Scripts;
using Assets.LSL4Unity;

/*
P300 Flashes Program
Author: Shaheed Murji
Adapted from: Eli Kinney-Lang "Matching_Demo/FlashScript.cs"

    User configured matrix which flashes cubes either in a single or row/column fashion. User defined parameters which allow 
    a variety of different use cases.

    Parameters: 
    o	Freq Hz (Hz): this is how frequent the cubes will be flashing
    o	Flash Length (s): how long a cube will be on for during a flash
    o	Num Samples (#): number of times a single cube will flash in a trial
    o	Distance X (units): distance between each cube on the x-axis
    o	Distance Y (units): distance between each cube on the y-axis
    o	My Cube (prefab): a prefab of a cube object that will be instantiated at runtime
    o	Num Rows (#): number of rows
    o	Num Columns (#): number of columns
    o	On Colour (Colour): Colour of cube object when flashed
    o	Off Colour (Colour): Colour of cube object when not flashing

    Inputs:
    o   'S' key: Single Flash Start/Stop
    o   'R' key: Row/Column Flash Start/Stop
    o   'D' key: Redraw Matrix
    o   'Q' key: Quit Program

    NOTE:
    o   Must press Q before closing the application, this will ensure that the LSL Outlet is properly destroyed
    
 */
public class P300_Flashes : MonoBehaviour
{
    /* Public Variables */
    public int refreshRate;
    public float freqHz;
    public float flashLength;
    public int numSamples;
    public double startX;
    public double startY;
    public float startZ;
    public double distanceX;
    public double distanceY;
    public GameObject myCube;
    public Resolution[] resol;
    public int numRows;
    public int numColumns;
    public Color onColour;
    public Color offColour;
    public int TargetCube; //This is new and determines what our target is.
    public bool SendLiveInfo; //This determines whether or not to send Live Information to other systems.

    //Variables for the Boxes
    /* Grid is mapped out as follows:

        c00     c10     c20

        c01     c11     c21

        c02     c12     c22

     */

    /* Variables shared with LSL Inlet (to be accessed to flash correct cube) */
    public List<GameObject> cube_list = new List<GameObject>();

    /* Private Variables */
    public bool startFlashes;
    private List<int> row_counter = new List<int>();
    private List<int> column_counter = new List<int>();
    private List<int> flash_counter = new List<int>();
    //To address the lag at the end of the trials for CR and Single
    private List<int> c_indexes = new List<int>();
    private List<int> r_indexes = new List<int>();
    private List<int> s_indexes = new List<int>();
    private int numTrials;


    private int counter = 0;
    private int rowFlashes;
    private int columnFlashes;
    private GameObject[,] cube_matrix;
    private int matrixCounter = 0;
    private int s_trials;
    private int rc_trials;
    private Dictionary<KeyCode, bool> keyLocks = new Dictionary<KeyCode, bool>();
    //Parent GameObjects to store dynamically created GameObjects
    private GameObject cubes;


    //Variables used for checking redraw
    private double current_startx;
    private double current_starty;
    private float current_startz;
    private double current_dx;
    private double current_dy;
    private int current_numrow;
    private int current_numcol;
    private GameObject current_cube;
    private bool locked_keys = false;

    /* LSL Variables */
    private LSLMarkerStream marker;
    private Inlet_P300 inletP300;

    // Start is called before the first frame update
    void Start()
    {
        //Get the screen refresh rate, so that the colours can be set appropriately
        resol = Screen.resolutions;
        refreshRate = resol[3].refreshRate;
        //Set up LSL Marker Stream
        marker = FindObjectOfType<LSLMarkerStream>();
        inletP300 = FindObjectOfType<Inlet_P300>();

        //Setting up Keys, to lock other keys when one simulation is being run
        keyLocks.Add(KeyCode.R, false);
        keyLocks.Add(KeyCode.S, false);
        keyLocks.Add(KeyCode.D, false);
        locked_keys = false;
        SendLiveInfo = false;

        //Check to see if inputs are valid, if not, then don't draw matrix and prompt user to redraw with the
        //correct inputs
        if(CheckEmpty()){
            print("Values must be non-zero and non-negative, please re-enter values and press 'D' to redraw...");
            locked_keys = true;
            return;
        }
        //Initialize Matrix
        SetUpMatrix();
        SetUpSingle();
        SetUpRC();

        SaveCurrentInfo();
        //Set the colour of the box to the given offColour
        TurnOff();
        System.Threading.Thread.Sleep(2000);
        SendInfo();

        

    }

    // Update is called once per frame
    void Update()
    {
        //NEW: Search for updated LSL Streams
        //Resolve new streams - Added EKL
        if (Input.GetKeyDown(KeyCode.F4))
        {
            inletP300.ResolveOnRequest();
        }

        //Row/Column Flash P300
        //Conditional to check key locks
        if (Input.GetKeyDown(KeyCode.R) && keyLocks[KeyCode.S] == false && keyLocks[KeyCode.D] == false && !locked_keys){
            keyLocks[KeyCode.R] = !keyLocks[KeyCode.R];
            startFlashes = !startFlashes;

            if(startFlashes){
                print("Starting P300 RCFlash");
                if (SendLiveInfo == true)
                {
                    //Send update on current information to LSL - This is new!
                    SendCurrentInfo();
                }
                marker.Write("P300 RCFlash Begins");
                SetUpRC();
                StartCoroutine("RCFlash");
            } else {
                // Selected if the user pauses the simulation before it is complete
                print("Stopping P300 RCFlash");
                marker.Write("P300 RCFlash Ends");
                StopCoroutine("RCFlash");
                ResetCounters();
                print("Counters Reset! Hit G again to run P300 RCFlash");
            }


        }

        //Single Flash P300
        //Conditional to check key locks
        if(Input.GetKeyDown(KeyCode.S) && keyLocks[KeyCode.R] == false && keyLocks[KeyCode.D] == false && !locked_keys){
            keyLocks[KeyCode.S] = !keyLocks[KeyCode.S];
            startFlashes = !startFlashes;

            if(startFlashes){
                
                print("Starting P300 SingleFlash");
                //Send update on current information to LSL - This is new!
                if (SendLiveInfo == true)
                {
                    //Send update on current information to LSL - This is new!
                    SendCurrentInfo();
                }
                //Writing to LSL to signal start of simulation
                marker.Write("P300 SingleFlash Begins");
                SetUpSingle();
                StartCoroutine("SingleFlash");
            } else {
                // Selected if the user pauses the simulation before it is complete
                print("Stopping P300 SingleFlash");
                //Writing to LSL to signal end of simulation
                marker.Write("P300 SingleFlash Ends");
                StopCoroutine("SingleFlash");
                ResetCounters();
                print("Counters Reset! Hit S again to run P300 SingleFlash");
            }

        }

        //Redraw Matrix
        //Select this after changing parameters
        if(Input.GetKeyDown(KeyCode.D)  && keyLocks[KeyCode.R] == false && keyLocks[KeyCode.S] == false){
            //Check if values are empty
            if(CheckEmpty()){
                print("Values must be non-zero and non-negative, please re-enter values and try again...");
                locked_keys = true;
                return;
            }
            keyLocks[KeyCode.D] = true;
            print("Redrawing Matrix");
            TurnOff();
            DestroyMatrix();
            ResetCounters();
            cube_list.Clear();
            SetUpMatrix();
            SetUpSingle();
            SetUpRC();
            TurnOff();
            keyLocks[KeyCode.D] = false;
            locked_keys = false;
        }

        //Quit Program 
        if(Input.GetKeyDown(KeyCode.Q)){
            print("Quitting Program...");
            marker.Write("Quit");
            marker = null;
            Application.Quit();
        }
    }

    /* Single Flash Operation */
    IEnumerator SingleFlash(){
        int prevCube = 100000;
        while(startFlashes){
            int randomCube;
            int randomIndex;
            //Generate a random number from the list of indices that have non-zero counters
            System.Random random = new System.Random();
            randomIndex = random.Next(s_indexes.Count);
            randomCube = s_indexes[randomIndex];

            //Sanity check to make sure you don't get 2 flashes back to back of the same cube!
            if (randomCube == prevCube && s_indexes.Count>2)
            {
                while (randomCube == prevCube)
                {
                    randomIndex = random.Next(s_indexes.Count);
                    randomCube = s_indexes[randomIndex];
                }

            }

            //Turn off the cubes to give the flashing image
            TurnOff();

            //If the counter is non-zero, then flash that cube and decrement the flash counter
            if(flash_counter[randomCube] > 0){
                yield return new WaitForSecondsRealtime((1f/freqHz)-flashLength);

                cube_list[randomCube].GetComponent<Renderer>().material.color = onColour;

                //Handle events if this is the target cube or not //NEW!
                if(randomCube == TargetCube)
                {
                    OnTargetFlash();
                }
                else
                {
                    OnNonTargetFlash();
                }

                flash_counter[randomCube]--;
                counter++;
                print("CUBE: " + randomCube.ToString());

                //Write to the LSL Outlet stream
               marker.Write("s," + randomCube.ToString());
            } else if(numTrials == counter){
                print("Done P300 Single Flash Trials");
                break;
            } else {
                //If the counter for a specific cube has reached zero, then remove it from the indexes so that the random
                //number generator does not pick it again (to reduce lag)
                if(flash_counter[randomCube] == 0){
                    s_indexes.RemoveAt(randomIndex);
                }
                //Go to the next iteration of the single flash 
                continue;
            }
            prevCube = randomCube;
            yield return new WaitForSecondsRealtime(flashLength);

        }
        ResetCounters();
        //Write to LSL stream to indicate end of P300 SingleFlash
        marker.Write("P300 SingleFlash Ends");
        
        startFlashes = !startFlashes;
        keyLocks[KeyCode.S] = !keyLocks[KeyCode.S];
    }

    /* RC Flash Operation */
    IEnumerator RCFlash(){
        while(startFlashes){
            int rcI;
            int crIndex = 0;
            int crFlip;
            System.Random random = new System.Random();
  
            //Generates a random number to determine if a column or row should be flashed
            //Rows = 1, Columns = 2
            crFlip = random.Next(1,3);

            //Generate a random number from a list of indices that have no-zero counters
            if((crFlip == 1 && r_indexes.Count != 0) || (crFlip == 2 && c_indexes.Count == 0)) {
                crFlip = 1;
                rcI = random.Next(r_indexes.Count);
                crIndex = r_indexes[rcI];
            } else if((crFlip == 2 && c_indexes.Count != 0) || (crFlip == 1 && r_indexes.Count == 0)) {
                crFlip = 2;
                rcI = random.Next(c_indexes.Count);
                crIndex = c_indexes[rcI];
            } else {
                continue;
            }

            //Turn off the grid and wait to display a flashing pattern
            TurnOff();
            //Flash Row if counter for that row is non-zero, then decrement counter
            if(crFlip == 1 && row_counter[crIndex] > 0){
                yield return new WaitForSecondsRealtime((1f/freqHz)-flashLength);

                FlashRow(crIndex);
                row_counter[crIndex]--;
                counter++;

            //Flash Column if counter for that column is non-zero, then decremement counter
            } else if(crFlip == 2 && column_counter[crIndex] > 0){
                yield return new WaitForSecondsRealtime((1f/freqHz) - flashLength);

                FlashColumn(crIndex);
                column_counter[crIndex]--;
                counter++;

            } else if(numTrials == counter){
                print("Done P300 RCFlash Trials");
                break;
            } else {
                //If the counter for a specific row and column has reached zero, then remove it from the indexes so that the random
                //number generator does not pick it again (to reduce lag)
                if(crFlip == 1){
                    if(row_counter[crIndex] == 0){
                        r_indexes.RemoveAt(rcI);
                    }
                } else {
                    if(column_counter[crIndex] == 0){
                        c_indexes.RemoveAt(rcI);
                    }
                }
                continue;
            }
            yield return new WaitForSeconds(flashLength);
        }
        ResetCounters();
        //Send to LSL stream to indicate end of P300 RCFlash
        marker.Write("P300 RCFlash Ends");
        startFlashes = !startFlashes;
        keyLocks[KeyCode.R] = !keyLocks[KeyCode.R];
    }

   /* Sets all cubes to the offColour */
    public void TurnOff(){
        for(int i = 0; i < cube_list.Count; i++){
            cube_list[i].GetComponent<Renderer>().material.color = offColour;
        }
    }

    /* Configures arrays and simulation values for Row/Column flashes */
    public void SetUpRC(){
        //Check if number of cell flashes is even or odd and adjust the row/column flashes accordingly
        if(numSamples % 2 == 0){
            //Calculate number of trials total
            numTrials = (numRows + numColumns) * (numSamples / 2);
            rc_trials = numTrials;

            rowFlashes = numSamples / 2;
            columnFlashes = numSamples / 2;
        } else {
            double num = (double) numSamples / 2;
            rowFlashes = (int) Math.Ceiling(num);
            columnFlashes = (int) Math.Ceiling(num) - 1;

            numTrials = (rowFlashes * numRows) + (columnFlashes * numColumns);
            rc_trials = numTrials;
        }

        //Set up the counters
        for(int i = 0; i < numRows; i++){
            row_counter.Add(rowFlashes);
            r_indexes.Add(i);
        }
        for(int i = 0; i < numColumns; i++){
            column_counter.Add(columnFlashes);
            c_indexes.Add(i);
        }

        print("---------- RC FLASH DETAILS ----------");
        print("Number of Trials will be: " + numTrials);
        print("Number of flashes for each cell: " + numSamples);
        print("Number of flashes for each row: " + rowFlashes);
        print("Number of flashes for each column: " + columnFlashes);
        print("--------------------------------------");
    }

    /* Configures array and simulation values for Single flashes */
    public void SetUpSingle(){
        /*
                Grid                  Number
        c00     c10     c20         0   1   2

        c01     c11     c21     ->  3   4   5

        c02     c12     c22         6   7   8
        */

        //Setting counters for each cube
        for(int i = 0; i < numRows * numColumns; i++){
            flash_counter.Add(numSamples);
        }

        numTrials = numSamples * (numRows * numColumns);
        s_trials = numTrials;

        //Set up test single indices
        for(int i = 0; i < (numRows * numColumns); i++){
            s_indexes.Add(i);
        }

        print("---------- SINGLE FLASH DETAILS ----------");
        print("Number of Trials will be: " + numTrials);
        print("Number of flashes for each cell: " + numSamples);
        print("--------------------------------------");
    }

    /* Find row in matrix and set the cubes in that row to onColour
        Send the indices of the row to LSL */
    public void FlashRow(int rowIndex){
        string row = "r";
        for(int x = 0; x < numColumns; x++){
            cube_matrix[x, rowIndex].GetComponent<Renderer>().material.color = onColour;
            row += "," + (cube_matrix[x,rowIndex].name).Substring(4);
        }
        print("ROW: " + row);
        marker.Write(row);
    }

    /* Find column in matrix and set the cubes in that column to onColour 
        Send the indices of the column to LSL */
    public void FlashColumn(int columnIndex){
        string column = "c";
        for(int y = 0; y < numRows; y++){
            cube_matrix[columnIndex, y].GetComponent<Renderer>().material.color = onColour;
            column += "," + (cube_matrix[columnIndex, y].name).Substring(4);
        }
        print("COLUMN: " + column);
        marker.Write(column);
    }

    /* Configure matrix and display this on the screen */
    public void SetUpMatrix(){
        /*Reference Matrix and thought process:

            0   1   2
            3   4   5
            6   7   8
        
            C0 = 0, 3, 6
            C1 = 1, 4, 7
            C2 = 2, 5, 8

            R0 = 0, 1, 2
            R1 = 3, 4, 5
            R2 = 6, 7, 8

        */

        //Initial set up
        cube_matrix = new GameObject[numColumns, numRows];
        cubes = new GameObject();
        cubes.name = "Cubes";
        
        /* Dynamic Matrix Setup */
        int cube_counter = 0;
        for(int y = numRows-1; y > -1; y--){
            for(int x = 0; x < numColumns; x++){
                //Instantiating prefabs
                GameObject new_cube = Instantiate(myCube);


                //Renaming objects
                new_cube.name = "cube" + cube_counter.ToString();

                //Adding to list
                cube_list.Add(new_cube);

                //Adding to Parent GameObject
                new_cube.transform.parent = cubes.transform;

                //Setting position of cube
                new_cube.transform.position = new Vector3((float)((x+startX)*distanceX), (float)((y+startY)*distanceY), startZ);

                //Activating objects
                new_cube.SetActive(true);
                cube_counter++;
            }
        }

        //Position Camera to the centre of the cubes
        float cameraX = (float) ((((cube_list[numColumns - 1].transform.position.x) - (cube_list[0].transform.position.x))/2) + (startX * 2));
        float cameraY = (float) ((((cube_list[0].transform.position.y) - (cube_list[cube_counter-1].transform.position.y))/2) + (startY * 2));
        float cameraSize;
        if(numRows > numColumns){
            cameraSize = numRows;
        } else {
            cameraSize = numColumns;
        }

        
        GameObject.Find("Main Camera").transform.position = new Vector3(cameraX, cameraY, -10f + startZ);
        GameObject.Find("Main Camera").GetComponent<Camera>().orthographicSize = cameraSize;
        print("Camera Position: X: " + (cameraX) + " Y: " +  (cameraY) + " Z: " + -10f);

        //Setting up cube matrix to be used during RC Flashes
        for(int y = 0; y < numRows; y++){
            for(int x = 0; x < numColumns; x++){
                cube_matrix[x,y] = cube_list[matrixCounter];
                matrixCounter++;
            }
        }
    }

    /* Resets all counters and clear arrays */
    public void ResetCounters(){
        counter = 0;
        matrixCounter = 0;
        row_counter.Clear();
        column_counter.Clear();
        flash_counter.Clear();
        s_indexes.Clear();
        r_indexes.Clear();
        c_indexes.Clear();
    }

    /* Destroy GameObjects */
    public void DestroyMatrix(){
        
        //Destroy Parent Objects
        Destroy(cubes);

    }

    /* Write information to LSL 
        Used in inital MATLAB configurations */
    public void SendInfo(){
        marker.Write(numRows.ToString());
        marker.Write(numColumns.ToString());
        marker.Write(numSamples.ToString());
        marker.Write(s_trials.ToString());
        marker.Write(rc_trials.ToString());
    }
    
    //Send Current information about the P300 setup
    public void SendCurrentInfo()
    {
        marker.Write("Current rows : " + numRows.ToString());
        marker.Write("Current cols: " + numColumns.ToString());
        marker.Write("Target Object: " + TargetCube.ToString());
    }

    /* Run everytime a value in the Unity editor changes, ensures that the user cannot run a simulation if critical values have been changed */
    public void OnValidate(){

        if(myCube != current_cube || distanceX != current_dx || distanceY != current_dy || numRows != current_numrow || numColumns != current_numcol || startX != current_startx || startY != current_starty || startZ != current_startz){
            if(myCube != null && distanceX != 0 && distanceY != 0 && numRows != 0 && numColumns != 0){
                print("Matrix Configuration Values have been changed, please press 'D' to redraw the matrix...");
                SaveCurrentInfo();
            }
            locked_keys = true;
        }
    }

    /* Save current states into variables for OnValidate to check */
    public void SaveCurrentInfo(){
        current_cube = myCube;
        current_startx = startX;
        current_starty = startY;
        current_startz = startZ;
        current_dx = distanceX;
        current_dy = distanceY;
        current_numrow = numRows;
        current_numcol = numColumns;
    }

    /* Checks to see if given values are valid */    
    public bool CheckEmpty(){
        if(myCube == null || distanceX <= 0 || distanceY <= 0 || numRows <= 0 || numColumns <= 0){
            return true;
        } else {
            return false;
        }
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