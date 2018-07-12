using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Transforms2D;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Collections;
using System.Text;
using System;
using Unity.Jobs;
using System.Diagnostics;

public class GameManager : ComponentSystem {

    public static MeshInstanceRenderer blockRenderer;
    public static EntityManager entityManager;
    public static EntityArchetype blockArchetype;
    public static EntityArchetype atlasUVArchetype;
    public static StringBuilder mainLogText = new StringBuilder();
    private static int logTextAdded = 0;
    public static Stopwatch watch;

    // Use this for initialization
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize() { 
        entityManager = World.Active.GetOrCreateManager<EntityManager>();
        watch = Stopwatch.StartNew();

        // archetype for all the cube textures
        atlasUVArchetype = entityManager.CreateArchetype(typeof(TextureUV));
        Blocks.CreateAtlasComponentData(entityManager);

        // a block archetype
        blockArchetype = entityManager.CreateArchetype(
            typeof(Position), typeof(TransformMatrix));

    }

    public static void Log(string text)
    {
        mainLogText.Append(text + Environment.NewLine);
        logTextAdded = 1;
    }

    protected override void OnUpdate() {
        doFileWriteJob();

    }

    protected override void OnStopRunning()
    {

        // initialize the world now that all blocks are created
        VoxelWorld.instance.OnExit();
        // this should be last to catch all potential log messages
        doFileWriteJob();

    }

    private void doFileWriteJob()
    {
        if (logTextAdded == 1)
        {
            // schedule the file write job
            LogToFile jobData = new LogToFile();
            jobData.logStr = new NativeArray<byte>(mainLogText.Length, Allocator.Temp);
            jobData.logStr.CopyFrom(Encoding.ASCII.GetBytes(mainLogText.ToString()));
            JobHandle handle = jobData.Schedule();
            handle.Complete();
            jobData.logStr.Dispose();
            logTextAdded = 0;
        }
    }
}
