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
    [SerializeField] private enum VoronoiType{Linear,Power,Combined,SinPow,CrazySin}
    [SerializeField] private VoronoiType voronoiType=VoronoiType.Combined;
    [SerializeField] private int varonoiCount=1;
    [SerializeField] private float fallOff=0.6f;
    [SerializeField] private float dropOff=0.6f;
    [SerializeField] private float vMinHeight=0.1f;
    [SerializeField] private float vMaxHeight=0.5f;
    
    //Border
    [SerializeField] private float borderHeight=0.1f;
    //MPD
    [SerializeField] private float MDMinHeight=-2f;
    [SerializeField] private float MDMaxHeight=2f;
    [SerializeField] private float MDRoughness=2.0f;
    [SerializeField] private float MDHeightDampener=2f;
    //Smooth
    [SerializeField] private int smoothAmmout=2;
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
            else if(voronoiType.Equals(VoronoiType.SinPow)){
                h=peak.y-Mathf.Pow(distanceToPeak*3,fallOff)-Mathf.Sin(distanceToPeak*Mathf.PI)/dropOff;
            }
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
    public void MidPointDisplacement(){
      float[,] heightMap=GetHeightMap();
      int width=terrainData.heightmapResolution-1;
      int squareSize=width;

      float minHeight=MDMinHeight;
      float maxHeight=MDMaxHeight;
      float heightDampener=Mathf.Pow(MDHeightDampener,-1*MDRoughness);
      
      int cornerX,cornerY;
      int midX,midY;
      int pmidXL,pmidXR,pmidYU,pmidYD;
      
      /*heightMap[0,0]=Random.Range(0f,0.2f);
      heightMap[0,terrainData.heightmapResolution-2]=Random.Range(0.0f,0.2f);
      heightMap[terrainData.heightmapResolution-2,0]=Random.Range(0.0f,0.2f);
      heightMap[terrainData.heightmapResolution-2,terrainData.heightmapResolution-2]
      =Random.Range(0.0f,0.2f);*/

    while(squareSize>0){
      for(int x=0;x<width;x+=squareSize){
        for(int y=0;y<width;y+=squareSize){
          cornerX=(x+squareSize);
          cornerY=(y+squareSize);

          midX=x+squareSize/2;
          midY=y+squareSize/2;

           heightMap[midX,midY]=(heightMap[x,y]+heightMap[cornerX,y]+heightMap[x,cornerY]+
           heightMap[cornerX,cornerY])/4f+Random.Range(minHeight,maxHeight);
          }
        }

        for(int x=0;x<width;x+=squareSize){
          for(int y=0;y<width;y+=squareSize){
            cornerX=(x+squareSize);
            cornerY=(y+squareSize);
            
            midX=x+squareSize/2;
            midY=y+squareSize/2;

            pmidXR=midX+squareSize;
            pmidYU=midY+squareSize;
            pmidXL=midX-squareSize;
            pmidYD=midY-squareSize;

            if(pmidXL<=0||pmidYD<=0||pmidXR>=width-1||pmidYU>=width-1)
              continue;

            heightMap[midX,y]=(heightMap[x,y]+heightMap[midX,midY]
            +heightMap[cornerX,y]+heightMap[midX,pmidYD])/4f+Random.Range(minHeight,maxHeight);
            
            heightMap[x,midY]=(heightMap[x,y]+heightMap[midX,midY]
            +heightMap[x,cornerY]+heightMap[pmidXL,midY])/4f+Random.Range(minHeight,maxHeight);

            heightMap[midX,cornerY]=(heightMap[cornerX,cornerY]+heightMap[midX,midY]
            +heightMap[x,cornerY]+heightMap[midX,pmidYU])/4f+Random.Range(minHeight,maxHeight);

             heightMap[cornerX,midY]=(heightMap[cornerX,cornerY]+heightMap[midX,midY]
            +heightMap[cornerX,y]+heightMap[pmidXR,midY])/4f+Random.Range(minHeight,maxHeight);

          }
        }
        squareSize/=2;
        minHeight*=heightDampener;
        maxHeight*=heightDampener;
      
      terrainData.SetHeights(0,0,heightMap);
      }
    }
    List<Vector2> GerenateNeighbours(Vector2 pos,int width,int height){
      List<Vector2> neighbours=new List<Vector2>();
      for(int y=-1;y<2;y++){
        for(int x=-1;x<2;x++){
            if(!(x==0&&y==0)){
                Vector2 nPos =new Vector2(Mathf.Clamp(pos.x+x,0,width-1),
                Mathf.Clamp(pos.y+y,0,height-1));
                if(!neighbours.Contains(nPos)){
                  neighbours.Add(nPos);
                }
            }
        }
      }
       return neighbours;
    }
    public void Smooth(){
      float[,] heightMap=terrainData.GetHeights(0,0,terrainData.heightmapResolution,terrainData.heightmapResolution);
      float smoothProgress=0;
      EditorUtility.DisplayProgressBar("Smooth Terrain","Progress",smoothProgress
      );

      int s =0;
      while(s<smoothAmmout){
      for (int x = 0; x < terrainData.heightmapResolution; x++)
      {
        for (int y = 0; y < terrainData.heightmapResolution; y++)
        {
          float avgHeight=heightMap[x,y];
          List<Vector2> neighbours=GerenateNeighbours(new Vector2(x,y),
          terrainData.heightmapResolution,terrainData.heightmapResolution);

        foreach(Vector2 n in neighbours){
          avgHeight+=heightMap[(int)n.x,(int)n.y];
        }
        heightMap[x,y]=avgHeight/((float)neighbours.Count+1);
        }
      }
      s++;
      smoothProgress++;
      EditorUtility.DisplayProgressBar("Smooth Terrain","Progress"
      ,smoothProgress/smoothAmmout);
      }
      terrainData.SetHeights(0,0,heightMap);
      EditorUtility.ClearProgressBar();
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
