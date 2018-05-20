using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Splatter
{
    private SprayableObject sprayableObject;
    private Controller controller;
    private Tools[] tools;
    private Color[] colors;

    private Texture2D textureTest;

    [HideInInspector]
    public LineRenderer line;

    private Vector2 lastUvPosition;
    private Vector2 uvPosition;

    private bool isStamped;
    private float size;

    private Sound[] sounds;

    public Splatter(SprayableObject _sprayableObject, Controller _controller, Tools[] _tools, Color[] _colors, Sound[] _sounds)
    {
        sprayableObject = _sprayableObject;
        controller = _controller;
        tools = _tools;
        colors = _colors;
        sounds = _sounds;

        line = controller.line.AddComponent<LineRenderer>();
        line.enabled = false;
        line.material = controller.colorMaterial;

    }
    
    public void Cursor()
    {
        if (tools[controller.currentTool].splatterType == Tools.SplatterType.Spray)
            size =4000;
        else if (tools[controller.currentTool].splatterType == Tools.SplatterType.Stamp)
            size = 1400;

        RaycastHit cursorHit;

        var origin = tools[controller.currentTool].head.transform.GetChild(0).transform;

        if (Physics.Raycast(origin.position, origin.forward, out cursorHit))
        {
            if (cursorHit.transform.tag == "SprayPaint")
            {
                line.enabled = false;
                sprayableObject.cursorMaterial.SetColor("_TintColor", colors[controller.currentColor]);

                RenderTexture.active = sprayableObject.cursorLayer;
                uvPosition = new Vector2(Map(0, sprayableObject.uvResolution.x, 0, 1, cursorHit.textureCoord.x), Map(0, sprayableObject.uvResolution.y, 0, 1, cursorHit.textureCoord.y));
                GL.Clear(true, true, Color.clear);
                GL.PushMatrix();
                GL.LoadPixelMatrix(0, sprayableObject.uvResolution.x, sprayableObject.uvResolution.y, 0);
                if (tools[controller.currentTool].texture != null)
                    Graphics.DrawTexture(new Rect(uvPosition.x - size / 2, (sprayableObject.cursorLayer.height - uvPosition.y) - size / 2, size, size), tools[controller.currentTool].texture, sprayableObject.cursorMaterial);
                GL.PopMatrix();
                RenderTexture.active = null;
            }
            else
            {
                uvPosition = Vector2.zero;
                Clear();

                if (tools[controller.currentTool].toolType == Tools.ToolType.t2D && tools[controller.currentTool].splatterType != Tools.SplatterType.Brush)
                {
                    line.enabled = true;
                    line.SetPosition(0, origin.position);
                    line.SetPosition(1, GetPointDistanceFromObject(0.1f, origin.position, cursorHit.point));
                    line.startWidth = 0.001f;
                    line.endWidth = 0.01f;

                    line.startColor = colors[controller.currentColor];
                    line.endColor = colors[controller.currentColor];
                }
                else
                {
                    line.enabled = false;
                }
            }
        }
    }

    public void Spray()
    {
        size = 4000;

        sprayableObject.splatterMaterial.SetColor("_TintColor", colors[controller.currentColor]);

        if (uvPosition != Vector2.zero)
        {
            float distance = new float();
            if (lastUvPosition != Vector2.zero)
            {
                distance = Vector2.Distance(uvPosition, lastUvPosition);
            }

            if (lastUvPosition == Vector2.zero || distance > size / 5)
            {
                /*
                for (int i = 0; i < sounds.Length; i++)
                {
                    if (sounds[i].name == "Spray")
                        sounds[i].PlaySound();
                }
                */

                RenderTexture.active = sprayableObject.paintLayer;
                GL.PushMatrix();
                GL.LoadPixelMatrix(0, sprayableObject.uvResolution.x, sprayableObject.uvResolution.y, 0);
                lastUvPosition = uvPosition;
                uvPosition.x = uvPosition.x - size / 2;
                uvPosition.y = sprayableObject.paintLayer.height - uvPosition.y - (size / 2);
                if (tools[controller.currentTool].texture != null)
                    Graphics.DrawTexture(new Rect(uvPosition.x, uvPosition.y, size, size), tools[controller.currentTool].texture, sprayableObject.splatterMaterial/* textures[controller.currentTool, controller.currentColor]*/);
                GL.PopMatrix();
                RenderTexture.active = null;
            }
        }
    }

    public void Stamp()
    {
        size = 1400;
        sprayableObject.splatterMaterial.SetColor("_TintColor", colors[controller.currentColor]);

        if (isStamped == false)
        {
            RenderTexture.active = sprayableObject.paintLayer;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, sprayableObject.uvResolution.x, sprayableObject.uvResolution.y, 0);
            lastUvPosition = uvPosition;
            uvPosition.x = uvPosition.x - size / 2;
            uvPosition.y = sprayableObject.paintLayer.height - uvPosition.y - (size / 2);
            if (tools[controller.currentTool].texture != null)
                Graphics.DrawTexture(new Rect(uvPosition.x, uvPosition.y, size, size), tools[controller.currentTool].texture, sprayableObject.splatterMaterial);
            GL.PopMatrix();
            RenderTexture.active = null;

            for (int i = 0; i < sounds.Length; i++)
            {
                if (sounds[i].name == "stamp")
                    sounds[i].PlaySound();
            }
        }

        isStamped = true;
    }

    public void StampReleased()
    {
        isStamped = false;
    }

    public void Clear()
    {
        RenderTexture.active = sprayableObject.cursorLayer;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = null;
    }

    private Vector3 GetPointDistanceFromObject(float distanceFromSurface, Vector3 toolPosition, Vector3 hitPointPosition)
    {
        Vector3 directionOfTravel = toolPosition - hitPointPosition;
        Vector3 finalDirection = directionOfTravel + directionOfTravel.normalized * distanceFromSurface;

        return hitPointPosition -finalDirection;
    }

    private float Map(float from, float to, float from2, float to2, float value)
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
}