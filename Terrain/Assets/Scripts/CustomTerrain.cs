using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{
    [SerializeField] private bool resetTerrain;
    [SerializeField] private Vector2 randomHeightRange=new Vector2(0,0.1f);
    [SerializeField] private Texture2D heightMapImage;
    [SerializeField] private Vector3 heightMapScale=new Vector3(1,1,1);
    [Header("Parlin Noise")]
    [SerializeField] private float perlinScaleX=0.01f;
    [SerializeField] private float perlinScaleY=0.01f;
    [SerializeField] private int perlinOffsetX=0;
    [SerializeField] private int perlinOffsetY=0;
    [SerializeField] private int perlinOctaves=3;
    [SerializeField] private float perlinPersistence=8;
    [SerializeField] private float perlinHeightScale=0.09f;
    //Multiple Parlin Noise
    [System.Serializable]
    public class PerlinParametres{
      public float mPerlinScaleX=0.01f;
      public float mPerlinScaleY=0.01f;
      public int mPerlinOffsetX=0;
      public int mPerlinOffsetY=0;
      public int mPerlinOctaves=3;
      public float mPerlinPersistence=8;
      public float mPerlinHeightScale=0.09f;
      public bool remove=false;
    }
    public List<PerlinParametres> perlinParameters=new List<PerlinParametres>(){
      new PerlinParametres()
    };
    //Voronoi
    [SerializeField] private enum VoronoiType{Linear,Power,Combined,CrazySin}
    [SerializeField] private VoronoiType voronoiType=VoronoiType.Combined;
    [SerializeField] private int varonoiCount=1;
    [SerializeField] private float fallOff=0.6f;
    [SerializeField] private float dropOff=0.6f;
    [SerializeField] private float vMinHeight=0.1f;
    [SerializeField] private float vMaxHeight=0.5f;
    
    //Border
    [SerializeField] private float borderHeight=0.1f;
    private Terrain terrain;
    private TerrainData terrainData;

    private float[,] GetHeightMap(){
      if(!resetTerrain)
        return terrainData.GetHeights(0,0,terrainData.heightmapResolution,terrainData.heightmapResolution);
      else
        return new float[terrainData.heightmapResolution,terrainData.heightmapResolution];
    }
    public void RandomTerrain(){
       float[,] heightMap=GetHeightMap();
      for (int x = 0; x < terrainData.heightmapResolution; x++)
      {
        for (int y = 0; y < terrainData.heightmapResolution; y++)
        {
            heightMap[x,y]+=Random.Range(randomHeightRange.x,randomHeightRange.y);
        }
      }
      terrainData.SetHeights(0,0,heightMap);
    }
    public void LoadTexture(){
       float[,] heightMap=GetHeightMap();
      for (int x = 0; x < terrainData.heightmapResolution; x++)
      {
        for (int z = 0; z < terrainData.heightmapResolution; z++)
        {
            heightMap[x,z]+=heightMapImage.GetPixel((int)((x)*heightMapScale.x),
                                                    (int)((z)*heightMapScale.z)).grayscale*
                                                    heightMapScale.y;
        }
      }
      terrainData.SetHeights(0,0,heightMap);
    }
    public void Perlin(){
      float[,] heightMap=GetHeightMap();
      for (int x = 0; x < terrainData.heightmapResolution; x++)
      {
        for (int y = 0; y < terrainData.heightmapResolution; y++)
        {
            heightMap[x,y]+=Utils.fBM((x+perlinOffsetX)*perlinScaleX,(y+perlinOffsetY)*perlinScaleY
            ,perlinOctaves,perlinPersistence)*perlinHeightScale;
        }
      }
      terrainData.SetHeights(0,0,heightMap);
    }
    public void MultiPerlin(){
      float[,] heightMap=GetHeightMap();
      for (int x = 0; x < terrainData.heightmapResolution; x++)
      {
        for (int y = 0; y < terrainData.heightmapResolution; y++)
        {
          foreach (PerlinParametres p in perlinParameters)
          {
            heightMap[x,y]+=Utils.fBM((x+p.mPerlinOffsetX)*p.mPerlinScaleX,(y+p.mPerlinOffsetY)*p.mPerlinScaleY
            ,p.mPerlinOctaves,p.mPerlinPersistence)*p.mPerlinHeightScale;
          }
        }
      }
      terrainData.SetHeights(0,0,heightMap);
    }
    public void Voronoi(){
      
      float[,] heightMap=GetHeightMap();
      for (int i = 0; i <varonoiCount; i++)
      {
      Vector3 peak=new Vector3(Random.Range(0,terrainData.heightmapResolution),
      Random.Range(vMinHeight,vMaxHeight),Random.Range(0,terrainData.heightmapResolution));
      if(heightMap[(int)peak.x,(int)peak.z]<peak.y)
      heightMap[(int)peak.x,(int)peak.z]=peak.y;

      Vector2 peakLocation=new Vector2(peak.x,peak.z);
      float maxDistance=Vector2.Distance(new Vector2(0,0),
      new Vector2(terrainData.heightmapResolution,terrainData.heightmapResolution));
       for (int x = 0; x < terrainData.heightmapResolution; x++)
      {
        for (int y = 0; y < terrainData.heightmapResolution; y++)
        {
          if(!(x==peak.x&&y==peak.z)){
            float distanceToPeak=Vector2.Distance(peakLocation,new Vector2(x,y))/maxDistance;
            float h;
            if(voronoiType.Equals(VoronoiType.Linear))
                h=peak.y-distanceToPeak*fallOff;
            else if(voronoiType.Equals(VoronoiType.Combined))
                h=peak.y-distanceToPeak*fallOff-Mathf.Pow(distanceToPeak,dropOff);
            else if(voronoiType.Equals(VoronoiType.Power))
                h=peak.y-Mathf.Pow(distanceToPeak,dropOff)*fallOff; 
            else
                h= Mathf.Sin(distanceToPeak*100)*0.1f; //Crazy Sin
            
            if(heightMap[x,y]<h)
              heightMap[x,y]=h;
          }
        }
      }
      }
      terrainData.SetHeights(0,0,heightMap);
    }
    public void Border(){
      float[,] heightMap=GetHeightMap();
      for (int x = 0; x < terrainData.heightmapResolution; x++)
      {
      heightMap[x,0]=borderHeight;
      heightMap[x,terrainData.heightmapResolution-1]=borderHeight;
      heightMap[0,x]=borderHeight;
      heightMap[terrainData.heightmapResolution-1,x]=borderHeight;
      }

       terrainData.SetHeights(0,0,heightMap);
    }
    public void ResetTerrain(){
       float[,] heightMap=GetHeightMap();
      for (int x = 0; x < terrainData.heightmapResolution; x++)
      {
        for (int y = 0; y < terrainData.heightmapResolution; y++)
        {
            heightMap[x,y]=0;
        }
      }
      terrainData.SetHeights(0,0,heightMap);
    }
    
    public void AddNewPerlin(){
      perlinParameters.Add(new PerlinParametres());
    }
    public void RemovePerlin(){
      List<PerlinParametres> keptPerlinParameters=new List<PerlinParametres>();
      for(int i=0;i<perlinParameters.Count();i++){
        if(!perlinParameters[i].remove){
          keptPerlinParameters.Add(perlinParameters[i]);
        }
      }
      if(keptPerlinParameters.Count()==0){
        keptPerlinParameters.Add(perlinParameters[0]);
      }
      perlinParameters=keptPerlinParameters;
    }
    private void OnEnable() {
        Debug.Log("Initialising Terrain Data");
        terrain=GetComponent<Terrain>();
        terrainData=terrain.terrainData;
        if(Terrain.activeTerrain.terrainData.Equals(terrain.terrainData))
        Debug.Log("Is the same");
    }
    private void Awake() {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp=tagManager.FindProperty("tags");

        AddTag(tagsProp,"Terrain");
        AddTag(tagsProp,"Cloud");
        AddTag(tagsProp,"Shore");

        tagManager.ApplyModifiedProperties();

        this.gameObject.tag="Terrain";
    }
    private void AddTag(SerializedProperty tagsProp, string newTag){
        bool found=false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t= tagsProp.GetArrayElementAtIndex(i);
            if(t.stringValue.Equals(newTag)){found=true; break;}
        }
        if(!found){
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue=newTag;
        }
    }
}
