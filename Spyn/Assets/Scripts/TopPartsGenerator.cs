using Unity.VisualScripting;
using UnityEngine;

public class TopPartsGenerator : MonoBehaviour
{
    [Header("Player Number")]
    [SerializeField, Range(1,2)]
    private int playerNumber = 1;

    [Header("Player Tip Materials")]
    [SerializeField] private Material p1TipMaterial;
    [SerializeField] private Material p2TipMaterial;

    [Header("Part Objects")]
    [SerializeField] private GameObject[] tops;
    [SerializeField] private GameObject[] mids;
    [SerializeField] private GameObject[] tips;

    private void Start()
    {
        ApplySelection();
    }

    private void ApplySelection()
    {
        int topIndex, midIndex, tipIndex;

        if (playerNumber == 1)
        {
            topIndex = SelectionData.P1Top;
            midIndex = SelectionData.P1Mid;
            tipIndex = SelectionData.P1Tip;
        }
        else
        {
            topIndex = SelectionData.P2Top;
            midIndex = SelectionData.P2Mid;
            tipIndex = SelectionData.P2Tip;
        }

        SetSelectedPart(tops, topIndex);
        SetSelectedPart(mids, midIndex);
        SetSelectedPart(tips, tipIndex);

        // Apply the player's colour to the selected Top and Tip.
        Material playerMaterial = playerNumber == 1
            ? p1TipMaterial
            : p2TipMaterial;

        if (tops != null && topIndex >= 0 && topIndex < tops.Length)
        {
            if (tops[topIndex] != null)
                ApplyMaterial(tops[topIndex], playerMaterial);
        }

        if (tips != null && tipIndex >= 0 && tipIndex < tips.Length)
        {
            if (tips[tipIndex] != null)
                ApplyMaterial(tips[tipIndex], playerMaterial);
        }

        SpynerController controller = GetComponent<SpynerController>();

        if (controller != null)
            controller.RefreshTip();
    }

    private void ApplyMaterial(GameObject part, Material material)
    {
        if (part == null || material == null)
            return;

        foreach (Renderer renderer in part.GetComponentsInChildren<Renderer>(true))
        {
            Material[] materials = renderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
                materials[i] = material;

            renderer.sharedMaterials = materials;
        }
    }

    private void SetSelectedPart(GameObject[] parts, int selectedIndex)
    {
          if (parts == null || parts.Length == 0)
        {
            Debug.LogError("No parts assigned on " + gameObject.name);
            return;
        }

        if (selectedIndex < 0 || selectedIndex >= parts.Length)
        {
            Debug.LogError("Invalid selection on " + gameObject.name);
            return;
        }

        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] != null)
                parts[i].SetActive(i == selectedIndex);
        }
    }

}
