﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; 
using UnityEngine;

/// <summary>
/// Control the elements in the sidebar and buffer them from other 
/// classes trying to access them. 
/// </summary>
public class SidebarController : MonoBehaviour
{
    public GameObject panel, levelDescriptor, availableTools;
    public GameObject[] tools = new GameObject[6]; 
    public GameObject checklist, tool, timer, hint;
    private Sprite[] panels = new Sprite[8];
    EnergyController EnergyController; 
    Image ToggleArrow, toggleTool; 
    Sprite downArrow, upArrow; 
    private stringLib stringLibrary;
    bool active = true; 
    public bool isActive
    {
        get
        {
            return true; 
        }
    }
    public void NextTool()
    {
        if (!active)
            tool.GetComponent<SelectedTool>().NextTool(); 
    }
    private void LoadPanels()
    {
        string path = "Sprites/panel-";
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i] = Resources.Load<Sprite>(path + (i + 2).ToString());
        }
        panels[1] = Resources.Load<Sprite>(path + "3Main"); 
    }
    // Start is called before the first frame update
    void Start()
    {
        LoadPanels();
        EnergyController = GameObject.Find("Energy").GetComponent<EnergyController>(); 
        ToggleArrow = transform.GetChild(2).transform.Find("ToggleSidebar").GetComponent<Image>(); 
        toggleTool = transform.GetChild(2).transform.Find("Toggle Tool").GetComponent<Image>(); 
        downArrow = Resources.Load<Sprite>("Sprites/arrowDown"); 
        upArrow = Resources.Load<Sprite>("Sprites/arrowUp"); 
        stringLibrary = new stringLib(); 
        levelDescriptor.GetComponent<Text>().text = GlobalState.level.Description;
        if (SystemInfo.operatingSystem.Contains("Android") || SystemInfo.operatingSystem.Contains("iOS")){
            transform.GetChild(2).transform.Find("Indicate Hide").GetComponent<Text>().text = ""; 
            transform.GetChild(2).transform.Find("Indicate Show").GetComponent<Text>().text = "";  
        }
    }
    IEnumerator FadeToolToggler(bool fadeIn){
        float frames = 20f; 
        float difA = 1/frames; 
        if (!fadeIn) difA*=-1; 
        CanvasGroup canvas = toggleTool.GetComponent<CanvasGroup>(); 
        while((canvas.alpha < 1 && fadeIn) || canvas.alpha > 0 && !fadeIn){
            canvas.alpha += difA; 
            yield return null; 
        }
    }
    public void ToggleSidebar()
    {
        if (!GlobalState.level.IsDemo){
            StopCoroutine(FadeToolToggler(!active)); 
            active = !active;
            this.GetComponent<Animator>().SetBool("open", !this.GetComponent<Animator>().GetBool("open"));
            EnergyController.GetComponent<Animator>().SetBool("open", !EnergyController.GetComponent<Animator>().GetBool("open"));
            if (active){
                ToggleArrow.sprite = downArrow; 
            }
            else ToggleArrow.sprite = upArrow;
            StartCoroutine(FadeToolToggler(!active)); 
        } 
    }
    public void ToggleLight()
    {
        panel.GetComponent<Image>().sprite = panels[4];
        timer.GetComponent<Text>().color = Color.black; 
        checklist.GetComponent<Text>().color = Color.black;
        tool.GetComponent<Text>().color = Color.black;
        levelDescriptor.GetComponent<Text>().color = Color.black; 
        availableTools.GetComponent<Text>().color = Color.black;
        tool.GetComponent<Text>().color = Color.black;
        //hint.GetComponent<Text>().color = Color.black;

        checklist.GetComponent<Text>().text = checklist.GetComponent<Text>().text.Replace(stringLibrary.checklist_complete_color_tag, stringLibrary.checklist_complete_color_tag_dark);
        checklist.GetComponent<Text>().text = checklist.GetComponent<Text>().text.Replace(stringLibrary.checklist_incomplete_activate_color_tag, stringLibrary.checklist_incomplete_activate_color_tag_dark);
        checklist.GetComponent<Text>().text = checklist.GetComponent<Text>().text.Replace(stringLibrary.checklist_incomplete_question_color_tag, stringLibrary.checklist_incomplete_question_color_tag_dark);
        checklist.GetComponent<Text>().text = checklist.GetComponent<Text>().text.Replace(stringLibrary.checklist_incomplete_name_color_tag, stringLibrary.checklist_incomplete_name_color_tag_dark);
        checklist.GetComponent<Text>().text = checklist.GetComponent<Text>().text.Replace(stringLibrary.checklist_incomplete_comment_color_tag, stringLibrary.checklist_incomplete_comment_color_tag_dark);
        checklist.GetComponent<Text>().text = checklist.GetComponent<Text>().text.Replace(stringLibrary.checklist_incomplete_uncomment_color_tag, stringLibrary.checklist_incomplete_uncomment_color_tag_dark);
        transform.GetChild(2).transform.Find("Indicate Hide").GetComponent<Text>().color = Color.black; 
        transform.GetChild(2).transform.Find("Indicate Show").GetComponent<Text>().color = Color.black; 
        for (int i = 0; i < stateLib.NUMBER_OF_TOOLS - 1; i++)
        {
            // @TODO: figure out if theyre done or not and put it in the expression
            tools[i].transform.GetChild(0).GetComponent<Text>().color = (false) ? new Color(0, 0.6f, 0.2f, 1) : (GlobalState.IsDark ? Color.white : Color.black); 
        }
        ToggleArrow.color = Color.black; 
    }
    public void ToggleDark()
    {
        timer.GetComponent<Text>().color = Color.white;
        panel.GetComponent<Image>().sprite = panels[1];
        levelDescriptor.GetComponent<Text>().color = Color.white;
        checklist.GetComponent<Text>().color = Color.white; 
        availableTools.GetComponent<Text>().color = Color.white;
        tool.GetComponent<Text>().color = Color.white;
        levelDescriptor.GetComponent<Text>().color = Color.white;

        checklist.GetComponent<Text>().text = checklist.GetComponent<Text>().text.Replace(stringLibrary.checklist_complete_color_tag, stringLibrary.checklist_complete_color_tag_light);
        checklist.GetComponent<Text>().text = checklist.GetComponent<Text>().text.Replace(stringLibrary.checklist_incomplete_activate_color_tag, stringLibrary.checklist_incomplete_activate_color_tag_light);
        checklist.GetComponent<Text>().text = checklist.GetComponent<Text>().text.Replace(stringLibrary.checklist_incomplete_question_color_tag, stringLibrary.checklist_incomplete_question_color_tag_light);
        checklist.GetComponent<Text>().text = checklist.GetComponent<Text>().text.Replace(stringLibrary.checklist_incomplete_name_color_tag, stringLibrary.checklist_incomplete_name_color_tag_light);
        checklist.GetComponent<Text>().text = checklist.GetComponent<Text>().text.Replace(stringLibrary.checklist_incomplete_comment_color_tag, stringLibrary.checklist_incomplete_comment_color_tag_light);
        checklist.GetComponent<Text>().text = checklist.GetComponent<Text>().text.Replace(stringLibrary.checklist_incomplete_uncomment_color_tag, stringLibrary.checklist_incomplete_uncomment_color_tag_light);
        transform.GetChild(2).transform.Find("Indicate Hide").GetComponent<Text>().color = Color.white; 
        transform.GetChild(2).transform.Find("Indicate Show").GetComponent<Text>().color = Color.white; 
        for (int i = 0; i < stateLib.NUMBER_OF_TOOLS - 1; i++)
        {
            // @TODO: figure out if theyre done or not and put it in the expression
            tools[i].transform.GetChild(0).GetComponent<Text>().color = (false) ? Color.green : (GlobalState.IsDark ? Color.white : Color.black); 
        }
        ToggleArrow.color = Color.white; 
    }
    // Update is called once per frame
    void Update()
    {
        if (GlobalState.GameState == stateLib.GAMESTATE_IN_GAME){
            this.GetComponent<Canvas>().enabled = true;
        }
        else if (GlobalState.GameState != stateLib.GAMESTATE_IN_GAME) GetComponent<Canvas>().enabled = false; 
    }
}
