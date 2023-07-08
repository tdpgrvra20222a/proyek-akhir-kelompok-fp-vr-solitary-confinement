using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ControllerScript : MonoBehaviour
{
    /*
    public Camera sceneCamera;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float step;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = sceneCamera.transform.position + sceneCamera.transform.forward * 3.0f;
    }
    void centerCube()
    {
        targetPosition = sceneCamera.transform.position + sceneCamera.transform.forward * 3.0f;
        targetRotation = Quaternion.LookRotation(transform.position - sceneCamera.transform.position);

        transform.position = Vector3.Lerp(transform.position, targetPosition, step);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, step);
    }

    // Update is called once per frame
    void Update()
    {
        step = 5.0f * Time.deltaTime;
        if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.0f) centerCube();
        if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft)) transform.Rotate(0, 5.0f * step, 0);
        if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight)) transform.Rotate(0, -5.0f * step, 0);
        if (OVRInput.GetUp(OVRInput.Button.One))
        {
            OVRInput.SetControllerVibration(1, 1, OVRInput.Controller.RTouch);
        }
        if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > 0.0f)
        {
            transform.position = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
            transform.rotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
        }
    }
    */

    [Header("Text")]
    public TextMeshProUGUI timerText;
    private bool ShowTimer;
    [SerializeField] public bool showtimer
    {
        get { return ShowTimer; }
        set { ShowTimer = value; timerText.enabled = value; }
    }
    private bool ShowText;
    [SerializeField] public bool showtest
    {
        get { return ShowText; }
        set { ShowText = value; test.enabled = value; }
    }
    public TextMeshProUGUI indicatortext;
    public TextMeshProUGUI guitexttime;
    public TextMeshProUGUI test;

    [Header("Timer")]
    private float timerremaining;
    private float timermult = 20f;
    private float Timerset;
    public float timerset //should be in seconds. 60 seconds = 1 minute
    {
        get { return Timerset; }
        set { Timerset = value; guitexttime.text = Timerset.ToString(); }
    }
    public bool isstarting = false;
    // private bool isstarting = false;
    private bool isrunning = true;

    [Header("Lighting")]
    public Vector3 lightproperty = new Vector3(45f, 0f, 0f);
    public GameObject sunobj;

    [Header("Events. from 0; day cycle, rain, earthquake, red")]
    public bool EnableEvent = true;
    public UnityEvent MulaiEvent;
    public float WaktuEventDivider = 8f;
    private float waktueventset;
    public UnityEvent suara;
    public CameraSettig modeobj;
    public int modeval;
    private static bool eventonce = false;
    // Start is called before the first frame update

    [Header("UI Summoner")]
    private bool[] inputcombi = new bool[2] { false, false };
    public GameObject uiobj;
    public GameObject raycasterobj;

    public void TimerUp()
    {
        timerset += timermult;
        timerremaining = timerset;
        waktueventset = timerset / WaktuEventDivider;
        timerText.text = timerremaining.ToString("0.00");
    }

    public void TimerDown()
    {
        timerset -= timermult;
        timerremaining = timerset;
        waktueventset = timerset / WaktuEventDivider;
        timerText.text = timerremaining.ToString("0.00");
    }

    public void UIExit()
    {
        uiobj.SetActive(false);
        raycasterobj.SetActive(false);
    }

    public void InitLight()
    {
        sunobj.transform.eulerAngles = lightproperty;
    }

    public void InitValues()
    {
        isstarting = false;
        isrunning = true;
        test.text = inputcombi[0].ToString() + " " + inputcombi[1].ToString();
        guitexttime.text = timerset.ToString();
        timerremaining = timerset;
        waktueventset = timerremaining / WaktuEventDivider;
        eventonce = false;
        indicatortext.text = "press \"A\" to start" ;
    }

    void Start()
    {
        timerset = 480f;
        InitValues();
        InitLight();
        showtimer = false;
        showtest = false;
        timerText.enabled = showtimer;
        test.enabled = showtest;
    }

    // Update is called once per frame
    void Update()
    {
        if (EnableEvent && !eventonce)
        {
            if (timerset - timerremaining >= waktueventset)
            {
                MulaiEvent.Invoke();
                eventonce = true;
            }
        }

        if (!isstarting)
        {
            if (OVRInput.Get(OVRInput.Button.One))
            {
                if (uiobj.activeSelf && raycasterobj.activeSelf) UIExit();
                modeobj.InitValues();
                InitLight();
                indicatortext.enabled = false;
                isstarting = true;
                suara.Invoke();
            }
        }

        if ((!isstarting || (!isrunning && isstarting)) && (!uiobj.activeSelf && !raycasterobj.activeSelf))
        {
            if (OVRInput.GetDown(OVRInput.Button.Three)) inputcombi[0] = true;
            if (OVRInput.GetDown(OVRInput.Button.Four)) inputcombi[1] = true;
            if (OVRInput.GetUp(OVRInput.Button.Three)) inputcombi[0] = false;
            if (OVRInput.GetUp(OVRInput.Button.Four)) inputcombi[1] = false;

            test.text = inputcombi[0].ToString() + " " + inputcombi[1].ToString();
            //Debug.Log(inputcombi[0].ToString() + " " + inputcombi[1].ToString());

            if (OVRInput.Get(OVRInput.Button.Start))
            {
                bool combiflag = true;
                for (int i = 0; i < 2; i++)
                {
                    if (!inputcombi[i])
                    {
                        combiflag = false;
                        break;
                    }
                }

                if (combiflag)
                {
                    uiobj.SetActive(true);
                    raycasterobj.SetActive(true);
                }
                else
                {
                    test.text = "incorrect";
                }
            }
        }

        timerText.text = timerremaining.ToString("0.00");
        if (isrunning && isstarting)
        {
            //Debug.Log(timerremaining);
            if (timerremaining <= 0)
            {
                suara.Invoke();
                timerremaining = 0f;
                isrunning = false;
                indicatortext.enabled = true;
                if (modeobj.modetype != 3)
                {
                    InitValues();
                    modeobj.modetype++;
                    indicatortext.text = "next sim. press \"A\"";
                }
                else
                {
                    indicatortext.text = "simulation complete.";
                }
                Debug.Log("time stopped");
            }
            else
                timerremaining -= Time.deltaTime;
        }
    }
}
