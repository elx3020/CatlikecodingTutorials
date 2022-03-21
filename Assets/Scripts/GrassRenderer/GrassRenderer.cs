using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassRenderer : MonoBehaviour
{
    MaterialPropertyBlock terrainData;

    Terrain terrain;
    TerrainLayer layer;
    Material terrainMaterial;

    [SerializeField]

    Color terrainColor = new Color(1, 1, 1, 1);



    void OnValidate()
    {
        terrain = GetComponent<Terrain>();
        if (terrainData == null)
        {
            terrainData = new MaterialPropertyBlock();
        }
        terrainData = new MaterialPropertyBlock();
        terrainData.SetTexture("_TextureMap", terrain.terrainData.heightmapTexture);
        terrainData.SetColor("_BaseColor", terrainColor);
        terrainMaterial = terrain.materialTemplate;
        terrainMaterial.SetColor("_BaseColor", terrainColor);
        terrainMaterial.SetVector("_TerrainUV", new Vector4(1f, 1f, 0.4f, 1f));



    }
    void Awake()
    {
        OnValidate();

    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {


    }
}
