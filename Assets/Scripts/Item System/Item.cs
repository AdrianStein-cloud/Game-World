using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Item", menuName = "Item/New Item")]
public class Item : ScriptableObject
{
    public Sprite sprite;
    public string Name;

    public virtual void OnPickup() { }
    public virtual void OnDrop() { }
    public virtual void OnSelect() { } // Not setup yet
    public virtual void OnDeselect() { } // Not setup yet
    public virtual void Use() { }
}
