using UnityEngine;

// This is an extended version of the original script by Juris "Skalbeard" Z.
// ORIGINAL README:
// This is an adittional component that we add to whatever needs to listen to biofeedback data
// Currently it is a specific impleentation for the player character as it gets the CharacterSkinController and caches parameters specific to this game.
// Generally how you would want to go about it, is create your own BciObj class, attach an interface to it, then give it parameters that it wants to cache and run logic on.
// You can of course also just attach the BCIInteractable interface straight onto your game object class, depending on your implementation and intended scope.
// Edited by EKL, 19-Oct-2021. 
// Edits:
// Removed the dependency on IBCIInteractable interface. Added default values and event handling potential for default systems here instead to send a number of events to JS.
public class MyBCIGameObj : MonoBehaviour 
{
    private GameObject control;

    ////If we want to set default values in the inspector and pass those into the SendEventToWeb function below.
    //[SerializeField]
    //private int eventNumDefault = 999999;
    //[SerializeField]
    //private float eventFloatDefault = 99999.99999f;


    public bool startReaction = false;

    void Start()
    {
        control = GameObject.FindGameObjectWithTag("BAP");
    }

    void Update()
    {
        if (startReaction)
        {
            ReactToBCI();
        }
    }

    // send - test string example.
    private void OnMouseDown()
    {
        SendEventToWeb("Mouse Down String Test");
        
    }

    //Defaults to empty string, num 0 and event float 0.0f
    public void SendEventToWeb(string eventString = "", int eventNum = 999999, float eventFloat = 99999.99999f)
    {
        //Example with sending string. Only sending event eventNums & event Floatsif it isn't the default values, hardcoded to ridiculuously high numbers...

        DataSender.Instance.SendStringToJS(eventString);
        if (eventNum != 999999)
        {
            DataSender.Instance.SendNumToJS(eventNum);
        }
        if (eventFloat != 99999.99999f)
        {
            DataSender.Instance.SendFloatToJS(eventFloat);
        }
        
        

    }

    //This was all specifically coded for the animated character used in the Template. Changing it to be more generic.
    //public void ReactToBCI()
    //{
    //    if (control != null)
    //    {
    //        print("bcigameobj ReactToBCI coherence data: " + BCIDataListener.CurrentData.coherence
    //            + " and blink data: " + BCIDataListener.CurrentData.blink
    //            + " and focus data: " + BCIDataListener.CurrentData.focus);
    //        control.ChangeFacialExpression(BCIDataListener.CurrentData.focus);
    //        if (BCIDataListener.CurrentData.blink > 0.1f)
    //            control.Blink();
    //    }
    //}

    public void ReactToBCI()
    {
        print("Hey! This thing happened on your BrainsAtPlay object.");
        //do something interesting.
        print("bcigameobj ReactToBCI coherence data: " + BCIDataListener.CurrentData.coherence
            + " and blink data: " + BCIDataListener.CurrentData.blink
            + " and focus data: " + BCIDataListener.CurrentData.focus);

    }
}
