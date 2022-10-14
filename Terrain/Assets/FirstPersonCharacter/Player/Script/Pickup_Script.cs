using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class Pickup_Script : Interactable
{   [SerializeField] private Transform parent;
    [SerializeField, Range(1f,500f)] private float force=500;
    public override void OffInteract()
    {
        gameObject.transform.parent=null;
        gameObject.GetComponent<Rigidbody>().useGravity=true;
        gameObject.GetComponent<Rigidbody>().drag=1;
    }

    public override void OnFocus()
    {
       
    }

    public override void OnInteract()
    {
        transform.parent=parent;
        GetComponent<Rigidbody>().useGravity=false;
        GetComponent<Rigidbody>().drag=10;
        if(Input.GetKey(KeyCode.Mouse0))
            ThrownObject();
    }

    public override void OnLoseFocus()
    {
        transform.parent=null;
        GetComponent<Rigidbody>().useGravity=true;
        GetComponent<Rigidbody>().drag=1;
        GetComponent<Rigidbody>().AddExplosionForce(5f,parent.transform.forward,40f,0,ForceMode.Acceleration);
    }
    private void ThrownObject(){
        transform.parent=null;
        GetComponent<Rigidbody>().useGravity=true;
        GetComponent<Rigidbody>().drag=1;
        GetComponent<Rigidbody>().AddForce(parent.transform.forward*force);
    }
}
