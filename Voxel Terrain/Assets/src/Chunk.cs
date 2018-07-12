using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Profiling;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms2D;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;

public class Chunk 
{
    // data that all chunks use
    public static MeshInstanceRenderer[] renderers;
    public static readonly int ChunkWidth = 8;
    public static readonly int ChunkHeight =128;
    public static NoiseAlgorithm alg = new NoiseAlgorithm();

    // start position in the world of this particular chunk
    private int PosX;
    private int PosZ;

    
    public static void InitEntities()
    {

        // can't jobify since this is all about setting up the MeshInstanceRenderer's
        renderers = new MeshInstanceRenderer[Blocks.BLOCK_COUNT];
        for (int i = 0; i < Blocks.BLOCK_COUNT; i++)
        {
            renderers[i] = GetLookFromPrototype("blockPrototype");
            renderers[i].material.mainTexture = Blocks.atlas;
            renderers[i].mesh = Blocks.Draw(i);
        }
    }

    public void Initialize(int cx, int cz)
    {
        PosX = cx;
        PosZ = cz;
        GameManager.Log("Starting chunk");
        NativeArray<int> blockIndices = new NativeArray<int>(ChunkWidth * ChunkHeight * ChunkWidth, Allocator.Temp);

        //UnityEngine.Profiling.Profiler.BeginSample("My Sample");
        alg.setBlocks(blockIndices, PosX, PosZ);
        //UnityEngine.Profiling.Profiler.EndSample();

        var entityManager = World.Active.GetOrCreateManager<EntityManager>();

        // [TO DO] note: turns out we need to cut down on MeshInstanceRenderers
        // so I'm grouping the vertices/etc. of each chunk together and
        // batching it, but it doesn't seem like a final solution
        // can't really jobify this since all it does is
        // put the components with the right renderer into the entity system
        // and the renderers are MeshInstanceRender's
        //for (int index = 0; index < ChunkWidth * ChunkHeight * ChunkWidth; index++)
        //{
        // int x = index / (ChunkHeight * ChunkWidth);
        // int y = (index - x * ChunkHeight * ChunkWidth) / ChunkWidth;
        // int z = index - x * ChunkHeight * ChunkWidth - y * ChunkWidth;

        //if (!(blockIndices[index] == (int)BlockTypes.Air))
        // {
        //     // Access the ECS entity manager

        //     // create entity and add position and texture to it
        //     Entity blockEntity = entityManager.CreateEntity(GameManager.blockArchetype);
        //     entityManager.SetComponentData(blockEntity, new Position { Value = new float3(x + PosX, y, z + PosZ) });
        //     //if ( x == 5 ) 
        //     //    entityManager.SetComponentData(blockEntity, new MeshCulledComponent {  });

        //     entityManager.AddSharedComponentData(blockEntity, renderers[blockIndices[index]]);
        // }
        //}

        // NOTE: CHUNKWIDTH IS SMALL SO WE DON'T TRY TO COMBINE TOO MANY
        // VERTICES FOR ONE CHUNK
        MeshInstanceRenderer combinationRenderer =  GetLookFromPrototype("blockPrototype");
        combinationRenderer.material.mainTexture = Blocks.atlas;
        combinationRenderer.mesh = CombineMeshes(blockIndices);
        Entity blockEntity = entityManager.CreateEntity(GameManager.blockArchetype);
        entityManager.SetComponentData(blockEntity, new Position { Value = new float3(PosX, 0, PosZ) });
        entityManager.AddSharedComponentData(blockEntity, combinationRenderer);

        blockIndices.Dispose();

    }

    private static MeshInstanceRenderer GetLookFromPrototype(string protoName)
    {
        var proto = GameObject.Find(protoName);
        var result = proto.GetComponent<MeshInstanceRendererComponent>().Value;
        Object.Destroy(proto);
        return result;
    }

    private static Mesh CombineMeshes(NativeArray<int> blocks)
    {
        Mesh d = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvMaps = new List<Vector2>();
        
        for (int index = 0; index < ChunkWidth * ChunkHeight * ChunkWidth; index++)
        {
            int x = index / (ChunkHeight * ChunkWidth);
            int y = (index - x * ChunkHeight * ChunkWidth) / ChunkWidth;
            int z = index - x * ChunkHeight * ChunkWidth - y * ChunkWidth;


            if (!(blocks[index] == (int)BlockTypes.Air))
            {
                Mesh currentMesh = renderers[blocks[index]].mesh;
                int count = vertices.Count;
                for (int locIndex = 0; locIndex < currentMesh.vertices.Length; locIndex++)
                {
                    vertices.Add(currentMesh.vertices[locIndex] + new Vector3(x, y, z));
                }

                for (int i = 0; i < currentMesh.triangles.Length; i++)
                {
                    triangles.Add(currentMesh.triangles[i] + count);
                }

                uvMaps.AddRange(currentMesh.uv);
            }
        }

        d.vertices = vertices.ToArray();
        d.triangles = triangles.ToArray();
        d.uv = uvMaps.ToArray();
        d.RecalculateNormals();
        d.RecalculateBounds();

        return d;
    }
    
}
