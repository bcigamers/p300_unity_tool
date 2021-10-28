public class BCIDataListener : Singleton<BCIDataListener>
{
    public static EEGData CurrentData;


    // Start is called before the first frame update
    void Start()
    {
        CurrentData = new EEGData {
            alpha = 0.0f,
            alphaBeta = 0.0f,
            alphaTheta = 0.0f,
            coherence = 0.0f,
            focus = 0.0f,
            thetaBeta = 0.0f,
            blink = 0.0f,
            o1 = 0.0f
        };

    }

    // These are called by the web Applet
    // The signature has to correspond on the applet side
    // All of the parameters have to be filled in one by one because these calls do not handle custom structures well
    public void UpdateAlpha(float alpha)
    {
        CurrentData.alpha = alpha;
    }

    public void UpdateAlphaBeta(float alphaBeta)
    {
        CurrentData.alphaBeta = alphaBeta;
    }

    public void UpdateAlphaTheta(float alphaTheta)
    {
        CurrentData.alphaTheta = alphaTheta;
    }

    public void UpdateCoherence(float coherence)
    {
        CurrentData.coherence = coherence;
    }

    public void UpdateFocus(float focus)
    {
        CurrentData.focus = focus;
    }

    public void UpdateThetaBeta(float thetaBeta)
    {
        CurrentData.thetaBeta = thetaBeta;
    }

    public void UpdateBlink(float blink)
    {
        CurrentData.blink = blink;
    }

    public void UpdateO1(float o1)
    {
        CurrentData.o1 = o1;
    }
}
