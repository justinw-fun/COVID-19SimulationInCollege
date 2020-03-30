using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ParamController : MonoBehaviour
{

    public Text DayUI; //Day text.
    public Toggle CheckBox; //Toggle that controlls whether to go hospital.
    public GameObject GymTarget;
    public GameObject HospitalTarget;
    public AudioSource audioSource;

    public float TimeScale = 1.0f;
    public float ActivityRate = 100.0f;
    public float SpreadRate = 5.0f;
    public float IncubationSpreadRate = 2.0f;
    public float GymFrequency = 50.0f;
    public bool GoToHospital = false;
    public int StandardTime = 15; //Time spend for going to next destination.

    
    [HideInInspector]
    public IAmClassroom[] ClassList;
    [HideInInspector]
    public IAmCafeteria[] CafeteriaList;
    [HideInInspector]
    public string text;

    public Slider[] Sliders;
    public Text[] InfectedNum;
    private MouseController mouseController;
    List<GameObject> stuList = new List<GameObject>();
    List<Collider> StuCollider = new List<Collider>();

    private NavigationAgent[] navTargets;
    private bool isPlaying;
    void InitPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("SpreadRate"))
        {
            SpreadRate = PlayerPrefs.GetFloat("SpreadRate");
        }
        if (PlayerPrefs.HasKey("IncubationSpreadRate"))
        {
            IncubationSpreadRate = PlayerPrefs.GetFloat("IncubationSpreadRate");
        }
        if (PlayerPrefs.HasKey("GymFrequency"))
        {
            GymFrequency = PlayerPrefs.GetFloat("GymFrequency");
        }
        if (PlayerPrefs.HasKey("TimeScale"))
        {
            TimeScale = PlayerPrefs.GetFloat("TimeScale");
        }
        if (PlayerPrefs.HasKey("ActivityRate"))
        {
            ActivityRate = PlayerPrefs.GetFloat("ActivityRate");
        }
        if (PlayerPrefs.HasKey("AutoGeli"))
        {
            GoToHospital = PlayerPrefs.GetInt("AutoGeli") == 0 ? false : true;
        }
    }
    private void OnEnable()
    {
        // Restore params saved last time from PlayerPrefs.
        InitPlayerPrefs();
        ClassList = FindObjectsOfType<IAmClassroom>();
        CafeteriaList = FindObjectsOfType<IAmCafeteria>();
        navTargets = FindObjectsOfType<NavigationAgent>();
        mouseController = FindObjectOfType<MouseController>();
        foreach (var item in navTargets)
        {
            stuList.Add(item.gameObject);
            StuCollider.Add(item.gameObject.GetComponent<Collider>());
        }
        audioSource = GetComponent<AudioSource>();
    }
    /// <summary>
    /// Upate UI in every 0.5s
    /// </summary>
    /// <returns></returns>
    IEnumerator SometimesUpdateInfectedNum()
    {
        //Debug.Log("SometimesUpdateInfectedNum");
        int[] infectArray = new int[4] { 0, 0, 0, 0 };// Store the number of each agent's state.
        foreach (var item in navTargets)
        {
            //Convert the Enum into int
            infectArray[(int)item.state] = infectArray[(int)item.state] + 1;
        }
        //Flush the UI text
        for (int i = 0; i < InfectedNum.Length; i++)
        {
            InfectedNum[i].text = infectArray[i].ToString();
        }

        yield return new WaitForSeconds(0.5f);

    }

    private void Start()
    {
        //Register event for CheckBox, which controlls GoToHospital
        CheckBox.onValueChanged.AddListener((bool value) => OnCheckBoxValueChange(value, CheckBox));

        //Register event for each Slider
        foreach (Slider item in Sliders)
        {
            item.onValueChanged.AddListener((float value) => OnSliderValueChange(value, item));
        }

        FlushSlidersCheckBox();

        //Enable all NavAgents and Collider
        setActivate(isPlaying);

    }

    private void OnCheckBoxValueChange(bool value, Toggle checkBox)
    {
        GoToHospital = value;
    }


    // Ihis function is to be called when slider value change. 
    private void OnSliderValueChange(float value, Slider EventSender)
    {
        Text text = EventSender.gameObject.GetComponentInChildren<Text>();
        text.text = Math.Round(value, 2).ToString() + "%";
        mouseController.enabled = false;
        switch (EventSender.name)
        {
            case "TimeScaleSlider":
                TimeScale = value;
                text.text = "x" + Math.Round(value, 2).ToString();
                break;
            case "InfSlider":
                SpreadRate = value;
                break;
            case "PreInfSlider":
                IncubationSpreadRate = value;
                break;
            case "GymSlider":
                GymFrequency = value;
                break;
            case "ActivateSlider":
                ActivityRate = value;
                break;

        }
    }

    void Update()
    {
        if (isPlaying)
        {
            Time.timeScale = TimeScale;
        }
        else
        {
            Time.timeScale = 0;
        }
        
        DayUI.text = "Day: " + text;
        if (mouseController.enabled == false && Input.GetMouseButtonUp(0))
        {
            mouseController.enabled = true;
        }
        StartCoroutine(SometimesUpdateInfectedNum());

    }


    void setActivate(bool condition)
    {
        foreach (var item in navTargets)
        {
            item.enabled = condition;
        }
        foreach (var item in StuCollider)
        {
            item.enabled = condition;
        }

    }


    void FlushSlidersCheckBox()
    {
        CheckBox.isOn = GoToHospital;
        foreach (Slider item in Sliders) 
        {
            switch (item.name)
            {
                case "TimeScaleSlider":
                    item.value = TimeScale;

                    break;
                case "InfSlider":
                    item.value = SpreadRate;
                    break;
                case "PreInfSlider":
                    item.value = IncubationSpreadRate;
                    break;
                case "GymSlider":
                    item.value = GymFrequency;
                    break;
                case "ActivateSlider":
                    item.value = ActivityRate;
                    break;
            }

        }
    }

    /// <summary>
    /// When click Reset Button, reset all the params.
    /// </summary>
    public void ResetParams()
    {
        SpreadRate = 5.0f;
        IncubationSpreadRate = 2.0f;
        GymFrequency = 50.0f;
        ActivityRate = 100.0f;
        TimeScale = 1.0f;
        GoToHospital = false;
        FlushSlidersCheckBox();
    }
    /// <summary>
    /// When Click Start button, enable all the agents and disable all the sliders 
    /// and checkbox, except TimeScale.
    /// </summary>
    public void StartPlay()
    {
        isPlaying = true;
        setActivate(isPlaying);
        foreach (Slider item in Sliders)
        {
            if (item.name != "TimeScaleSlider")//TimeScaleSlider still be worked.
            {
                item.interactable = false;
            }

        }
        CheckBox.interactable = false;
        audioSource.Play();
    }
    public void StopPlaying()
    {
        audioSource.Stop();
        isPlaying = false;
        setActivate(isPlaying);
        foreach (Slider item in Sliders) 
        {
            item.interactable = true;
        }
        CheckBox.interactable = true;


        // Save all the paras to PlayerPrefs.
        PlayerPrefs.SetFloat("SpreadRate", SpreadRate);
        PlayerPrefs.SetFloat("IncubationSpreadRate", IncubationSpreadRate);
        PlayerPrefs.SetFloat("GymFrequency", GymFrequency);
        PlayerPrefs.SetFloat("TimeScale", TimeScale);
        PlayerPrefs.SetFloat("ActivityRate", ActivityRate);
        PlayerPrefs.SetFloat("AutoGeli", GoToHospital == false ? 0 : 1);

        // Get the current scene and reload.
        string SceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(SceneName);

    }
}
