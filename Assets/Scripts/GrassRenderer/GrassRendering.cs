using System.Collections;

using System.Collections.Generic;

using Unity.Collections;



using UnityEngine;


using UnityEngine.Rendering;


// [ExecuteInEditMode]
public class GrassRendering : MonoBehaviour

{

    public Mesh grassMesh;

    public Mesh groundMesh;

    public Terrain terrain;
    public Vector2 offset;


    public Material grassMaterial;

    public int seed;

    public int size;

    [Range(0, 20000)]
    public int grassNum;

    public int startHeight = 1000;

    ComputeBuffer pointsBuffer;


    RenderTexture grassTexture;

    Bounds bounds;

    static readonly int propertiesId = Shader.PropertyToID("_Properties"),
    propertiesMId = Shader.PropertyToID("_PropertiesM"),
    terrainLightMapId = Shader.PropertyToID("_TerrainLightMap");

    Vector4 terrainLightMap;

    MaterialPropertyBlock materialPropertyBlock;





    void Awake()
    {

        Random.InitState(seed);

        Setup();





    }


    void Setup()
    {

        bounds = new Bounds(terrain.GetPosition() + new Vector3(terrain.terrainData.size.x / 2, 0f, terrain.terrainData.size.z / 2), terrain.terrainData.size);





        // GetPositionFromTerrain();
        GetPositionFromTerrainFilter();


    }

    private struct MeshProperties
    {
        public Vector3 Position;
        public Vector3 Normal;

        // public Vector4 lightMap;
        public static int Size()
        {
            return
                sizeof(float) * 3 + // position;
                sizeof(float) * 3; // normal;
                                   // sizeof(float) * 4;   //lightmap   
        }

    }


    private struct MeshPropertiesM
    {
        public Matrix4x4 mat;
        public Vector3 Normal;
        public static int Size()
        {
            return
                sizeof(float) * 4 * 4 + // matrix;
                sizeof(float) * 3;      // normal;
        }

    }



    void GetPositionsWithRays()
    {

        MeshProperties[] properties = new MeshProperties[grassNum];
        // Vector3[] normals = new Vector3[groundMesh.normals.Length];

        for (int i = 0; i < grassNum; i++)
        {
            MeshProperties props = new MeshProperties();
            Vector3 origin = transform.position;
            Vector3 origin_normal = Vector3.zero;
            origin.y = startHeight;
            origin.x += size * Random.Range(-0.5f, 0.5f);
            origin.z += size * Random.Range(-0.5f, 0.5f);
            Ray ray = new Ray(origin, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                origin = hit.point + new Vector3(0, grassMesh.bounds.size.y / 4, 0);
                origin_normal = hit.normal;
                props.Position = origin;
                props.Normal = origin_normal;
                properties[i] = props;

            }

        }

        Debug.Log(properties[0].Normal);

        pointsBuffer = new ComputeBuffer(grassNum, MeshProperties.Size());
        // normalsBuffer = new ComputeBuffer(normals.Length, 3 * 4);
        // normalsBuffer.SetData(normals);
        pointsBuffer.SetData(properties);
        // grassMaterial.SetBuffer("_Normals", normalsBuffer);
        grassMaterial.SetBuffer("_Properties", pointsBuffer);





    }


    void GetPositionFromTerrain()
    {
        MeshProperties[] properties = new MeshProperties[grassNum];
        Vector3 origin = transform.position;
        Vector3 origin_normal = Vector3.zero;
        float terrainWidth = terrain.terrainData.size.x;
        float terrainHeight = terrain.terrainData.size.z;

        float[,,] maps = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);
        float conversionfactor = terrain.terrainData.alphamapHeight / terrainHeight;
        Debug.Log(conversionfactor);

