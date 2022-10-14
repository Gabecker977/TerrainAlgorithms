using UnityEngine;
public abstract class Interactable : MonoBehaviour
{
    public virtual void Awake() {
        gameObject.layer = 11;
        
    }
    public abstract void OnInteract();
    public abstract void OffInteract();
    public abstract void OnFocus();
    public abstract void OnLoseFocus();
}
