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
using System.Runtime.InteropServices;
using System;

public class Chunk 
{
    // data that all chunks use
    public static MeshInstanceRenderer[] renderers;
    public static readonly int ChunkWidth = 32;
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

    public void Initialize(int cx, int cz, int idx, int idz)
    {
        PosX = cx;
        PosZ = cz;
       
        // 1D array containing all the block id's for the world
        NativeArray<int> blockIndices = new NativeArray<int>(ChunkWidth * ChunkHeight * ChunkWidth, Allocator.Temp);
        // make the terrain, but return number of solid blocks in the world for this chunk
        int solidBlocks = alg.setBlocks(blockIndices, PosX, PosZ);
        // now make an array with just the real blocks and not air
        // also need to keep positions of those blocks and if they're near air so
        // that they should be drawn
        NativeArray<int> solidBlockIndices = new NativeArray<int>(solidBlocks, Allocator.Temp);
        Vector3[] positions = new Vector3[solidBlocks];
        byte[] nearAir = new byte[solidBlocks];

        // calculate what should be drawn from the solid block list
        // and put all the solid blocks into the same array
        solidBlocks = 0;
        int width = ChunkWidth;
        int height = ChunkHeight;
        for (int i = 0; i < blockIndices.Length; i++)
        {
            if (blockIndices[i] != (int)BlockTypes.Air)
            {
                int x = i / (height * width);
                int y = (i - x * height * width) / width;
                int z = i - x * height * width - y * width;

                solidBlockIndices[solidBlocks] = blockIndices[i];
                positions[solidBlocks] = new Vector3(x, y, z);
                nearAir[solidBlocks] = checkAir(blockIndices, positions[solidBlocks], idx, idz);                 solidBlocks++;
            }
        }

        int countEntities = 0;
        var entityManager = World.Active.GetOrCreateManager<EntityManager>();
        int totalCount = solidBlockIndices.Length;
        for (int index = 0; index < totalCount; index++)
        {

            if (nearAir[index] == 1)
            {
                countEntities++;

                float x = positions[index].x;
                float y = positions[index].y;
                float z = positions[index].z;

                // create entity and add position and texture to it
                Entity blockEntity = entityManager.CreateEntity(GameManager.blockArchetype);
                entityManager.SetComponentData(blockEntity, new Position { Value = new float3(x + PosX, y, z + PosZ) });
                entityManager.AddSharedComponentData(blockEntity, renderers[solidBlockIndices[index]]);
            }
        }

        //GameManager.Log("number of entities created is: " + countEntities);

        blockIndices.Dispose();
        // we would keep the blocks if we were doing something other than 
        // just chucking them into the display system
        solidBlockIndices.Dispose();

    }

    private byte checkAir(NativeArray<int> blocks, Vector3 position, int idx, int idz)
    {
        byte nearAir = 0;
        int checkIndex = 0;
        int x = (int)position.x;
        int y = (int)position.y;
        int z = (int)position.z;
        int width = ChunkWidth;
        int height = ChunkHeight;
        int transparent = (int)BlockTypes.Air;

        // check top: y+1
        checkIndex = checkIndex = z + x * height * width + (y+1) * width;
        if (y+1 >= (height-1) || (blocks[checkIndex]== transparent))
        {
            nearAir = 1;
        }
        // check bottom: y-1
        checkIndex = checkIndex = z + x * height * width + (y - 1) * width;
        if (y - 1 < 0 || (blocks[checkIndex] == transparent))
        {
            nearAir = 1;
        }

        // check left: z-1
        checkIndex = checkIndex = (z - 1) + x * height * width + y * width;
        if (
            ((idz == 0) && ((z - 1) < 0)) || 
            ((z - 1) >= 0 && (blocks[checkIndex] == transparent)) )
        {
            nearAir = 1;
        }

        // check right: z+1
        checkIndex = (z + 1) + x * height * width + y * width;
        if (( (idz == VoxelWorld.chunkZ - 1) && (z + 1) >= (width - 1)) ||
            ((z + 1) <= (width - 1)) && (blocks[checkIndex] == transparent))
        {
            nearAir = 1;
        }


        // check front: x-1
        checkIndex = z + (x - 1) * height * width + y * width;
        if (
            ((idx == 0) && (x - 1) <= 0) ||
            ((x - 1) >= 0) && (blocks[checkIndex] == transparent) )
        {
            nearAir = 1;
        }

        // check back: x+1
        checkIndex = z + (x + 1) * height * width + y * width;
        if ( ((idx == VoxelWorld.chunkX - 1) && (x + 1) >= (width - 1)) ||
            ((x + 1) <= (width - 1)) && (blocks[checkIndex] == transparent))
        {
            nearAir = 1;
        }

        return nearAir;
    }


    // used as a utility to set up blocks to render
    private static MeshInstanceRenderer GetLookFromPrototype(string protoName)
    {
        var proto = GameObject.Find(protoName);
        var result = proto.GetComponent<MeshInstanceRendererComponent>().Value;
        UnityEngine.Object.Destroy(proto);
        return result;
    }

}
