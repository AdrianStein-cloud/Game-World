using UnityEngine;

public class AnimatorEvents : MonoBehaviour
{
    Animator anim;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void EnableBool(string name) => anim.SetBool(name, true);
    public void DisableBool(string name) => anim.SetBool(name, false);
}
