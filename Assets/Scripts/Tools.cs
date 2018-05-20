using System;
using UnityEngine;


[Serializable]
public class Tools {

    public int id;
    public string name;

    public GameObject head;

    public ToolType toolType;
    public enum  ToolType { t3D, t2D }

    public SplatterType splatterType;
    public enum SplatterType { Brush, Spray, Stamp }

    public Texture2D texture;
    public Sprite icon;
}
