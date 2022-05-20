using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifyTerrain : MonoBehaviour
{
    public GameObject blockHighlight;
    public BlockObject grassObject;

    public LayerMask ground;
    private float maxDistance = 4;

    private void Update()
    {
        bool leftMouse = Input.GetMouseButtonDown(0);
        bool rightMouse = Input.GetMouseButtonDown(1);

        RaycastHit ray;

        if(Physics.Raycast(transform.position,transform.forward,out ray, maxDistance, ground))
        {
            Vector3 targetBlock;

            if (leftMouse)
                targetBlock = ray.point + transform.forward * 0.01f;
            else
                targetBlock = ray.point - transform.forward * 0.01f;

            int chunkX = Mathf.FloorToInt(targetBlock.x ) / 16;
            int chunkZ = Mathf.FloorToInt(targetBlock.z ) / 16;

            Vector2 chunkPos = new Vector2(chunkX, chunkZ);

            GenerateChunk currentChunk = GenerateTerrain.chunks[chunkPos];

            int bix = Mathf.FloorToInt(targetBlock.x) - chunkX + 1;
            int biy = Mathf.FloorToInt(targetBlock.y);
            int biz = Mathf.FloorToInt(targetBlock.z) - chunkZ + 1;

            if(leftMouse)
            {
                currentChunk.blocks[bix, biy, biz] = null;
                currentChunk.CreateMesh();
            }
            else if (rightMouse)
            {
                currentChunk.blocks[bix, biy, biz] = grassObject;
                currentChunk.CreateMesh();
            }

            blockHighlight.SetActive(true);
            blockHighlight.transform.position = new Vector3(Mathf.FloorToInt(targetBlock.x) + 0.5f, Mathf.FloorToInt(targetBlock.y) + 0.5f, Mathf.FloorToInt(targetBlock.z) + 0.5f);
        }
        else
        {
            blockHighlight.SetActive(false);
        }
    }
}
