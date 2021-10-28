using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Linq;
using Assets.LSL4Unity.Scripts.AbstractInlets;
using LSL;

public class Demo_Inlet_P300 : AStringInlet
{
    private string input = "";
    private double timestamp;
    private List<GameObject> cube_list;
    public Sprite selectedSprite;
    private List<Sprite> default_images;
    private int cubeIndex;
    private GameObject sphere;
    private Rigidbody rb;


    protected override void Process(string[] newSample, double timeStamp)
    {
        input = newSample[0];
        timestamp = timeStamp;
        //Avoid doing heavy processing here, use CoRoutines
        //Obtain necessary information from the Demo_P300_Flashes.cs file. 
        GameObject cubeController = GameObject.Find("HUD_Controller");
        Demo_P300_Flashes p300Flashes = cubeController.GetComponent<Demo_P300_Flashes>();
        cube_list = p300Flashes.cube_list;
        default_images = p300Flashes.default_images;
        //Call CoRoutine to do further processing
        if(input == "P300 SingleFlash Ends" || input == "P300 RCFlash Ends"){
            StartCoroutine("SelectedCube");
        }

    }

    IEnumerator SelectedCube(){
        TurnOff();
        print("Flashes Done, displaying random cube and moving cube");

        System.Random random = new System.Random();
        int randomIndex = random.Next(cube_list.Count);
        GameObject randomCube = cube_list[randomIndex];
        print("SELECTING " + randomIndex);
        randomCube.GetComponent<Image>().sprite = selectedSprite;
        sphere = GameObject.Find("Sphere");
        rb = sphere.GetComponent<Rigidbody>();
        MoveSphere(randomIndex);
        yield return new WaitForSecondsRealtime(2);
    }

    /* Sets all cubes to the default Sprites */
    private void TurnOff(){
        for(int i = 0; i < cube_list.Count; i++){
            //Change the sprite to the default image preset
            cube_list[i].GetComponent<Image>().sprite = default_images[i];
        }
    }

    private void MoveSphere(int moveIndex){
        switch(moveIndex){
            case 0:
                print("Turning Ball Left");
                StartCoroutine(Rotate_Ball(new Vector3(0, 1f, 0)));
                break;
            case 1:
                print("Moving Ball Forward");
                StartCoroutine(Move_Ball(new Vector3(0,0,-1f)));
                break;
            case 2:
                print("Turning Ball Foward");
                StartCoroutine(Rotate_Ball(new Vector3(-1f, 0, 0)));
                break;
            case 3:
                print("Moving Ball Left");
                StartCoroutine(Move_Ball(new Vector3(-1f, 0, 0)));
                break;
            case 4:
                print("Jumping");
                StartCoroutine(Jumping());
                break;
            case 5:
                print("Moving Ball Right");
                StartCoroutine(Move_Ball(new Vector3(1f, 0, 0)));
                break;
            case 6:
                print("Turning Ball Backwards");
                StartCoroutine(Rotate_Ball(new Vector3(1f, 0, 0)));
                break;
            case 7:
                print("Moving Ball Backwards");
                StartCoroutine(Move_Ball(new Vector3(0, 0, 1f)));
                break;
            case 8:
                print("Turning Ball Right");
                StartCoroutine(Rotate_Ball(new Vector3(0, -1f, 0)));
                break;
        }
    }

    IEnumerator Move_Ball(Vector3 direction){
        float speed = 10f;
        float startTime = Time.time;
        Vector3 currentposition = sphere.transform.position;
        Vector3 endposition = sphere.transform.position + direction;

        while(currentposition != endposition && ((Time.time - startTime) * speed) < 1f){
            float move = Mathf.Lerp(0, 1, (Time.time - startTime) * speed);

            sphere.transform.position += direction * move;

            yield return null;
        }
    }

    IEnumerator Jumping(){
        float jumpHeight = 500f;

        rb.AddForce(Vector3.up * jumpHeight);

        yield return null;
    }

    IEnumerator Rotate_Ball(Vector3 direction){
        float speed = 10f;
        float startTime = Time.time;
        Vector3 currentposition = sphere.transform.position;
        Vector3 endposition = sphere.transform.position + direction;

        while(currentposition != endposition && ((Time.time - startTime) * speed) < 1f){
            float move = speed * Time.deltaTime;

            Vector3 newDir = Vector3.RotateTowards(sphere.transform.forward, direction, move, 0.0f);

            sphere.transform.rotation = Quaternion.LookRotation(newDir);

            yield return null;
        }
    }
}
