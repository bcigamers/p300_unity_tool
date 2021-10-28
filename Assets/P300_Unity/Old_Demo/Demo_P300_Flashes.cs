using System.Net.Mime;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Assets.LSL4Unity.Scripts;
using Assets.LSL4Unity;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
//using UnityEngine.Windows.Speech;

/*
P300 Flashes Demo Program for HoloLens
Author: Shaheed Murji
Adapted from: Eli Kinney-Lang "Matching_Demo/FlashScript.cs"


    Parameters: 
    o	Freq Hz (Hz): this is how frequent the cubes will be flashing
    o	Flash Length (s): how long a cube will be on for during a flash
    o	Num Samples (#): number of times a single cube will flash in a trial
    o	Distance X (units): distance between each cube on the x-axis
    o	Distance Y (units): distance between each cube on the y-axis
    o	My Cube (prefab): a prefab of a cube object that will be instantiated at runtime
    o	My Light (prefab): a prefab of a light object that will be instantiated at runtime
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
public class Demo_P300_Flashes : MonoBehaviour
{
    /* Public Variables */
    public int refreshRate;
    public float freqHz;
    public float flashLength;
    public int numSamples;
    public Resolution[] resol;
    public int numRows;
    public int numColumns;

    /* Public Variables specific to this program */
    public GameObject hud;
    public Sprite onSprite;
    public List<Sprite> default_images = new List<Sprite>();   // Specific to this program

    /* Unused public variables for Flashing Controller in HoloLens */
    // public double startX;
    // public double startY;
    // public float startZ;
    // public double distanceX;
    // public double distanceY;
    // public GameObject myCube;
    // public Light myLight;
    // public Color onColour;
    // public Color offColour;


    //Variables for the Boxes
    /* Grid is mapped out as follows:

        c00     c10     c20

        c01     c11     c21

        c02     c12     c22

     */

    /* Variables shared with LSL Inlet (to be accessed to flash correct cube) */
    public List<GameObject> cube_list = new List<GameObject>();
    // public List<Light> light_list = new List<Light>();  // Lights are not being used in the Flashing Controller

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
    private int s_trials;
    private int rc_trials;
    private Dictionary<KeyCode, bool> keyLocks = new Dictionary<KeyCode, bool>();

    private bool locked_keys = false;

    /* Speech Recognizer Variables */
    //KeywordRecognizer keywordRecognizer = null;
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    /* Unused Private Variables for Flashing Controller in Hololens */

    //private Light[,] light_matrix;
    //private List<GameObject> light_objects = new List<GameObject>();
    //private int matrixCounter = 0;

    //Parent GameObjects to store dynamically created GameObjects
    // private GameObject cubes;
    // private GameObject lights;

    //Variables used for checking redraw
    // private double current_startx;
    // private double current_starty;
    // private float current_startz;
    // private double current_dx;
    // private double current_dy;
    // private int current_numrow;
    // private int current_numcol;
    // private GameObject current_cube
    // private Light current_light;

    /* LSL Variables */
    private LSLMarkerStream marker;

    // Start is called before the first frame update
    void Start()
    {
        //Get the screen refresh rate, so that the colours can be set appropriately
        resol = Screen.resolutions;
        refreshRate = resol[3].refreshRate;
        //Set up LSL Marker Stream
        marker = FindObjectOfType<LSLMarkerStream>();

        //Setting up Keys, to lock other keys when one simulation is being run
        keyLocks.Add(KeyCode.R, false);
        keyLocks.Add(KeyCode.S, false);
        //keyLocks.Add(KeyCode.D, false); //Removing Key 'D' since redraw matrix is meant for dynamically created matrices
        locked_keys = false;

        /* Speech Recognizer Variables */
        keywords.Add("Start Flash", () =>
        {
            //Copied code from SingleFlash
            keyLocks[KeyCode.S] = !keyLocks[KeyCode.S];
            startFlashes = !startFlashes;

            if (startFlashes)
            {
                print("Starting P300 SingleFlash");
                //Writing to LSL to signal start of simulation
                marker.Write("P300 SingleFlash Begins");
                SetUpSingle();
                StartCoroutine("SingleFlash");
            }
            else
            {
                // Selected if the user pauses the simulation before it is complete
                print("Stopping P300 SingleFlash");
                //Writing to LSL to signal end of simulation
                marker.Write("P300 SingleFlash Ends");
                StopCoroutine("SingleFlash");
                ResetCounters();
                print("Counters Reset! Hit S again to run P300 SingleFlash");
            }
        });

        // Tell the KeywordRecognizer about our keywords
        //keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());

        ////Register a callback for the KeywordRecognizer and start recognizing 
        //keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        //keywordRecognizer.Start();


        /* Unused Functionality, since cube matrix is not being created dynamically, there are no matrix reconfigurations to be made */
        //Check to see if inputs are valid, if not, then don't draw matrix and prompt user to redraw with the
        //correct inputs
        // if(CheckEmpty()){
        //     print("Values must be non-zero and non-negative, please re-enter values and press 'D' to redraw...");
        //     locked_keys = true;
        //     return;
        // }

        //Initialize Matrix
        HUD_Setup(); //Function specific to this program
                     //SetUpMatrix();
        SetUpSingle();
        SetUpRC();

        // SaveCurrentInfo();
        //Set the colour of the box to the given offColour
        TurnOff();
        System.Threading.Thread.Sleep(2000);
        SendInfo();

        /* Testing purposes */
        startFlashes = !startFlashes;
        keyLocks[KeyCode.S] = !keyLocks[KeyCode.S];
        print("Starting P300 SingleFlash");
        marker.Write("P300 SingleFlash Begins");
        SetUpSingle();
        StartCoroutine("SingleFlash");




    }

    // Update is called once per frame
    void Update()
    {
        //Row/Column Flash P300
        //Conditional to check key locks
        // if(Input.GetKeyDown(KeyCode.R) && keyLocks[KeyCode.S] == false && keyLocks[KeyCode.D] == false && !locked_keys){
        if (Input.GetKeyDown(KeyCode.R) && keyLocks[KeyCode.S] == false && !locked_keys)
        {

            keyLocks[KeyCode.R] = !keyLocks[KeyCode.R];
            startFlashes = !startFlashes;

            if (startFlashes)
            {
                print("Starting P300 RCFlash");
                marker.Write("P300 RCFlash Begins");
                SetUpRC();
                StartCoroutine("RCFlash");
            }
            else
            {
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
        // if(Input.GetKeyDown(KeyCode.S) && keyLocks[KeyCode.R] == false && keyLocks[KeyCode.D] == false && !locked_keys){
        if (Input.GetKeyDown(KeyCode.S) && keyLocks[KeyCode.R] == false && !locked_keys)
        {

            keyLocks[KeyCode.S] = !keyLocks[KeyCode.S];
            startFlashes = !startFlashes;

            if (startFlashes)
            {
                print("Starting P300 SingleFlash");
                //Writing to LSL to signal start of simulation
                marker.Write("P300 SingleFlash Begins");
                SetUpSingle();
                StartCoroutine("SingleFlash");
            }
            else
            {
                // Selected if the user pauses the simulation before it is complete
                print("Stopping P300 SingleFlash");
                //Writing to LSL to signal end of simulation
                marker.Write("P300 SingleFlash Ends");
                StopCoroutine("SingleFlash");
                ResetCounters();
                print("Counters Reset! Hit S again to run P300 SingleFlash");
            }

        }

        /* Removed Functionality Redraw Matrix */
        //Select this after changing parameters
        // if(Input.GetKeyDown(KeyCode.D)  && keyLocks[KeyCode.R] == false && keyLocks[KeyCode.S] == false){
        //     //Check if values are empty
        //     // if(CheckEmpty()){
        //     //     print("Values must be non-zero and non-negative, please re-enter values and try again...");
        //     //     locked_keys = true;
        //     //     return;
        //     // }
        //     keyLocks[KeyCode.D] = true;
        //     print("Redrawing Matrix");
        //     TurnOff();
        //     // DestroyMatrix();
        //     ResetCounters();
        //     cube_list.Clear();
        //     // light_list.Clear();
        //     // SetUpMatrix();
        //    //HUD_Setup();
        //     SetUpSingle();
        //     SetUpRC();
        //     TurnOff();
        //     keyLocks[KeyCode.D] = false;
        //     locked_keys = false;
        // }

        //Quit Program 
        if (Input.GetKeyDown(KeyCode.Q))
        {
            print("Quitting Program...");
            marker.Write("Quit");
            marker = null;
            Application.Quit();
        }
    }

    /* Single Flash Operation */
    IEnumerator SingleFlash()
    {
        while (startFlashes)
        {
            int randomCube;
            int randomIndex;
            //Generate a random number from the list of indices that have non-zero counters
            System.Random random = new System.Random();
            randomIndex = random.Next(s_indexes.Count);
            randomCube = s_indexes[randomIndex];

            //Turn off the cubes to give the flashing image
            TurnOff();

            //If the counter is non-zero, then flash that cube and decrement the flash counter
            if (flash_counter[randomCube] > 0)
            {
                yield return new WaitForSecondsRealtime((1f / freqHz));

                /* Dynamic cubes relying on colours */
                //cube_list[randomCube].GetComponent<Image>().color = onColour;
                // light_list[randomCube].enabled = true;

                //Changing the image to an onSprite image defined by the user
                cube_list[randomCube].GetComponent<Image>().sprite = onSprite;

                flash_counter[randomCube]--;
                counter++;
                print("CUBE: " + randomCube.ToString());

                //Write to the LSL Outlet stream
                marker.Write("s," + randomCube.ToString());
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

            yield return new WaitForSecondsRealtime(flashLength);

        }
        ResetCounters();
        //Write to LSL stream to indicate end of P300 SingleFlash
        marker.Write("P300 SingleFlash Ends");
        startFlashes = !startFlashes;
        keyLocks[KeyCode.S] = !keyLocks[KeyCode.S];
    }

    /* RC Flash Operation */
    IEnumerator RCFlash()
    {
        while (startFlashes)
        {
            int rcI;
            int crIndex = 0;
            int crFlip;
            System.Random random = new System.Random();

            //Generates a random number to determine if a column or row should be flashed
            //Rows = 1, Columns = 2
            crFlip = random.Next(1, 3);

            //Generate a random number from a list of indices that have no-zero counters
            if ((crFlip == 1 && r_indexes.Count != 0) || (crFlip == 2 && c_indexes.Count == 0))
            {
                crFlip = 1;
                rcI = random.Next(r_indexes.Count);
                crIndex = r_indexes[rcI];
            }
            else if ((crFlip == 2 && c_indexes.Count != 0) || (crFlip == 1 && r_indexes.Count == 0))
            {
                crFlip = 2;
                rcI = random.Next(c_indexes.Count);
                crIndex = c_indexes[rcI];
            }
            else
            {
                continue;
            }

            //Turn off the grid and wait to display a flashing pattern
            TurnOff();
            //Flash Row if counter for that row is non-zero, then decrement counter
            if (crFlip == 1 && row_counter[crIndex] > 0)
            {
                yield return new WaitForSecondsRealtime((1f / freqHz));

                FlashRow(crIndex);
                row_counter[crIndex]--;
                counter++;

                //Flash Column if counter for that column is non-zero, then decremement counter
            }
            else if (crFlip == 2 && column_counter[crIndex] > 0)
            {
                yield return new WaitForSecondsRealtime((1f / freqHz));

                FlashColumn(crIndex);
                column_counter[crIndex]--;
                counter++;

            }
            else if (numTrials == counter)
            {
                print("Done P300 RCFlash Trials");
                break;
            }
            else
            {
                //If the counter for a specific row and column has reached zero, then remove it from the indexes so that the random
                //number generator does not pick it again (to reduce lag)
                if (crFlip == 1)
                {
                    if (row_counter[crIndex] == 0)
                    {
                        r_indexes.RemoveAt(rcI);
                    }
                }
                else
                {
                    if (column_counter[crIndex] == 0)
                    {
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

    /* Sets all cubes to the default Sprites*/
    public void TurnOff()
    {
        for (int i = 0; i < cube_list.Count; i++)
        {

            //Change the sprite to the default image preset
            cube_list[i].GetComponent<Image>().sprite = default_images[i];

            /* Unused functionality */
            //cube_list[i].GetComponent<Image>().color = offColour;
            // light_list[i].enabled = false;
        }
    }

    /* Setting up Matrix from HUD */
    public void HUD_Setup()
    {
        print("HUD has " + hud.transform.childCount + " children");

        //Create a list from the children
        for (int i = 0; i < hud.transform.childCount; i++)
        {
            GameObject child = hud.transform.GetChild(i).gameObject;
            child.name = "cube" + i;

            cube_list.Add(child);
            default_images.Add(child.GetComponent<Image>().sprite);
        }

        cube_matrix = new GameObject[numColumns, numRows];
        int childcounter = 0;
        //Assign cubes to the matrix
        for (int y = 0; y < numRows; y++)
        {
            for (int x = 0; x < numColumns; x++)
            {
                cube_matrix[x, y] = cube_list[childcounter];
                childcounter++;
            }
        }

    }
    /* Configures arrays and simulation values for Row/Column flashes */
    public void SetUpRC()
    {
        //Check if number of cell flashes is even or odd and adjust the row/column flashes accordingly
        if (numSamples % 2 == 0)
        {
            //Calculate number of trials total
            numTrials = (numRows + numColumns) * (numSamples / 2);
            rc_trials = numTrials;

            rowFlashes = numSamples / 2;
            columnFlashes = numSamples / 2;
        }
        else
        {
            double num = (double)numSamples / 2;
            rowFlashes = (int)Math.Ceiling(num);
            columnFlashes = (int)Math.Ceiling(num) - 1;

            numTrials = (rowFlashes * numRows) + (columnFlashes * numColumns);
            rc_trials = numTrials;
        }

        //Set up the counters
        for (int i = 0; i < numRows; i++)
        {
            row_counter.Add(rowFlashes);
            r_indexes.Add(i);
        }
        for (int i = 0; i < numColumns; i++)
        {
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
    public void SetUpSingle()
    {
        /*
                Grid                  Number
        c00     c10     c20         0   1   2

        c01     c11     c21     ->  3   4   5

        c02     c12     c22         6   7   8
        */

        //Setting counters for each cube
        for (int i = 0; i < numRows * numColumns; i++)
        {
            flash_counter.Add(numSamples);
        }

        numTrials = numSamples * (numRows * numColumns);
        s_trials = numTrials;

        //Set up test single indices
        for (int i = 0; i < (numRows * numColumns); i++)
        {
            s_indexes.Add(i);
        }

        print("---------- SINGLE FLASH DETAILS ----------");
        print("Number of Trials will be: " + numTrials);
        print("Number of flashes for each cell: " + numSamples);
        print("--------------------------------------");
    }

    /* Find row in matrix and set the cubes in that row to onColour
        Send the indices of the row to LSL */
    public void FlashRow(int rowIndex)
    {
        string row = "r";
        for (int x = 0; x < numColumns; x++)
        {
            /* Unused Functionality */
            //cube_matrix[x, rowIndex].GetComponent<Image>().color = onColour;
            //light_matrix[x, rowIndex].enabled = true;
            cube_matrix[x, rowIndex].GetComponent<Image>().sprite = onSprite;

            row += "," + (cube_matrix[x, rowIndex].name).Substring(4);
        }
        print("ROW: " + row);
        marker.Write(row);
    }

    /* Find column in matrix and set the cubes in that column to onColour 
        Send the indices of the column to LSL */
    public void FlashColumn(int columnIndex)
    {
        string column = "c";
        for (int y = 0; y < numRows; y++)
        {
            /* Unused Functionality */
            //cube_matrix[columnIndex, y].GetComponent<Image>().color = onColour;
            //light_matrix[columnIndex, y].enabled = true;
            cube_matrix[columnIndex, y].GetComponent<Image>().sprite = onSprite;

            column += "," + (cube_matrix[columnIndex, y].name).Substring(4);
        }
        print("COLUMN: " + column);
        marker.Write(column);
    }
    /* Unused Functionality, this is meant for dynamically created matrices and is not used in the Flashing Controller for HoloLens */
    /* Configure matrix and display this on the screen */
    // public void SetUpMatrix(){
    //     /*Reference Matrix and thought process:

    //         0   1   2
    //         3   4   5
    //         6   7   8

    //         C0 = 0, 3, 6
    //         C1 = 1, 4, 7
    //         C2 = 2, 5, 8

    //         R0 = 0, 1, 2
    //         R1 = 3, 4, 5
    //         R2 = 6, 7, 8

    //     */

    //     //Initial set up
    //     cube_matrix = new GameObject[numColumns, numRows];
    //     //light_matrix = new Light[numColumns, numRows];
    //     cubes = new GameObject();
    //     cubes.name = "Cubes";
    //     //lights = new GameObject();
    //     //lights.name = "Lights";

    //     /* Dynamic Matrix Setup */
    //     int cube_counter = 0;
    //     for(int y = numRows-1; y > -1; y--){
    //         for(int x = 0; x < numColumns; x++){
    //             //Instantiating prefabs
    //             GameObject new_cube = Instantiate(myCube);
    //             //GameObject light_object = new GameObject();
    //             //Light new_light = light_object.AddComponent<Light>();

    //             //Renaming objects
    //             new_cube.name = "cube" + cube_counter.ToString();
    //             //new_light.name = "light" + cube_counter.ToString();

    //             //Adding to list
    //             cube_list.Add(new_cube);
    //             // light_list.Add(new_light);
    //             // light_objects.Add(light_object);

    //             //Adding to Parent GameObject
    //             new_cube.transform.parent = cubes.transform;
    //             //light_object.transform.parent = lights.transform;

    //             //Setting position of cube and light
    //             new_cube.transform.position = new Vector3((float)((x+startX)*distanceX), (float)((y+startY)*distanceY), startZ);
    //             //light_object.transform.position = new Vector3((float)((x+startX)*distanceX), (float)((y+startY)*distanceY), -5 + startZ);

    //             //Activating objects
    //             new_cube.SetActive(true);
    //             //light_object.SetActive(true);
    //             //new_light.enabled = false;
    //             cube_counter++;
    //         }
    //     }

    //     //Position Camera to the centre of the cubes
    //     float cameraX = (float) ((((cube_list[numColumns - 1].transform.position.x) - (cube_list[0].transform.position.x))/2) + (startX * 2));
    //     float cameraY = (float) ((((cube_list[0].transform.position.y) - (cube_list[cube_counter-1].transform.position.y))/2) + (startY * 2));
    //     float cameraSize;
    //     if(numRows > numColumns){
    //         cameraSize = numRows;
    //     } else {
    //         cameraSize = numColumns;
    //     }


    //     GameObject.Find("Main Camera").transform.position = new Vector3(cameraX, cameraY, -10f + startZ);
    //     GameObject.Find("Main Camera").GetComponent<Camera>().orthographicSize = cameraSize;
    //     print("Camera Position: X: " + (cameraX) + " Y: " +  (cameraY) + " Z: " + -10f);

    //     //Setting up cube and light matrix to be used during RC Flashes
    //     for(int y = 0; y < numRows; y++){
    //         for(int x = 0; x < numColumns; x++){
    //             cube_matrix[x,y] = cube_list[matrixCounter];
    //             //light_matrix[x,y] = light_list[matrixCounter];
    //             matrixCounter++;
    //         }
    //     }
    // }

    /* Resets all counters and clear arrays */
    public void ResetCounters()
    {
        counter = 0;
        //matrixCounter = 0;
        row_counter.Clear();
        column_counter.Clear();
        flash_counter.Clear();
        s_indexes.Clear();
        r_indexes.Clear();
        c_indexes.Clear();
    }
    /* Unused Functionality, no need to destroy the matrix since it is not dynamically created */
    /* Destroy GameObjects */
    // public void DestroyMatrix(){
    //     light_list.Clear();

    //     //Destroy Parent Objects
    //     Destroy(cubes);
    //     Destroy(lights);

    //     /* Below is no longer needed since the parent objects are being destroyed, so the children objects also get destroyed */
    //     // foreach(GameObject cube in cube_list){
    //     //     Destroy(cube);
    //     // }
    //     // foreach(GameObject light in light_objects){
    //     //     Destroy(light);
    //     // }
    // }

    /* Write information to LSL 
        Used in inital MATLAB configurations */
    public void SendInfo()
    {
        marker.Write(numRows.ToString());
        marker.Write(numColumns.ToString());
        marker.Write(numSamples.ToString());
        marker.Write(s_trials.ToString());
        marker.Write(rc_trials.ToString());
    }

    //private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    //{
    //    System.Action keywordAction;
    //    if (keywords.TryGetValue(args.text, out keywordAction))
    //    {
    //        keywordAction.Invoke();
    //    }
    //}

    /* Unused Functionality, Editor is no longer being used to change the dimensions of the matrix, thus no checking needs to be done */
    /* Run everytime a value in the Unity editor changes, ensures that the user cannot run a simulation if critical values have been changed */
    // public void OnValidate(){

    //     if(myCube != current_cube || myLight != current_light || distanceX != current_dx || distanceY != current_dy || numRows != current_numrow || numColumns != current_numcol || startX != current_startx || startY != current_starty || startZ != current_startz){
    //         if(myCube != null && myLight != null && distanceX != 0 && distanceY != 0 && numRows != 0 && numColumns != 0){
    //             print("Matrix Configuration Values have been changed, please press 'D' to redraw the matrix...");
    //             SaveCurrentInfo();
    //         }
    //         locked_keys = true;
    //     }
    // }

    /* Unused Functionality, Editor is no longer being used to change the dimensions of the matrix, thus no checking needs to be done */
    /* Save current states into variables for OnValidate to check */
    // public void SaveCurrentInfo(){
    //     current_cube = myCube;
    //     current_light = myLight;
    //     current_startx = startX;
    //     current_starty = startY;
    //     current_startz = startZ;
    //     current_dx = distanceX;
    //     current_dy = distanceY;
    //     current_numrow = numRows;
    //     current_numcol = numColumns;
    // }

    /* Unused Functionality, Editor is no longer being used to change the dimensions of the matrix, thus no checking needs to be done */
    /* Checks to see if given values are valid */
    // public bool CheckEmpty(){
    //     if(myCube == null || myLight == null || distanceX <= 0 || distanceY <= 0 || numRows <= 0 || numColumns <= 0){
    //         return true;
    //     } else {
    //         return false;
    //     }
    // }
}