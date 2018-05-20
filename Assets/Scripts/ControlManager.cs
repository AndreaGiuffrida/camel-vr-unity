using System;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;
using Valve.VR;

public class ControlManager : MonoBehaviour {

    public SprayableObject sprayableObject;

    public Color[] colors;
    public Tools[] tools;
    public Controller controller;

    private List<BrushStroke[]> brushStroke = new List<BrushStroke[]>();
    private Splatter splatter;

    public Sound[] sounds;

    public Material brushMaterial;

    GameObject container;

    float deltaTime = 0.0f;

    void Start() {
        sprayableObject.Init(colors);
        controller.Init(tools, colors, sounds, container);
        splatter = new Splatter(sprayableObject, controller, tools, colors, sounds);
        container = new GameObject("Stroke Container");

        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == "ambientTutorial")
                sounds[i].PlaySound();
        }
    }

    void Update()
    {
        if ((int)controller.trackedObj.index != -1)
        {
            //Trigger
            if (controller.device.GetHairTriggerDown())
            {
                controller.isPainting = true;

                GameObject stroke = new GameObject("Stroke #" + brushStroke.Count);
                stroke.transform.parent = container.transform;
                brushStroke.Add(new BrushStroke[2]);

                for (int i = 0; i < 2; i++)
                    brushStroke[brushStroke.Count - 1][i] = new BrushStroke(controller, tools, colors, sounds, brushMaterial, stroke, (i == 0) ? true : false);
            }

            if (controller.device.GetHairTriggerUp())
                controller.isPainting = false;

            //Trackpad
            if (controller.device.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
                controller.TrackpadTouchDown(controller.device.GetAxis(EVRButtonId.k_EButton_SteamVR_Touchpad));
            else
                controller.TrackpadTouchUp();

            if (controller.device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
                controller.TrackpadPressDown(controller.device.GetAxis(EVRButtonId.k_EButton_SteamVR_Touchpad));

            if (controller.device.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
                controller.TrackpadPressUp(controller.device.GetAxis(EVRButtonId.k_EButton_SteamVR_Touchpad));
        }
        
        if (tools[controller.currentTool].toolType == Tools.ToolType.t3D)
        {
            //splatter.line.enabled = false;
            splatter.Clear();
            splatter.line.enabled = false;

            if (controller.isPainting == true)
            {
                for (int i = 0; i < 2; i++)
                    brushStroke[brushStroke.Count - 1][i].GenerateStroke();
            }
        }
        else if (tools[controller.currentTool].toolType == Tools.ToolType.t2D)
        {
            splatter.Cursor();

            if (tools[controller.currentTool].splatterType == Tools.SplatterType.Spray)
            {
                if (controller.isPainting == true)
                    splatter.Spray();
            }
            else if(tools[controller.currentTool].splatterType == Tools.SplatterType.Stamp)
            {
                if (controller.isPainting == true)
                    splatter.Stamp();
                else
                    splatter.StampReleased();
            }
        }
        controller.ControllerUpdate();

        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

   

    /*
    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        GUI.Label(rect, text, style);
    }
    */
}
