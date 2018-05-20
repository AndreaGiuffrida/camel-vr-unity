 using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Controller {

    public GameObject controller;

    //Controller
    [Header("Controller")]
    public GameObject body;
    public GameObject trigger;
    public GameObject uI;
    [HideInInspector]
    public bool isPainting;

    //Material
    [Header("Material")]
    public Material colorMaterial;

    //Cursor
    [Header("Cursor")]
    public GameObject cursor;

    //Current
    [Header("Current")]
    public Image current;
    public Sprite defaultIcon;

    //Tools    
    [Header("Tools")]
    public Button toolButton;
    private Transform toolIcon;
    private Tools[] tool;
    public int currentTool;
    public GameObject line;

    //Colors
    [Header("Colors")]
    public Button colorButton;
    private Transform colorIcon;
    private Color[] color;
    public int currentColor;

    //Return
    [Header("Return")]
    public Button returnButton;
    private Transform returnIcon;

    public GameObject uIContainer;
    private GameObject brushContainer;

    Sound[] sounds;

    Color colorStart, colorEnd;

    //HTC Vive
    [HideInInspector]
    public SteamVR_TrackedObject trackedObj;
    [HideInInspector]
    public SteamVR_Controller.Device device
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    public void Init(Tools[] _tool, Color[] _color, Sound[] _sounds, GameObject _container)
    {
        //Define VIVE controller
        trackedObj = controller.GetComponent<SteamVR_TrackedObject>();

        //Define State
        isPainting = false;
        cursor.SetActive(false);

        //Pass properties from Control Manager
        tool = _tool;
        color = _color;
        sounds = _sounds;
        brushContainer = _container;

        //Define Icons
        colorIcon = colorButton.transform.GetChild(0);
        toolIcon = toolButton.transform.GetChild(0);
        returnIcon = returnButton.transform.GetChild(0);

        //Define colors 
        colorMaterial.color = color[currentColor];
        colorIcon.GetComponent<Image>().color = color[currentColor];

        toolIcon.GetComponent<Image>().color = color[currentColor];
        returnIcon.GetComponent<Image>().color = color[currentColor];
        colorMaterial.color = color[currentColor];

        colorStart = color[currentColor];
        colorEnd = color[currentColor + 1];

        HideTools();
    }

    public void ControllerUpdate()
    {
        
        Color colorLerp = Color.Lerp(colorStart, colorEnd, Mathf.PingPong(Time.time, 1));
        current.GetComponent<Image>().sprite = tool[currentTool].icon;

        colorMaterial.SetColor("_EmissionColor", colorLerp);

        toolIcon.GetComponent<Image>().color = colorLerp;
        colorIcon.GetComponent<Image>().color = colorLerp;
        returnIcon.GetComponent<Image>().color = colorLerp;
    }

    private Button ButtonSelection(Vector2 axis, bool isPressed, bool isRelased)
    {
        Button currentButton;

        bool isToolPressed = ButtonMapping(axis, toolButton, isPressed, isRelased);
        bool isColorPressed = ButtonMapping(axis, colorButton, isPressed, isRelased);
        bool isReturnPressed = ButtonMapping(axis, returnButton, isPressed, isRelased);

        if (isToolPressed)
            currentButton = toolButton;
        else if (isColorPressed)
            currentButton = colorButton;
        else if (isReturnPressed)
            currentButton = returnButton;
        else
            currentButton = null;

        return currentButton;
    }

    private bool ButtonMapping(Vector2 axis, Button button, bool isPressed, bool isReleased)
    {
        bool isSelected;

        RectTransform buttonTransform = button.GetComponent<RectTransform>();

        float left = Map(-1, 1, 0, 1, buttonTransform.anchorMin.x);
        float right = Map(-1, 1, 0, 1, buttonTransform.anchorMax.x);
        float top = Map(-1, 1, 0, 1, buttonTransform.anchorMin.y);
        float bottom = Map(-1, 1, 0, 1, buttonTransform.anchorMax.y);

        float width = buttonTransform.rect.width;
        float height = buttonTransform.rect.height;

        if (axis.x > left && axis.x < right)
        {
            if (axis.y > top && axis.y < bottom)
            {
                if (isPressed == true)
                    buttonTransform.localPosition += Vector3.forward * 0.001f;

                if(isReleased == true)
                    buttonTransform.localPosition += Vector3.forward * -0.001f;

                isSelected = true;
            }
            else
            {
                if (button != returnButton)
                    button.image.color = Color.white;
                isSelected = false;
            }
        }
        else
        {
            if(button != returnButton)
                button.image.color = Color.white; 
            isSelected = false;
        }
        return isSelected;
    }

    //Cursor position 
    private Vector3 CursorMapping(Vector2 axis)
    {
        var canvas = uI.transform.GetChild(0);

        //Define Canvas size
        var uiWidth = canvas.GetComponent<RectTransform>().rect.width;
        var uiHeight = canvas.GetComponent<RectTransform>().rect.height;

        //Define Cursor Position
        var x = Map(-uiWidth / 2, uiWidth / 2, -1, 1, axis.x);
        var y = Map(-uiHeight / 2, uiHeight / 2, -1, 1, axis.y);
        var z = cursor.transform.position.z;
        Vector3 cursorPositon = new Vector3(x, y, z);
       
        return cursorPositon;
    }

    public void TrackpadTouchDown(Vector2 axis)
    {
        //Set UI Cursor
        cursor.SetActive(true);
        cursor.GetComponent<RectTransform>().anchoredPosition = CursorMapping(axis);

        //UI Zoom
        //uI.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
        //uI.GetComponent<RectTransform>().anchoredPosition = new Vector2(uI.GetComponent<RectTransform>().anchoredPosition.x, 0.0326f);

        //Define current Button
        Button currentButton = ButtonSelection(axis, false, false);

        //Define touch action
        if (currentButton == toolButton)
        {
            ToolTouch();
        }
        else if (currentButton == colorButton)
        {
            if (currentColor == color.Length - 1)
                colorIcon.GetComponent<Image>().color = new Color(color[0].r, color[0].b, color[0].b);
            else
                colorIcon.GetComponent<Image>().color = color[currentColor];
                
        }
        else if (currentButton == returnButton)
        {
        }
        else
        {
            //toolIcon.GetComponent<Image>().sprite = defaultIcon;
            //colorIcon.GetComponent<Image>().color = color[currentColor];
            HideTools();
        }
        
    }

    public void TrackpadTouchUp()
    {
        //Remove UI cursor
        cursor.SetActive(false);

        //UI unZoom
        //uI.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
        //uI.GetComponent<RectTransform>().anchoredPosition = new Vector2(uI.GetComponent<RectTransform>().anchoredPosition.x, 0.0229f);

        //reset button colors
        toolButton.image.color = Color.white;
        colorButton.image.color = Color.white;
        //returnButton.image.color = Color.white;
        HideTools();

        //reset icon
        toolIcon.GetComponent<Image>().sprite = defaultIcon;
        colorIcon.GetComponent<Image>().color = color[currentColor];
    }

    //Trackpad Touch
    public void TrackpadPressDown(Vector2 axis)
    {
        //Define current button
        Button currentButton = ButtonSelection(axis, true, false);

        //Define button action
        if (currentButton == toolButton)
            ToolPressDown();
        if (currentButton == colorButton)
            ColorPressDown();
        if (currentButton == returnButton)
            ReturnPressDown();
    }

    //Trackpad Release 
    public void TrackpadPressUp(Vector2 axis)
    {
        //Define current button
        Button currentButton = ButtonSelection(axis, false, true);
    }

    //View tools onTouch
    public void ToolTouch()
    {
        //Declare tool preview
        Tools previousTool;
        Tools nextTool;

        //Set all tool previews unactive
        for (int i = 0; i < tool.Length; i++)
        {
            tool[i].head.SetActive(false);
        }

        //Set tool preview value
        if (currentTool - 1 < 0)
            previousTool = tool[tool.Length - 1];
        else
            previousTool = tool[currentTool - 1];

        if (currentTool + 1 > tool.Length - 1)
            nextTool = tool[0];
        else
            nextTool = tool[currentTool + 1];

        //Set tool preview active
        previousTool.head.SetActive(true);
        tool[currentTool].head.SetActive(true);
        nextTool.head.SetActive(true);

        //Set tool preview position
        previousTool.head.transform.localPosition = Vector3.left * 1f;
        tool[currentTool].head.transform.localPosition = Vector3.zero;
        nextTool.head.transform.localPosition = Vector3.right * 1f;

        //Set tool preview scale
        previousTool.head.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        tool[currentTool].head.transform.localScale = new Vector3(1,1,1);
        nextTool.head.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
    }

    //Change tool onPress
    private void ToolPressDown()
    {
        //Vibration
        device.TriggerHapticPulse(3999);

        //Check if last tool + next tool
        if (currentTool == tool.Length - 1)
            currentTool = 0;
        else
            currentTool += 1;

        //sound
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == "controllerChange")
                sounds[i].PlaySound();
        }

        //colorMaterial.color = color[currentColor];
    }

    //Change Color onPress
    private void ColorPressDown()
    {
        //Vibration
        device.TriggerHapticPulse(3999);

        
        colorStart = color[currentColor];

        //Check if last color + next color
        if (currentColor == color.Length - 1)
            currentColor = 0;
        else
            currentColor += 1;

        colorEnd = color[currentColor];

        //sound
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == "controllerChange")
                sounds[i].PlaySound();
        }
    }

    //Delete last Stroke
    private void ReturnPressDown()
    {
        //Vibration
        device.TriggerHapticPulse(3999);
        
        //Remove Last Brush
        if (tool[currentTool].toolType == Tools.ToolType.t3D)
        {
            var nbChild = brushContainer.transform.childCount;
            UnityEngine.Object.Destroy(brushContainer.transform.GetChild(nbChild - 1));
        }
    }

    //Mapping Function
    public float Map(float from, float to, float from2, float to2, float value)
    {
        if (value <= from2)
        {
            return from;
        }
        else if (value >= to2)
        {
            return to;
        }
        else
        {
            return (to - from) * ((value - from2) / (to2 - from2)) + from;
        }
    }

    private void HideTools()
    {
        for (int i = 0; i < tool.Length; i++)
        {
            if (i == currentTool)
                tool[i].head.SetActive(true);
            else
                tool[i].head.SetActive(false);
        }
    }
}