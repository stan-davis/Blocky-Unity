using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultBlock", menuName = "Blocky/New Block")]
public class BlockObject : ScriptableObject
{
    public bool isSolid = true;
    public bool isTransparent = false;
    public Vector2[] blockUVPosition = new Vector2[6];

    public Vector2[] GetUV(int side)
    {
        Vector2[] uvs;
        float textureSize = 16.0f;

        uvs = new Vector2[]
        {
            new Vector2(blockUVPosition[side].x / textureSize + 0.001f, blockUVPosition[side].y / textureSize + 0.001f),
            new Vector2(blockUVPosition[side].x / textureSize+ 0.001f, (blockUVPosition[side].y + 1) / textureSize - 0.001f),
            new Vector2((blockUVPosition[side].x + 1) /textureSize - 0.001f, (blockUVPosition[side].y + 1) / textureSize - 0.001f),
            new Vector2((blockUVPosition[side].x + 1) /textureSize - 0.001f, blockUVPosition[side].y / textureSize + 0.001f),
        };

        return uvs;
    }
}