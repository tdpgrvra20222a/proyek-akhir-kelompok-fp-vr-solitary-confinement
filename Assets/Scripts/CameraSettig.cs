using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CameraSettig : MonoBehaviour
{
    public ControllerScript conbase;
    public bool start;
    private float eventduration;
    public float[] eventtimerset = { 4, 2, 8 };
    private float[] eventtimer = new float[3];
    public AnimationCurve curve;

    [Header("Mode")]
    public TextMeshProUGUI guitextmode;
    private int ModeType;
    public int modetype
    {
        get { return ModeType; }
        set { ModeType = value; textupdate(); }
    }

    [Header("Audio Stuff")]
    private static Vector3[] lokasisuara = new[] { new Vector3(0f, -0.235f, -1.561f), new Vector3(0f, 0.206f, -1.561f) };
    public AudioSource suara;
    public GameObject audioobject;

    [Header("Day Cycle")]
    public GameObject sunmanage;
    public Vector3[] sunsetter = new Vector3[3];

    [Header("Rain")]
    public AudioClip[] cliphujan = new AudioClip[2];
    public float VolRain = 1.25f;
    public AnimationCurve raincurve;
    private Material sky;
    public ParticleSystem rainpart;
    public int[] raincount = new int[3];

    [Header("Gempa")]
    public AudioClip clipgempa;
    public float VolGempa = 0.5f;
    public float IntensityGempaDivider = 60f;

    [Header("Red")]
    public Light lampu1;
    public Light lampu2;
    public GameObject lampobj;
    public Material matobj1;
    public Material matobj2;
    
    public static CameraSettig instance;

    public void ModeNext()
    {
        if (modetype + 1 > 3) modetype = 0;
        else modetype += 1;
    }

    public void ModePrev()
    {
        if (modetype - 1 < 0) modetype = 3;
        else modetype -= 1;
    }

    private void Awake()
    {
        instance = this;
    }

    private void textupdate()
    {
        string modetext = "";
        switch (ModeType)
        {
            case 0:
                modetext = "Day Cycle";
                break;
            case 1:
                modetext = "Raining";
                break;
            case 2:
                modetext = "Earthquake";
                break;
            case 3:
                modetext = "Abnormal";
                break;
        }
        guitextmode.text = "Mode: " + modetext;
    }

    public void InitValues()
    {
        textupdate();
        //rain
        Light sunlight = sunmanage.GetComponent<Light>();
        sunlight.intensity = 1f;
        sky = RenderSettings.skybox;
        sky.SetColor("_SkyTint", Color.white);
        suara.Stop();
        var emission = rainpart.emission;
        emission.rateOverTime = raincount[0];
        rainpart.Stop();
        //abnormal
        lampu1.color = Color.white;
        lampu2.color = Color.white;
        lampobj.GetComponent<MeshRenderer>().materials[1].SetColor("_EmissionColor", Color.white);
        lampobj.GetComponent<MeshRenderer>().materials[1].color = Color.white;
        matobj1.color = Color.white;
        matobj2.color = Color.white;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitValues();
    }

    // Update is called once per frame
    void Update()
    {
        if (start)
        {
            start = false;
            starter();
        }
    }

    public void starter()
    {
        float basetime = conbase.timerset;
        eventtimer = new float[] { basetime / eventtimerset[0], basetime / eventtimerset[1], basetime / eventtimerset[2] };
        eventduration = eventtimer[0];
        switch (modetype)
        {
            case 0:
                StartCoroutine(DayCycle());
                break;
            case 1:
                audioobject.transform.position = lokasisuara[1];
                suara.clip = cliphujan[0];
                StartCoroutine(Rain());
                break;
            case 2:
                audioobject.transform.position = lokasisuara[0];
                suara.clip = clipgempa;
                StartCoroutine(Shaking());
                break;
            case 3:
                StartCoroutine(RedSummoner());
                break;
        }
    }

    public IEnumerator Shaking()
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        suara.volume = 0f;
        suara.Play();
        while (elapsedTime < eventduration)
        {
            elapsedTime += Time.deltaTime;
            float strength = curve.Evaluate(elapsedTime / eventduration);
            transform.position = startPosition + Random.insideUnitSphere * strength / IntensityGempaDivider / 2.5f;
            suara.volume = VolGempa * strength;
            yield return null;
        }
        elapsedTime = 0f;
        eventduration = eventtimer[1];
        suara.Stop();
        transform.position = startPosition;

        //round 2
        suara.Play();
        while (elapsedTime < eventduration)
        {
            elapsedTime += Time.deltaTime;
            float strength = curve.Evaluate(elapsedTime / eventduration);
            transform.position = startPosition + Random.insideUnitSphere * strength / IntensityGempaDivider;
            suara.volume = VolGempa * strength * 3;
            yield return null;
        }
        elapsedTime = 0f;
        eventduration = eventtimer[2];
        suara.Stop();
        transform.position = startPosition;

        //round 2
        suara.Play();
        while (elapsedTime < eventduration)
        {
            elapsedTime += Time.deltaTime;
            float strength = curve.Evaluate(elapsedTime / eventduration);
            transform.position = startPosition + Random.insideUnitSphere * strength / IntensityGempaDivider;
            suara.volume = VolGempa * strength * 3;
            yield return null;
        }
        suara.Stop();
        transform.position = startPosition;
    }

    public IEnumerator Rain()
    {
        float elapsedTime = 0f;

        suara.volume = 0f;
        suara.Play();
        Color basecolor = sky.GetColor("_SkyTint");
        Light sunlight = sunmanage.GetComponent<Light>();
        //darker sky
        while (elapsedTime < eventduration)
        {
            sunlight.intensity = Mathf.Lerp(1f, 0f, elapsedTime / eventduration);
            sky.SetColor("_SkyTint", Color.Lerp(basecolor, Color.black, elapsedTime / eventduration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        elapsedTime = 0f;
        eventduration = eventtimer[1];
        //starts raining
        var emission = rainpart.emission;
        emission.rateOverTime = raincount[0];
        rainpart.Play();
        while (elapsedTime < eventduration)
        {
            emission.rateOverTime = Mathf.Lerp(raincount[0], raincount[1], elapsedTime / eventduration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        emission.rateOverTime = raincount[1];
        elapsedTime = 0f;
        eventduration = eventtimer[2];
        //rains more
        while (elapsedTime < eventduration)
        {
            emission.rateOverTime = Mathf.Lerp(raincount[1], raincount[2], elapsedTime / eventduration);
            float strength = raincurve.Evaluate(elapsedTime / eventduration);
            suara.volume = 1f + VolRain * strength * 6;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public IEnumerator DayCycle()
    {
        float elapsedTime = 0f;
        Vector3 awalmuter = sunmanage.transform.eulerAngles;

        while (elapsedTime < eventduration)
        {
            sunmanage.transform.eulerAngles = Vector3.Lerp(awalmuter, sunsetter[0], elapsedTime / eventduration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        elapsedTime = 0f;
        eventduration = eventtimer[1];
        sunmanage.transform.eulerAngles = sunsetter[0];
        Light sunlight = sunmanage.GetComponent<Light>();

        //round 2
        while (elapsedTime < eventduration)
        {
            sunlight.intensity = Mathf.Lerp(1f, 0.3f, elapsedTime / eventduration);
            sunmanage.transform.eulerAngles = Vector3.Lerp(sunsetter[0], sunsetter[1], elapsedTime / eventduration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        elapsedTime = 0f;
        eventduration = eventtimer[2];
        sunmanage.transform.eulerAngles = sunsetter[1];

        //round 3
        while (elapsedTime < eventduration)
        {
            sunlight.intensity = Mathf.Lerp(0.3f, 0f, elapsedTime / eventduration);
            sunmanage.transform.eulerAngles = Vector3.Lerp(sunsetter[1], sunsetter[2], elapsedTime / eventduration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        sunmanage.transform.eulerAngles = sunsetter[2];
    }

    public IEnumerator RedSummoner()
    {
        float elapsedTime = 0f;
        float durasplit;
        Color warna1 = Color.white;
        Color warna2 = Color.red;
        Material untuklampu = lampobj.GetComponent<MeshRenderer>().materials[1];
        Debug.Log("round1");
        durasplit = eventduration * 40 / 100;
        while (elapsedTime < durasplit)
        {
            lampu1.color = Color.Lerp(warna1, warna2, elapsedTime / durasplit);
            lampu2.color = Color.Lerp(warna1, warna2, elapsedTime / durasplit);
            untuklampu.SetColor("_EmissionColor", Color.Lerp(warna1, warna2, elapsedTime / durasplit));
            untuklampu.color = Color.Lerp(warna1, warna2, elapsedTime / durasplit);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        elapsedTime = 0f;
        durasplit = eventduration * 60 / 100;

        while (elapsedTime < durasplit)
        {
            Debug.Log(lampu1.color);
            lampu1.color = Color.Lerp(warna2, warna1, elapsedTime / durasplit);
            lampu2.color = Color.Lerp(warna2, warna1, elapsedTime / durasplit);
            untuklampu.SetColor("_EmissionColor", Color.Lerp(warna2, warna1, elapsedTime / durasplit));
            untuklampu.color = Color.Lerp(warna2, warna1, elapsedTime / durasplit);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        elapsedTime = 0f;

        eventduration = eventtimer[1];
        durasplit = eventduration / 10;
        float elapsedsmt = 0f;
        bool colortime = false;
        Debug.Log("round2");
        while (elapsedTime < eventduration)
        {
            if (elapsedsmt > durasplit)
            {
                if (colortime) colortime = false;
                else colortime = true;
                elapsedsmt = 0f;
            }
            elapsedsmt += Time.deltaTime;
            if (colortime)
            {
                lampu1.color = warna1;
                lampu2.color = warna1;
                untuklampu.SetColor("_EmissionColor", warna1);
                untuklampu.color = warna1;
            }
            else
            {
                lampu1.color = warna2;
                lampu2.color = warna2;
                untuklampu.SetColor("_EmissionColor", warna2);
                untuklampu.color = warna2;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        lampu1.color = warna1;
        lampu2.color = warna1;
        untuklampu.color = warna1;
        untuklampu.SetColor("_EmissionColor", warna1);

        Debug.Log("round3");
        elapsedTime = 0f;
        eventduration = eventtimer[2];
        durasplit = eventduration * 25 / 100;
        while (elapsedTime < durasplit)
        {
            lampu1.color = Color.Lerp(warna1, warna2, elapsedTime / durasplit);
            lampu2.color = Color.Lerp(warna1, warna2, elapsedTime / durasplit);
            untuklampu.SetColor("_EmissionColor", Color.Lerp(warna1, warna2, elapsedTime / durasplit));
            untuklampu.color = Color.Lerp(warna1, warna2, elapsedTime / durasplit);
            sky.SetColor("_SkyTint", Color.Lerp(warna1, warna2, elapsedTime / durasplit));
            matobj1.color = Color.Lerp(warna1, warna2, elapsedTime / durasplit);
            matobj2.color = Color.Lerp(warna1, warna2, elapsedTime / durasplit);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

    }
}
