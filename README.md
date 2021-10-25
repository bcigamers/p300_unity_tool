## P300_Unity_v2

Thanks for downloading the P300 Unity tool Version 2! 
This tool was created to help game developers rapidly introduce a simple brain-computer interface (BCI) control scheme (the P300) into their games. 
The updated P300 Unity asset has been written for integration into a BCI processing pipeline using the LabStreamingLayer (LSL) or BrainsAtPlay (B@P) backbone to synchronize events occuring in the game with specific brain activity.

##PATCH NOTES:
There has been significant changes in this tool. We have updated the back-end to better function with more flexible tools in python, and added support for WebGL Games to integrate with the JS BrainsAtPlay suite. (https://brainsatplay.com)

## Getting Started

The P300 Unity project was built around the idea of providing a flexible framework for game developers to integrate in the P300 BCI control scheme. To get started, you simply need to open the P300_unity_tool zip file in Unity. This includes all of the necessary back-end components requried for including a P300 control scheme.

### Prerequisites

Unity Game Engine, V. 2020.3.1 or higher. **It is critical to have at least this version of Unity, or else certain parts of the package manager may not work**.

### Setting up the P300 Variables

Here is a step by step set of instructions to help you how get all of the game objects needed for running the P300 tool in your game.

#Create the main P300 controller game object.

```
In the Unity editor, create an empty game object and name it P300Controller.
```

#Add the required components to the P300Controller object.

```
Right-click on the P300Controller object and add the P300_Controller.cs, Setup_P300.cs, SingleFlash.cs, and P300Events scripts from the P300_Unity package. (Alternatively, drag these scripts from the package folder onto your P300Controller object.

These scripts are responsible for the following:
- P300_Controller.cs - Primary controller for the P300 events. This class also let's user's define the parameters of the P300 stimulus event through the editor. Inputs include the 'S' key for "Flash Start/Stop" and the 'Q' key to quit the program. If you wish to use specific game objects for the P300 stimulus, put them into the 'myObject' field. This script assumes you will be using the P300 to select game objects directly.

- Setup_P300.cs - Initializes a matrix of objects by default for the P300 flashing.

- SingleFlash.cs - This script contains all the logic for how flashing occurs in the P300 program.

- P300Events.cs - This is the primary event handling script using Unity's Action Event system.
 
```

#Add the required *MarkerStreams* object to the game scene.
```
In the P300_Unity package folder, find the MarkerStreams prefab object located in the Prefabs folder. Add this to the game scene. 
```

#Now verify that the added *MarkerStreams* game object has 2 attached components: the *LSL Marker Stream* script and the *Inlet_P300_Event* script. If it does not have those two scripts then you will need to add them.

```
If the MakerStreams prefab does not have the above scripts, they need to be added. The LSL Maker Stream script can be found under P300_Unity/LSL4Unity/Scripts/. The Inlet_P300 is right inside the P300_Unity folder.
```

#Add the *cube* prefab into the game. This will be the primary 'object' the P300 will use to populate the scene. You can change this prefab to be any shape/color/sprite you wish for your game.

```
From the P300_Unity/Prefabs folder, drag the cube prefab into the game heirarchy. Feel free to have the cube prefab be inactive in the hiearachy.
```

That should be it! You should now be able to hit play, and the P300 tool will populate a set of *game objects* based on the options in the *P300_Controller* component. Hit "S" to try running single flashes, and you should see an output reading in your debug log naming off each cube every time it is highlighted.

## Using the P300 Unity Tool

There are several places for you as the developer to alter the P300 tool to do what you want. 

<p>First, you can alter what type of objects are being highlighted by changing the properties of the *cube* prefab.
Note that the prefab includes an 'ActionEvents' script. This is where you can change the behavior of what happens when an object is selected by the P300 as it is paired with the *P300Events* script on the P300_Controller using Unity's built-in event system.</p>

<p>If you want to change what happens when the correct object is selected, you will need to edit the *Action_Controller* script, or work with the "P300Event" and "Action Controller" scripts. Right now there is just:
`cube_list[cubeIndex].GetComponent<Renderer>().material.color = Color.red;` showing the object that was selected.

Information coming from MATLAB/Python/etc. are fed through the *Inlet_P300_Event* script back to Unity to tell it what item was selected.</p>
 
## Built With

* [LabStreamingLayer](https://github.com/sccn/labstreaminglayer) - The synchronization framework used
* [LSL4Unity](https://github.com/xfleckx/LSL4Unity) - The LSL wrapper used for Unity

## Contributing

Please contact [Eli Kinney-Lang](https://github.com/ekinney-lang) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

This is the version 2.0 of the P300 Unity asset. Updated October 21, 2021 by EKL.

## Authors

* **Eli Kinney-Lang** - *Project Lead* - [Github Repo](https://github.com/ekinney-lang)
* **Shem Murji** - *Primary Contributor* - [Github Repo](https://github.com/shemmurji/shemmurji.github.io)

BrainsAtPlay Support
* **Juris Skalbeard - *Contributor* - [Github Repo](https://github.com/Skalbeard)


## License

This project is licensed under the CC-BY-SA 3.0- see [here](https://creativecommons.org/licenses/by-sa/3.0/) for details

## Acknowledgments

* This project was developed under the support of the Calgary Pediatric Stroke Program (CPSP), BCI Games, BCI4Kids, Dr. Adam Kirton and Dr. Ephrem Zewdie.
* Original funding for this work provided by: Biomedical Engineering Program at the University of Calgary

