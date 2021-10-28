using UnityEngine;


public class GameApplication : Singleton<GameApplication>
{
    // Probably can abstract this and use it as parent for any kind of other game application
    void Start()
    {
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
