using System.Collections.Generic;
using UnityEngine;

public class ChemistrySet : MonoBehaviour, ILockChecker
{
    [SerializeField] GameObject powder;
    [SerializeField] MeshRenderer glass1;
    [SerializeField] MeshRenderer glass2;
    [SerializeField] MeshRenderer glass3;
    [SerializeField] List<Item> requiredItems;

    Material liquid1;
    Material liquid2;
    Material liquid3;

    bool mixed1;
    bool mixed2;
    bool mixed3;

    private void Start()
    {
        powder.SetActive(false);
        liquid1 = glass1.materials[1];
        liquid2 = glass2.materials[1];
        liquid3 = glass3.materials[1];
        liquid1.DisableKeyword("_EMISSION");
        liquid2.DisableKeyword("_EMISSION");
        liquid3.DisableKeyword("_EMISSION");
    }

    public void Mix()
    {
        if (!mixed1 && PlayerSimpleInventory.Instance.HoldsItem(requiredItems[0]))
        {
            mixed1 = true;
            powder.SetActive(true);
        }

        if (!mixed2 && PlayerSimpleInventory.Instance.HoldsItem(requiredItems[1]))
        {
            mixed2 = true;
            liquid1.EnableKeyword("_EMISSION");
        }

        if (!mixed3 && PlayerSimpleInventory.Instance.HoldsItem(requiredItems[2]))
        {
            mixed3 = true;
            liquid2.EnableKeyword("_EMISSION");
        }

        if (mixed1 && mixed2 && mixed3)
        {
            liquid3.EnableKeyword("_EMISSION");
        }
    }

    public bool IsLocked()
    {
        return !mixed1 || !mixed2 || !mixed3;
    }
}
