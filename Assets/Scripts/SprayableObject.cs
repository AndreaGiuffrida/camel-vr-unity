using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SprayableObject {

    public string name;

    public GameObject objectToSprayOn;
    public Material defaultLayerMaterial;
    public Material paintLayerMaterial;
    public Material cursorLayerMaterial;

    public Material cursorMaterial;
    public Material splatterMaterial;

    [HideInInspector]
    public Vector2 uvResolution;

    [HideInInspector]
    public RenderTexture cursorLayer, paintLayer;

    [HideInInspector]
    public Material[] sprayableObjectMaterial;

    public SprayableObject()
    {
        uvResolution = new Vector2(10000, 10000);
    }

    public void Init (Color[] color) {

        cursorLayer = new RenderTexture((int)uvResolution.x, (int)uvResolution.y, 32);
        paintLayer = new RenderTexture((int)uvResolution.x, (int)uvResolution.y, 32);

        sprayableObjectMaterial = new Material[3];

        for (int i = 0; i < sprayableObjectMaterial.Length; i++)
        {
            if (i == 0)
            {
                sprayableObjectMaterial[i] = defaultLayerMaterial;
                defaultLayerMaterial.color = color[Random.Range(0, color.Length -1)];
            }
            else if (i == 1)
            {
                sprayableObjectMaterial[i] = paintLayerMaterial;
                sprayableObjectMaterial[i].mainTexture = paintLayer;
            }
            else
            {
                sprayableObjectMaterial[i] = cursorLayerMaterial;
                sprayableObjectMaterial[i].mainTexture = cursorLayer;
            }
        }
        objectToSprayOn.GetComponent<MeshRenderer>().materials = sprayableObjectMaterial;
    }
}
