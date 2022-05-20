using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateChunk : MonoBehaviour
{
    public const int CHUNK_WIDTH = 16;
    public const int CHUNK_HEIGHT = 128;

    public BlockObject[,,] blocks = new BlockObject[CHUNK_WIDTH + 2, CHUNK_HEIGHT, CHUNK_WIDTH + 2];
    public GenerateChunk[] neighbourChunks = new GenerateChunk[4];

    public Vector2 thisChunkPos;
    public bool isGenerated = false;

    public void CreateMesh()
    {
        if (!isGenerated) return;

        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        int faceCount = 0;

        for (int x = 1; x < CHUNK_WIDTH + 1; x++)
            for (int z = 1; z < CHUNK_WIDTH + 1; z++)
                for (int y = 0; y < CHUNK_HEIGHT; y++)
                {
                    if(blocks[x,y,z] != null)
                    {
                        Vector3 blockPosition = new Vector3(x - 1, y, z -1);

                        //top
                        if(y == CHUNK_HEIGHT - 1 || blocks[x, y + 1, z] == null || blocks[x,y + 1,z].isTransparent)
                        {
                            vertices.Add(blockPosition + new Vector3(0, 1, 0));
                            vertices.Add(blockPosition + new Vector3(0, 1, 1));
                            vertices.Add(blockPosition + new Vector3(1, 1, 1));
                            vertices.Add(blockPosition + new Vector3(1, 1, 0));
                            faceCount++;

                            uvs.AddRange(blocks[x, y, z].GetUV(0));
                        }

                        //bottom
                        if (y == 0 || blocks[x, y - 1, z] == null || blocks[x, y - 1, z].isTransparent)
                        {
                            vertices.Add(blockPosition + new Vector3(0, 0, 0));
                            vertices.Add(blockPosition + new Vector3(1, 0, 0));
                            vertices.Add(blockPosition + new Vector3(1, 0, 1));
                            vertices.Add(blockPosition + new Vector3(0, 0, 1));
                            faceCount++;

                            uvs.AddRange(blocks[x, y, z].GetUV(1));
                        }
                        
                        //back
                        if(z == 0 || blocks[x, y, z - 1] == null || blocks[x, y, z - 1].isTransparent)
                        {
                            vertices.Add(blockPosition + new Vector3(0, 0, 0));
                            vertices.Add(blockPosition + new Vector3(0, 1, 0));
                            vertices.Add(blockPosition + new Vector3(1, 1, 0));
                            vertices.Add(blockPosition + new Vector3(1, 0, 0));
                            faceCount++;

                            uvs.AddRange(blocks[x, y, z].GetUV(2));
                        }

                        //right
                        if (x == CHUNK_WIDTH - 1 || blocks[x + 1, y, z] == null || blocks[x + 1, y, z].isTransparent)
                        {
                            vertices.Add(blockPosition + new Vector3(1, 0, 0));
                            vertices.Add(blockPosition + new Vector3(1, 1, 0));
                            vertices.Add(blockPosition + new Vector3(1, 1, 1));
                            vertices.Add(blockPosition + new Vector3(1, 0, 1));
                            faceCount++;

                            uvs.AddRange(blocks[x, y, z].GetUV(3));
                        }

                        //front
                        if (z == CHUNK_WIDTH - 1 || blocks[x, y, z + 1] == null || blocks[x, y, z + 1].isTransparent)
                        {
                            vertices.Add(blockPosition + new Vector3(1, 0, 1));
                            vertices.Add(blockPosition + new Vector3(1, 1, 1));
                            vertices.Add(blockPosition + new Vector3(0, 1, 1));
                            vertices.Add(blockPosition + new Vector3(0, 0, 1));
                            faceCount++;

                            uvs.AddRange(blocks[x, y, z].GetUV(4));
                        }

                        //left
                        if (x == 0 || blocks[x - 1, y, z] == null || blocks[x - 1, y, z].isTransparent)
                        {
                            vertices.Add(blockPosition + new Vector3(0, 0, 1));
                            vertices.Add(blockPosition + new Vector3(0, 1, 1));
                            vertices.Add(blockPosition + new Vector3(0, 1, 0));
                            vertices.Add(blockPosition + new Vector3(0, 0, 0));
                            faceCount++;

                            uvs.AddRange(blocks[x, y, z].GetUV(5));
                        }
                    }
                }

        int tl = vertices.Count - 4 * faceCount;
        for (int i = 0; i < faceCount; i++)
            triangles.AddRange(new int[] { tl + i * 4, tl + i * 4 + 1, tl + i * 4 + 2, tl + i * 4, tl + i * 4 + 2, tl + i * 4 + 3 });

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}