        for (int i = 0; i < grassNum; i++)
        {

            MeshProperties instancedProps = new MeshProperties();
            origin.x = (terrainWidth / 2) + terrainWidth * Random.Range(-0.5f, 0.5f);
            origin.z = (terrainHeight / 2) + terrainHeight * Random.Range(-0.5f, 0.5f);
            origin.y = terrain.SampleHeight(origin);
            origin_normal = terrain.terrainData.GetInterpolatedNormal(origin.x / terrain.terrainData.size.x, origin.z / terrain.terrainData.size.z);
            // Debug.Log(maps[(int)(origin.x * conversionfactor), (int)(origin.z * conversionfactor), 0]);
            instancedProps.Position = origin;
            instancedProps.Normal = origin_normal;
            properties[i] = instancedProps;
            // Debug.DrawRay(properties[i].Position, properties[i].Normal, Color.blue, 10f);
        }
        // Debug.Log(properties[0].Normal);
        pointsBuffer = new ComputeBuffer(grassNum, MeshProperties.Size());
        pointsBuffer.SetData(properties);
        grassMaterial.SetBuffer(propertiesId, pointsBuffer);
        terrainLightMap = terrain.realtimeLightmapScaleOffset;



    }


    void GetPositionFromTerrainFilter()
    {
        MeshProperties[] properties = new MeshProperties[grassNum];
        Vector3 origin = transform.position;
        Vector3 origin_normal = Vector3.zero;
        float terrainWidth = terrain.terrainData.size.x;
        float terrainHeight = terrain.terrainData.size.z;

        float[,,] maps = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);
        float conversionfactor = terrain.terrainData.alphamapHeight / terrainHeight;

        for (int i = 0; i < grassNum; i++)
        {

            MeshProperties instancedProps = new MeshProperties();
            origin.x = (terrainWidth / 2) + terrainWidth * Random.Range(-0.5f, 0.5f);
            origin.z = (terrainHeight / 2) + terrainHeight * Random.Range(-0.5f, 0.5f);
            if (maps[(int)(origin.z * conversionfactor), (int)(origin.x * conversionfactor), 0] > 0.5f)
            {
                origin.y = terrain.SampleHeight(origin);
                origin_normal = terrain.terrainData.GetInterpolatedNormal(origin.x / terrain.terrainData.size.x, origin.z / terrain.terrainData.size.z);
                // Debug.Log(maps[(int)(origin.x * conversionfactor), (int)(origin.z * conversionfactor), 0]);
                instancedProps.Position = origin;
                instancedProps.Normal = origin_normal;
                properties[i] = instancedProps;
                Debug.DrawRay(properties[i].Position, properties[i].Normal, Color.blue, 10f);
            }


            // 
        }
        // Debug.Log(properties[0].Normal);
        pointsBuffer = new ComputeBuffer(grassNum, MeshProperties.Size());
        pointsBuffer.SetData(properties);
        grassMaterial.SetBuffer(propertiesId, pointsBuffer);
        terrainLightMap = terrain.realtimeLightmapScaleOffset;



    }







    void GetPositionFromTerrainMatrix()
    {
        MeshPropertiesM[] properties = new MeshPropertiesM[grassNum];
        Vector3 origin = transform.position;
        Vector3 origin_normal = Vector3.zero;

        for (int i = 0; i < grassNum; i++)
        {

            MeshPropertiesM instancedProps = new MeshPropertiesM();
            origin.x = transform.position.x + size * Random.Range(-0.5f, 0.5f);
            origin.z = transform.position.z + size * Random.Range(-0.5f, 0.5f);
            origin.y = terrain.SampleHeight(origin);
            origin_normal = terrain.terrainData.GetInterpolatedNormal(origin.x / terrain.terrainData.size.x, origin.z / terrain.terrainData.size.z);
            instancedProps.mat = Matrix4x4.TRS(origin, Quaternion.identity, Vector3.one);
            instancedProps.Normal = origin_normal;
            properties[i] = instancedProps;
            // Debug.DrawRay(properties[i].Position, properties[i].Normal, Color.blue, 10f);
        }
        pointsBuffer = new ComputeBuffer(grassNum, MeshPropertiesM.Size());
        pointsBuffer.SetData(properties);
        grassMaterial.SetBuffer(propertiesMId, pointsBuffer);



    }

    private void Update()

    {

        GetPositionFromTerrainFilter();
        if (materialPropertyBlock == null)
        {
            materialPropertyBlock = new MaterialPropertyBlock();
            materialPropertyBlock.SetVector(terrainLightMapId, terrainLightMap);
        }
        bounds = new Bounds(transform.position, Vector3.one * (size));
        grassMaterial.SetVector("_TextureOffset", offset);
        // grassMaterial.SetVector("_GrassOffset", transform.position);
        Graphics.DrawMeshInstancedProcedural(grassMesh, 0, grassMaterial, bounds, pointsBuffer.count, materialPropertyBlock, ShadowCastingMode.Off);


    }



    private void OnDisable()
    {
        if (pointsBuffer != null)
        {
            pointsBuffer.Release();
        }



        pointsBuffer = null;


    }

}