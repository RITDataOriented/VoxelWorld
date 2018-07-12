using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class VoxelWorld  {

    public static VoxelWorld instance = new VoxelWorld();
    private bool RanOnce = false;
    private static Chunk[,] chunks;
    public static int chunkX = 4;
    public static int chunkZ = 4;

    // Use this for initialization
    public void Start()
    {
        GameManager.Log("Initializing World Thread");
        GameManager.watch.Stop();
        GameManager.Log("Creating world at: " + GameManager.watch.ElapsedMilliseconds);
        GameManager.watch.Start();

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
                        chunks[i, j].Initialize(i * Chunk.ChunkWidth, j * Chunk.ChunkWidth, i, j);
                    }
                }

                RanOnce = true;
            }
        }
        catch (System.Exception e)
        {
            GameManager.Log(e.ToString());
        }
        GameManager.watch.Stop();
        GameManager.Log("Done with world creation at: " + GameManager.watch.ElapsedMilliseconds);

    }

    public void OnExit()
    {
        GameManager.Log("Exiting voxel world.");
        Chunk.alg.OnExit();
    }
}
