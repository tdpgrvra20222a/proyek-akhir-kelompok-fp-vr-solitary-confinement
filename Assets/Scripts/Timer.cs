using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [Header("Text")]
    public TextMeshProUGUI timerText;
    public bool showtimer = false;
    public TextMeshProUGUI indicatortext;

    [Header("Timer")]
    public float timerset = 30; //should be in seconds. 60 seconds = 1 minute
    private float timerremaining;
    private bool isstarting = false;
    private bool isrunning = true;

    [Header("Fader")]
    public float speedScale = 1f;
    public Color fadeColor = Color.black;
    public AnimationCurve Curve = new AnimationCurve(
        new Keyframe(0, 1), new Keyframe(0.5f, 0.5f, -1.5f, -1.5f), new Keyframe(1, 0));
    private bool startFadedOut = false;

    private float alpha = 0f;
    private Texture2D texture;
    private int direction = -1;
    private float fadetime = 1f;

    public UnityEvent sceneevent;
    // Start is called before the first frame update
    void Start()
    {
        timerText.enabled = showtimer;
        timerremaining = timerset;

        if (startFadedOut) alpha = 1f; else alpha = 0f;
        texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha));
        texture.Apply();
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.Two))
        {
            if (timerText.enabled) timerText.enabled = false;
            else timerText.enabled = true;
        }

        if (OVRInput.Get(OVRInput.Button.One) && !isstarting)
        {
            indicatortext.enabled = false;
            isstarting = true;
        }

        timerText.text = timerremaining.ToString("0.00");
        if (isrunning && isstarting)
        {
            //Debug.Log(timerremaining);
            if (timerremaining <= 0)
            {
                timerremaining = 0f;
                isrunning = false;
                Debug.Log("time stopped");
            }
            else
                timerremaining -= Time.deltaTime;
        }
    }

    public void OnGUI()
    {
        if (fadetime > 0f && !isrunning)
        {
            if (alpha > 0) GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texture);
            if (direction != 0)
            {
                fadetime += direction * Time.deltaTime * speedScale;
                alpha = Curve.Evaluate(fadetime);
                texture.SetPixel(0, 0, new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha));
                texture.Apply();
                if (alpha <= 0f || alpha >= 1f)
                {
                    Debug.Log("direction made to 0");
                    direction = 0;
                }
            }
        }
    }
}
