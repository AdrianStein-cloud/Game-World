using UnityEngine;
using UnityEngine.SceneManagement;

public class Elevator : MonoBehaviour
{
    Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void LoadScene(int level)
    {
        SceneManager.LoadSceneAsync(level);
    }

    public void Open()
    {
        animator.SetBool("Open", true);
    }

    public void Close()
    {
        animator.SetBool("Open", false);
    }
}
