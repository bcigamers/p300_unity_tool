using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameLimiter : MonoBehaviour
{

    public int refresh_rate = 60;

    void Awake()
    {
        Application.targetFrameRate = refresh_rate;

    }
}
