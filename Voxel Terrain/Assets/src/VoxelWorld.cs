using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class VoxelWorld  {

    public static VoxelWorld instance = new VoxelWorld();
    private bool RanOnce = false;
    private static Chunk[,] chunks;
    private static int chunkX = 16;
    private static int chunkZ = 16;

    // Use this for initialization
    public void Start()
    {
        GameManager.Log("Initializing World Thread");
        Debug.Log("Initializing World thread");
        try
        {
            if (!RanOnce)
            {
                // create the chunk array and global data all chunks need
                chunks = new Chunk[chunkX, chunkZ];
                Chunk.alg.InitializeBlocks(Chunk.ChunkWidth, Chunk.ChunkHeight, Chunk.ChunkWidth, 64, 28);
                Chunk.alg.InitializePerlinNoise();
                Chunk.InitEntities();

                // create each chunk
                // chunks themselves can't really be jobified since
                // they're primarily about
                // setting up the MeshInstanceRenderer's
                for (int i = 0; i < chunkX; i++)
                {
                    for (int j = 0; j < chunkZ; j++)
                    {
                        chunks[i, j] = new Chunk();
                        chunks[i, j].Initialize(i * Chunk.ChunkWidth, j * Chunk.ChunkWidth);
                    }
                }

                RanOnce = true;
            }
        }
        catch (System.Exception e)
        {
            GameManager.Log(e.ToString());
        }

    }

    public void OnExit()
    {
        GameManager.Log("Exiting voxel world.");
        Chunk.alg.OnExit();
    }
}
