using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flash : MonoBehaviour
{
    public int refresh_rate = 60;

    
    public FrameLimiter frameLimiter;

    public int stim_freq = 10;
    private int period;
    private int ISI_count = 0;
    private int frames_off;
    private int frames_on;
    private bool frame_on = false;

    public GameObject cube;

    void Start()
    {
        period = (frameLimiter.refresh_rate / stim_freq) / 2;
    }
    void Update()
    {
        ISI_count++;

        if (ISI_count % period == 0)
        {
            if (frame_on == true)
            {
                // turn the cube on or off
                cube.GetComponent<Renderer>().material.color = Color.green;
                frame_on = false;
            }
            else
            {
                // turn the cube on or off
                cube.GetComponent<Renderer>().material.color = Color.blue;
                frame_on = true;
            }
        }





    }
}
