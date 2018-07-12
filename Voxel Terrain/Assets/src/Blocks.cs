using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public struct TextureUV : IComponentData
{
    public int nameID;
    public float pixelStartX;
    public float pixelStartY;
    public float pixelEndY;
    public float pixelEndX;
}
public struct AtlasUVInfo
{
    public int Length;
    public ComponentDataArray<TextureUV> uvMaps;
}


public enum BlockTypes : int { Bedrock = 0, Dirt, Grass, Water, Air };
public class Blocks : ComponentSystem
{
    // all block information for the list of blocks:
    [Inject] public AtlasUVInfo mBlockData;
    public static string[] names;
    public static TextureUV[] textures;
    public static bool[] isTransparent;
    public static int hasInitialized = 0;
    public const int BLOCK_COUNT = 5;

    // texture atlas variables:
    public static readonly int pixelWidth = 128;
    public static readonly int pixelHeight = 128;
    public static int atlasHeight = 0;
    public static int atlasWidth = 0;
    public static Texture2D atlas;


    public TextureUV getTextureUV(string name)
    {

        if (mBlockData.Length > 0)
        {
            for (int index = 0; index < names.Length; index++)
            {
                if (names[index].Equals(name))
                {
                    return mBlockData.uvMaps[index];
                }
            }
        }
        return new TextureUV();
    }

    protected override void OnUpdate()
    {
        // there is a better way to do this!
        if (hasInitialized == 0 && mBlockData.Length > 0)
        {
            // create all the textures we'll use
            textures = new TextureUV[BLOCK_COUNT];
            isTransparent = new bool[BLOCK_COUNT];

            TextureUV tex = getTextureUV("blocks\\dirt.png");
            textures[(int)BlockTypes.Dirt] = tex;
            isTransparent[(int)BlockTypes.Dirt] = false;
            tex = getTextureUV("blocks\\bedrock.png");
            textures[(int)BlockTypes.Bedrock] = tex;
            isTransparent[(int)BlockTypes.Bedrock] = false;
            tex = getTextureUV("blocks\\glass_cyan.png");
            textures[(int)BlockTypes.Water] = tex;
            isTransparent[(int)BlockTypes.Water] = false;
            tex = getTextureUV("blocks\\grass_top.png");
            textures[(int)BlockTypes.Grass] = tex;
            isTransparent[(int)BlockTypes.Grass] = false;
            textures[(int)BlockTypes.Air] = tex;
            isTransparent[(int)BlockTypes.Air] = true;
            Debug.Log("Blocks are initialized");

            // initialize the world now that all blocks are created
            VoxelWorld.instance.Start();
            hasInitialized = 1;
        }
        else if (mBlockData.Length <= 0)
        {
            Debug.Log("Blocks are not yet initialized");
        }
    }

