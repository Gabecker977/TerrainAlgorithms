using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TesteInteraction : Interactable
{   MeshRenderer meshRenderer;    
    [SerializeField]private Color[] colors;
    [SerializeField,Range(0f,1f)] private float lerpTime;
    private int indexColor=0;
    private void Start() {
      meshRenderer=GetComponent<MeshRenderer>();
    }
    public override void OffInteract()
    {
        
    }

    public override void OnFocus()
    {
        
    }

    public override void OnInteract()
    {
        ChangeColors();
    }

    public override void OnLoseFocus()
    {
       
    }
    float t=0;
    private void ChangeColors(){
        meshRenderer.material.color=Color.Lerp(meshRenderer.material.color,colors[indexColor],lerpTime*Time.deltaTime);

        t=Mathf.Lerp(t,1,lerpTime*Time.deltaTime);
        if(t>.9f){
            t=0f;
            indexColor++;
            indexColor=(indexColor>=colors.Length)? 0: indexColor;
        }
    }

}
