using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionController: MonoBehaviour
{
    //Set this id based on the object name and classifier response given by the Inlet_P300_Events system.
    public int id;

    private void Start()
    {
        
        P300Events.current.OnTargetSelection += OnP300Action;
    }

    private void OnP300Action(int id)
    {
        
        //Do whatever actions you want based on the id of the object! Example below just changes it's color. 
        if(id==this.id)
        {
            this.GetComponent<Renderer>().material.color = Color.red;
        }

    }

}