    public static Mesh Draw(int index)
    {
        TextureUV map = textures[index];
        Mesh d = new Mesh();

        // put together the uvmap format
        Vector2[] uvMap = new Vector2[]
        {
           // bottom
           new Vector2(map.pixelStartX, map.pixelStartY),
           new Vector2(map.pixelStartX, map.pixelEndY),
           new Vector2(map.pixelEndX, map.pixelStartY),
           new Vector2(map.pixelEndX, map.pixelEndY),
           // top
           new Vector2(map.pixelStartX, map.pixelStartY),
           new Vector2(map.pixelStartX, map.pixelEndY),
           new Vector2(map.pixelEndX, map.pixelStartY),
           new Vector2(map.pixelEndX, map.pixelEndY),
           // back
           new Vector2(map.pixelStartX, map.pixelStartY),
           new Vector2(map.pixelStartX, map.pixelEndY),
           new Vector2(map.pixelEndX, map.pixelStartY),
           new Vector2(map.pixelEndX, map.pixelEndY),
           // front
           new Vector2(map.pixelStartX, map.pixelStartY),
           new Vector2(map.pixelStartX, map.pixelEndY),
           new Vector2(map.pixelEndX, map.pixelStartY),
           new Vector2(map.pixelEndX, map.pixelEndY),
           // right
           new Vector2(map.pixelStartX, map.pixelStartY),
           new Vector2(map.pixelStartX, map.pixelEndY),
           new Vector2(map.pixelEndX, map.pixelStartY),
           new Vector2(map.pixelEndX, map.pixelEndY),
           // left
           new Vector2(map.pixelStartX, map.pixelStartY),
           new Vector2(map.pixelStartX, map.pixelEndY),
           new Vector2(map.pixelEndX, map.pixelStartY),
           new Vector2(map.pixelEndX, map.pixelEndY),
        };

        // create the cube vertices
        List<Vector3> vertices =
               new List<Vector3>()
               { // bottom face
                    new Vector3(0,0,0),
                    new Vector3(0,0,1),
                    new Vector3(1,0,0),
                    new Vector3(1,0,1),
                    // top face
                    new Vector3(0,1,0),
                    new Vector3(0,1,1),
                    new Vector3(1,1,0),
                    new Vector3(1,1,1),
                    // back face
                    new Vector3(1,0,0),
                    new Vector3(1,0,1),
                    new Vector3(1,1,0),
                    new Vector3(1,1,1),
                    // front face
                    new Vector3(0,0,0),
                    new Vector3(0,0,1),
                    new Vector3(0,1,0),
                    new Vector3(0,1,1),
                    // left face
                    new Vector3(0,0,0),
                    new Vector3(1,0,0),
                    new Vector3(0,1,0),
                    new Vector3(1,1,0),
                    // right face
                    new Vector3(0,0,1),
                    new Vector3(1,0,1),
                    new Vector3(0,1,1),
                    new Vector3(1,1,1),
               };

        // create the indices
        List<int> triangles =
              new List<int>() {
                    // bottom face
                    0,2,1,3,1,2,
                    // top face
                    0+4,1+4,2+4,3+4,2+4,1+4,
                    // back face
                    0+8,2+8,1+8,3+8,1+8,2+8,
                    // front face
                    0+12,1+12,2+12,3+12,2+12,1+12,
                    // left face
                    0+16,2+16,1+16,3+16,1+16,2+16,
                    // right face
                    0+20,1+20,2+20,3+20,2+20,1+20,

              };

        d.vertices = vertices.ToArray();
        d.triangles = triangles.ToArray();
        d.uv = uvMap;
        d.RecalculateNormals();
        d.RecalculateBounds();

        return d;
    }

    // create the atlas texture image from lots of little images
    public static void CreateAtlasComponentData(EntityManager entityManager)
    {
        names = Directory.GetFiles("blocks");

        // this assumes images are a power of 2, so it's slightly off 
        int squareRoot = Mathf.CeilToInt(Mathf.Sqrt(Blocks.names.Length));
        int squareRootH = squareRoot;
        atlasWidth = squareRoot * pixelWidth;
        atlasHeight = squareRootH * pixelHeight;
        if (squareRoot * (squareRoot - 1) > Blocks.names.Length)
        {
            squareRootH = squareRootH - 1;
            atlasHeight = squareRootH * pixelHeight;
        }

        // allocate space for the atlas and file data
        atlas = new Texture2D(atlasWidth, atlasHeight);
        byte[][] fileData = new byte[Blocks.names.Length][];

        // read the file data in parallel
        Parallel.For(0, Blocks.names.Length,
        index =>
        {
            fileData[index] = File.ReadAllBytes(Blocks.names[index]);
        });

        int x1 = 0;
        int y1 = 0;
        Texture2D temp = new Texture2D(pixelWidth, pixelHeight);
        float pWidth = (float)pixelWidth;
        float pHeight = (float)pixelHeight;
        float aWidth = (float)atlas.width;
        float aHeight = (float)atlas.height;
        Entity uvEntity;

        for (int i = 0; i < Blocks.names.Length; i++)
        {
            float pixelStartX = ((x1 * pWidth) + 1) / aWidth;
            float pixelStartY = ((y1 * pHeight) + 1) / aHeight;
            float pixelEndX = ((x1 + 1) * pWidth - 1) / aWidth;
            float pixelEndY = ((y1 + 1) * pHeight - 1) / aHeight;
            uvEntity = entityManager.CreateEntity(GameManager.atlasUVArchetype);
            entityManager.SetComponentData(uvEntity, new TextureUV
            {
                nameID = i,
                pixelStartX = pixelStartX,
                pixelStartY = pixelStartY,
                pixelEndY = pixelEndY,
                pixelEndX = pixelEndX,
            });

            temp.LoadImage(fileData[i]);
            atlas.SetPixels(x1 * pixelWidth, y1 * pixelHeight, pixelWidth, pixelHeight, temp.GetPixels());

            x1 = (x1 + 1) % squareRoot;
            if (x1 == 0)
            {
                y1++;
            }


        }

        atlas.alphaIsTransparency = true;
        atlas.Apply();
        Debug.Log("completed atlas");

        // test to make sure there's not an off by one error on images
        //File.WriteAllBytes("../atlas.png", atlas.EncodeToPNG());
    }


}
