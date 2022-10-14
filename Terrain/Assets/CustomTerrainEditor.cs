using UnityEngine;
using UnityEditor;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]
public class CustomTerrainEditor : Editor
{
    private SerializedProperty resetTerrain;
    private SerializedProperty randomHeightRange;
    private SerializedProperty heightMapImage;
    private SerializedProperty heightMapScale;
    private SerializedProperty perlinScaleX;
    private SerializedProperty perlinScaleY;
    private SerializedProperty perlinOffsetX;
    
    private SerializedProperty perlinOffsetY;
    private SerializedProperty perlinOctaves;
    private SerializedProperty perlinPersistence;
    private SerializedProperty perlinHeightScale;
    private GUITableState perlinParameterTable;
    private SerializedProperty perlinParameters;
    private SerializedProperty borderHeight;
    private SerializedProperty vCount;
    private SerializedProperty vfallOff;
    private SerializedProperty vdropOff;
    private SerializedProperty vMinHeight;
    private SerializedProperty vMaxHeight;  
    private SerializedProperty voronoiType;  

    private bool showRandom=false;
    private bool showLoadHeights=false;
    private bool showPerlin=false;
    private bool showMultiPerlin=false;
    private bool showVoronoi=false;
    private bool showBorder=false;
    private void OnEnable() {
        resetTerrain=serializedObject.FindProperty("resetTerrain");
        randomHeightRange=serializedObject.FindProperty("randomHeightRange");
        heightMapImage=serializedObject.FindProperty("heightMapImage");
        heightMapScale=serializedObject.FindProperty("heightMapScale");
        perlinScaleX=serializedObject.FindProperty("perlinScaleX");
        perlinScaleY=serializedObject.FindProperty("perlinScaleY");
        perlinOffsetX=serializedObject.FindProperty("perlinOffsetX");
        perlinOffsetY=serializedObject.FindProperty("perlinOffsetY");
        perlinOctaves=serializedObject.FindProperty("perlinOctaves");
        perlinPersistence=serializedObject.FindProperty("perlinPersistence");
        perlinHeightScale=serializedObject.FindProperty("perlinHeightScale");
        perlinParameterTable=new GUITableState("perlinParameterTable");
        perlinParameters=serializedObject.FindProperty("perlinParameters");
        vfallOff=serializedObject.FindProperty("fallOff");
        vdropOff=serializedObject.FindProperty("dropOff");
        vMinHeight=serializedObject.FindProperty("vMinHeight");
        vMaxHeight=serializedObject.FindProperty("vMaxHeight");
        borderHeight=serializedObject.FindProperty("borderHeight");
        vCount=serializedObject.FindProperty("varonoiCount");
        voronoiType=serializedObject.FindProperty("voronoiType");
    }
    public override void OnInspectorGUI() {
    serializedObject.Update();

    CustomTerrain terrain=(CustomTerrain)target;

    EditorGUILayout.PropertyField(resetTerrain);

    showRandom=EditorGUILayout.Foldout(showRandom,"Random");
    if(showRandom){
        //Line
        EditorGUILayout.LabelField("",GUI.skin.horizontalSlider);
        //Bold Label
        GUILayout.Label("Set Height Between Random Values",EditorStyles.boldLabel);
        //The property of Custom  Terrain
        EditorGUILayout.PropertyField(randomHeightRange);
        if(GUILayout.Button("Random Height")){
            terrain.RandomTerrain();
        }
    }
    showLoadHeights=EditorGUILayout.Foldout(showLoadHeights,"Load Heights");
    if(showLoadHeights){
        EditorGUILayout.LabelField("",GUI.skin.horizontalSlider);
        //Bold Label
        GUILayout.Label("Load Heights From Texture",EditorStyles.boldLabel);
        //The property of Custom  Terrain
        EditorGUILayout.PropertyField(heightMapImage);
        EditorGUILayout.PropertyField(heightMapScale);

        if(GUILayout.Button("Load Texture")){
            terrain.LoadTexture();
        }
    }
    showPerlin=EditorGUILayout.Foldout(showPerlin,"Single Perlin Noise");
    if(showPerlin){
        EditorGUILayout.LabelField("",GUI.skin.horizontalSlider);
        //Bold Label
        GUILayout.Label("Perlin Noise",EditorStyles.boldLabel);
        //The property of Custom  Terrain
        EditorGUILayout.Slider(perlinScaleX,0,1,new GUIContent("Scale X"));
        EditorGUILayout.Slider(perlinScaleY,0,1,new GUIContent("Scale Y"));
        EditorGUILayout.IntSlider(perlinOffsetX,0,10000,new GUIContent("Offset X"));
        EditorGUILayout.IntSlider(perlinOffsetY,0,10000,new GUIContent("Offset Y"));
        EditorGUILayout.Slider(perlinPersistence,0.1f,10,new GUIContent("Persistence"));
        EditorGUILayout.Slider(perlinHeightScale,0,1,new GUIContent("Height Scale"));
        EditorGUILayout.IntSlider(perlinOctaves,1,10,new GUIContent("Octaves"));
        if(GUILayout.Button("Perlin Noise")){
            terrain.Perlin();
        }
    }
    showMultiPerlin=EditorGUILayout.Foldout(showMultiPerlin,"Multiple Perlin Noise");
    if(showMultiPerlin){
        EditorGUILayout.LabelField("",GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("Multiple Perlin Noise",EditorStyles.boldLabel);
        perlinParameterTable=GUITableLayout.DrawTable(
            perlinParameterTable,serializedObject.FindProperty("perlinParameters"));
        EditorGUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("+"))
        terrain.AddNewPerlin();
        if(GUILayout.Button("-"))
        terrain.RemovePerlin();
        EditorGUILayout.EndHorizontal();
        if(GUILayout.Button("Apply Multiple Perlin"))
        terrain.MultiPerlin();
        
    }
    showVoronoi=EditorGUILayout.Foldout(showVoronoi,"Voronoi");
    if(showVoronoi){
        EditorGUILayout.LabelField("",GUI.skin.horizontalSlider);
        EditorGUILayout.PropertyField(voronoiType);
        EditorGUILayout.IntSlider(vCount,1,10,new GUIContent("Peak Count"));
        EditorGUILayout.Slider(vfallOff,0f,10f,new GUIContent("Falloff"));
        EditorGUILayout.Slider(vdropOff,0f,10f,new GUIContent("Dropoff"));
        EditorGUILayout.Slider(vMinHeight,0f,0.9f,new GUIContent("Min Height"));
        EditorGUILayout.Slider(vMaxHeight,vMinHeight.floatValue,1f,new GUIContent("Max Height"));
        if(GUILayout.Button("Voronoi")){
            terrain.Voronoi();
        }
    }
    showBorder=EditorGUILayout.Foldout(showBorder,"Border");
    if(showBorder) {
        EditorGUILayout.LabelField("",GUI.skin.horizontalSlider);
        EditorGUILayout.Slider(borderHeight,0,1,new GUIContent("Border Height"));
        if(GUILayout.Button("Border")){
            terrain.Border();
        }
    }
     EditorGUILayout.LabelField("",GUI.skin.horizontalSlider);
    if(GUILayout.Button("Reset Terrain"))
    {
        terrain.ResetTerrain();
    }
    serializedObject.ApplyModifiedProperties();
 }
}
