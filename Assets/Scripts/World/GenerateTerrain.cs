using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class GenerateTerrain : MonoBehaviour
{
    [Header("Onload Variables")]
	public Transform playerTransform;
    public GameObject chunkObject;
    public static Dictionary<Vector2, GenerateChunk> chunks = new Dictionary<Vector2, GenerateChunk>();
	List<GenerateChunk> chunkList = new List<GenerateChunk>();
	List<Vector2> chunksToCreate = new List<Vector2>();

    [Range(1,10)]
    public int renderDistance = 1;

    [Header("Terrain Generation")]
    public int seed;

    public BlockObject blockBedrock;
    public BiomeData[] biomeTypes;

    private FastNoiseLite fastNoise = new FastNoiseLite();
    private int strongestBiomeIndex = 0;

    private float averageChunkGenTime;

    private void Start()
    {
        var position = playerTransform.position;
        currentChunk = new Vector2(Mathf.FloorToInt(position.x) / GenerateChunk.CHUNK_WIDTH - 1, Mathf.FloorToInt(position.z) / GenerateChunk.CHUNK_WIDTH - 1);
        
        foreach (KeyValuePair<Vector2, GenerateChunk> chunk in chunks)
        {
            Destroy(chunk.Value.gameObject);
        }

        chunks.Clear();
    }

    Vector2 currentChunk = new Vector2(0,0);

    private void Update()
    {
        var position = playerTransform.position;
        Vector2 playerChunkPos = new Vector2(Mathf.FloorToInt(position.x) / GenerateChunk.CHUNK_WIDTH, Mathf.FloorToInt(position.z) / GenerateChunk.CHUNK_WIDTH);

        if (playerChunkPos.x != currentChunk.x || playerChunkPos.y != currentChunk.y)
        {
            currentChunk = playerChunkPos;
            LoadChunks(playerChunkPos);
        }

        for (int i = 0; i < 2; i++)
        {
            if (chunksToCreate.Count > 0)
            {
                Vector2 chunkToGen = chunksToCreate[0];
                float thisChunkTimer = Time.realtimeSinceStartup;
                GenerateChunk thisChunk = CreateChunk((int)chunkToGen.x, (int)chunkToGen.y);
                thisChunkTimer = Time.realtimeSinceStartup - thisChunkTimer;
                averageChunkGenTime += thisChunkTimer;
                averageChunkGenTime /= 2;
                chunksToCreate.Remove(chunkToGen);
            }
        }
    }

    private void LoadChunks(Vector2 playerChunkPos)
    {
        for(int i = (int)playerChunkPos.x - renderDistance; i <= (int)playerChunkPos.x + renderDistance; i++)
            for(int j = (int)playerChunkPos.y - renderDistance; j <= (int)playerChunkPos.y + renderDistance; j++)
            {
                if ((new Vector2(i, j) - new Vector2(currentChunk.x, currentChunk.y)).magnitude > renderDistance)
                    continue;

                Vector2 cp = new Vector2(i, j);
                if(!chunksToCreate.Contains(cp) && (!chunks.TryGetValue(cp, out GenerateChunk thisChunk) || !thisChunk.isGenerated))
                {
                    chunksToCreate.Add(cp);
                }
            }

        chunksToCreate.Sort(delegate(Vector2 a, Vector2 b)
        {
            int magnitudeA = Mathf.FloorToInt((new Vector2(a.x, a.y) - new Vector2((int)playerChunkPos.x, (int)playerChunkPos.y)).magnitude);
            int magnitudeB = Mathf.FloorToInt((new Vector2(b.x, b.y) - new Vector2((int)playerChunkPos.x, (int)playerChunkPos.y)).magnitude);
            return magnitudeA - magnitudeB;
        });

        List<Vector2> chunksToDestroy = new List<Vector2>();

        foreach(KeyValuePair<Vector2, GenerateChunk> c in chunks)
        {
            Vector2 cp = c.Key;

            if (Mathf.Abs(cp.x - (int)playerChunkPos.x) > renderDistance || Mathf.Abs(cp.y - (int)playerChunkPos.y) > renderDistance)
                chunksToDestroy.Add(cp);
        }

        foreach(Vector2 cpos in chunksToDestroy)
        {
            Destroy(chunks[cpos].gameObject);
            chunks.Remove(cpos);
        }
    }

    GenerateChunk CreateChunk(int chunkX, int chunkZ)
    {
        Vector2 thisChunkPos = new Vector2(chunkX, chunkZ);

        if(!chunks.TryGetValue(new Vector2(chunkX, chunkZ), out GenerateChunk thisChunkObject))
        {
            GameObject chunkPrefab = Instantiate(chunkObject, new Vector3(chunkX * GenerateChunk.CHUNK_WIDTH, 0, chunkZ * GenerateChunk.CHUNK_WIDTH), Quaternion.identity);
            chunkPrefab.transform.parent = gameObject.transform;
            chunkPrefab.name = "Chunk [" + chunkX + ", " + chunkZ + "]";
            thisChunkObject = chunkPrefab.GetComponent<GenerateChunk>();
            chunks.Add(thisChunkPos, thisChunkObject);
        }

        thisChunkObject.thisChunkPos = new Vector2(chunkX, chunkZ);

        for (int x = 0; x < GenerateChunk.CHUNK_WIDTH + 2; x++)
            for (int z = 0; z < GenerateChunk.CHUNK_WIDTH + 2; z++)
                for (int y = 0; y < GenerateChunk.CHUNK_HEIGHT; y++)
                {
                    thisChunkObject.blocks[x, y, z] = GetBlockType(x + (chunkX * GenerateChunk.CHUNK_WIDTH), y, z + (chunkZ * GenerateChunk.CHUNK_WIDTH));
                }

        /*
        if (biomeTypes[strongestBiomeIndex].treeTrunk != null)
            CreateTrees(thisChunkObject.blocks, chunkX, chunkZ);
        */

        thisChunkObject.isGenerated = true;
        thisChunkObject.CreateMesh();
        return thisChunkObject;
    }

    private BlockObject GetBlockType(int x, int y, int z)
    {
        fastNoise.SetSeed(seed);
        fastNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        //Primary terrain generation
        int groundHeight = Mathf.FloorToInt(GenerateChunk.CHUNK_HEIGHT * 0.5f);
        float strongestWeight = 0;
        int count = 0;
        float sumOfHeights = 0.0f;

        for(int i = 0; i < biomeTypes.Length; i++)
        {
            float weight = fastNoise.GetNoise((x + 0.1f) * biomeTypes[i].biomeScale + biomeTypes[i].noiseOffset, (z + 0.1f) * biomeTypes[i].biomeScale + biomeTypes[i].noiseOffset) + 0.001f;

            if(weight > strongestWeight)
            {
                strongestWeight = weight;
                strongestBiomeIndex = i;
            }

            float height = Mathf.Abs(biomeTypes[i].terrainHeight * fastNoise.GetNoise((x + 0.1f) * biomeTypes[i].terrainScale, (z + 0.1f) * biomeTypes[i].terrainScale) * weight) + 0.001f;

            if(height > 0)
            {
                sumOfHeights += height;
                count++;
            }
        }

        BiomeData biome = biomeTypes[strongestBiomeIndex];
        sumOfHeights /= count;

        float terrainHeight = sumOfHeights + groundHeight;

        BlockObject blockObject = null;

        if (y <= terrainHeight)
        {
            blockObject = biome.belowSurfaceBlock;

            if (y >= terrainHeight - 1)
            {
                blockObject = biome.surfaceBlock;
            }
            if(y <= terrainHeight - 4)
            {
                blockObject = biome.undergroundBlock;
            }
        }

        //Caves
        float caveMask = fastNoise.GetNoise(x + 0.1f, z + 0.1f) * 0.3f + 0.001f;
        float caveNoise = fastNoise.GetNoise(x + 0.1f * 4f, y + 0.1f * 8f, z + 0.1f * 4f) * 0.3f + 0.001f;

        if (caveNoise > Mathf.Max(caveMask, 0.25f))
            blockObject = null;

        if (y == 0)
            blockObject = blockBedrock;

        return blockObject;
    }

    void CreateTrees(BlockObject[,,] blocks, int x, int z)
    {
        System.Random treeRand = new System.Random(x * 10000 + z);

        float treeMap = fastNoise.GetNoise(x * 0.8f, z * 0.8f);

        if (treeMap > 0)
        {
            treeMap *= 1.5f;
            int treeFrequency = Mathf.FloorToInt((float)treeRand.NextDouble() * 5 * treeMap);

            for (int i = 0; i < treeFrequency; i++)
            {
                int treeX = (int)(treeRand.NextDouble() * 15) + 1;
                int treeZ = (int)(treeRand.NextDouble() * 15) + 1;

                int y = GenerateChunk.CHUNK_HEIGHT - 1;

                while (y > 0 && blocks[treeX, y, treeZ] == null)
                    y--;
                y++;

                //Trunk
                int trunkMaxHeight = 4;

                if (blocks[treeX, y - 1, treeZ] == biomeTypes[strongestBiomeIndex].surfaceBlock)
                {
                    for (int j = 0; j <= trunkMaxHeight; j++)
                    {
                        if (y + j < GenerateChunk.CHUNK_HEIGHT)
                        {
                            blocks[treeX, y + j, treeZ] = biomeTypes[strongestBiomeIndex].treeTrunk;
                            blocks[treeX, y + j + 1, treeZ] = biomeTypes[strongestBiomeIndex].treeLeaves;
                        }
                    }


                    int groundDistance = Random.Range(1, 2);

                    if (biomeTypes[strongestBiomeIndex].treeLeaves != null)
                    {
                        //Leaves
                        for (int k = 0; k <= trunkMaxHeight - groundDistance; k++)
                        {
                            blocks[treeX + 1, y + trunkMaxHeight + 1 - k, treeZ] = biomeTypes[strongestBiomeIndex].treeLeaves;
                            blocks[treeX - 1, y + trunkMaxHeight + 1 - k, treeZ] = biomeTypes[strongestBiomeIndex].treeLeaves;
                            blocks[treeX, y + trunkMaxHeight + 1 - k, treeZ + 1] = biomeTypes[strongestBiomeIndex].treeLeaves;
                            blocks[treeX, y + trunkMaxHeight + 1 - k, treeZ - 1] = biomeTypes[strongestBiomeIndex].treeLeaves;

                            if (k >= 1 && k <= (trunkMaxHeight - groundDistance) - 1)
                            {
                                blocks[treeX + 1, y + trunkMaxHeight + 1 - k, treeZ + 1] = biomeTypes[strongestBiomeIndex].treeLeaves;
                                blocks[treeX - 1, y + trunkMaxHeight + 1 - k, treeZ - 1] = biomeTypes[strongestBiomeIndex].treeLeaves;
                                blocks[treeX + 1, y + trunkMaxHeight + 1 - k, treeZ - 1] = biomeTypes[strongestBiomeIndex].treeLeaves;
                                blocks[treeX - 1, y + trunkMaxHeight + 1 - k, treeZ + 1] = biomeTypes[strongestBiomeIndex].treeLeaves;
                            }
                        }
                    }
                }
            }
        }
    }
}
