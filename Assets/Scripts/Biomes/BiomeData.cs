using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultBiome", menuName = "Blocky/New Biome")]
public class BiomeData : ScriptableObject
{
    [Header("Block Types")]
    public BlockObject surfaceBlock;
    public BlockObject belowSurfaceBlock;
    public BlockObject undergroundBlock;

    public BlockObject treeTrunk;
    public BlockObject treeLeaves;

    [Header("Biome Generation")]
    public int terrainHeight;
    [Range(1f, 3000f)]
    public float noiseOffset;
    [Range(0.01f, 0.5f)]
    public float biomeScale;
    [Range(0.01f, 3f)]
    public float terrainScale;
}
