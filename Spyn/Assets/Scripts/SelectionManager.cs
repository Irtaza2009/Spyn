using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;

public class SelectionManager : MonoBehaviour
{
    [Header("Preview Positions")]
    [SerializeField] private Transform p1TopPreview;
    [SerializeField] private Transform p1MidPreview;
    [SerializeField] private Transform p1TipPreview;
    [SerializeField] private Transform p2TopPreview;
    [SerializeField] private Transform p2MidPreview;
    [SerializeField] private Transform p2TipPreview;

    [Header("Player Tip Materials")]
    [SerializeField] private Material p1TipMaterial;
    [SerializeField] private Material p2TipMaterial;

    [Header("Prefabs")]
    [SerializeField] private GameObject[] tops;
    [SerializeField] private GameObject[] mids;
    [SerializeField] private GameObject[] tips;

    [Header("Battle Scene")]

    private int[] choices = new int[6];

    private Transform[] previews;

    private void Awake()
    {
        previews = new Transform[]
        {
            p1TopPreview, p1MidPreview, p1TipPreview,
            p2TopPreview, p2MidPreview, p2TipPreview
        };
    }

    private void Start()
    {
        for (int i = 0; i < 6; i++)
            ShowPreview(i);
    }

    //P1 Thingies
    public void P1PreviousTop() => ChangePart(0, tops, -1);
    public void P1NextTop()
    {
        Debug.Log("Player 1: Next Top button clicked!");
        ChangePart(0, tops, 1);
    } 

    public void P1PreviousMid() => ChangePart(1, mids, -1);
    public void P1NextMid() => ChangePart(1, mids, 1);

    public void P1PreviousTip() => ChangePart(2, tips, -1);
    public void P1NextTip() => ChangePart(2, tips, 1);

    //P2 Thingies

    public void P2PreviousTop() => ChangePart(3, tops, -1);
    public void P2NextTop() => ChangePart(3, tops, 1);

    public void P2PreviousMid() => ChangePart(4, mids, -1);
    public void P2NextMid() => ChangePart(4, mids, 1);

    public void P2PreviousTip() => ChangePart(5, tips, -1);
    public void P2NextTip() => ChangePart(5, tips, 1);

    private void ChangePart(int slot, GameObject[] parts, int direction)
    {
        if (parts == null || parts.Length == 0) return;

        choices[slot] = (choices[slot] + direction + parts.Length) % parts.Length;

        ShowPreview(slot);
    }

    private void ShowPreview(int slot)
    {
        GameObject[] parts =
            slot % 3 == 0 ? tops :
            slot % 3 == 1 ? mids : tips;

        if (parts == null || parts.Length == 0)
            return;

        if (previews[slot] == null)
            return;

        for (int i = previews[slot].childCount - 1; i >= 0; i--)
            Destroy(previews[slot].GetChild(i).gameObject);

        GameObject preview = Instantiate(parts[choices[slot]], previews[slot]);

        preview.transform.localPosition = Vector3.zero;
        preview.transform.localRotation = Quaternion.identity;
        preview.transform.localScale = Vector3.one;

        // Recolour Top and Tip previews, but leave Mid unchanged.
        if (slot % 3 == 0 || slot % 3 == 2)
        {
            Material material = slot < 3 ? p1TipMaterial : p2TipMaterial;
            ApplyMaterial(preview, material);
        }
    }

    private void ApplyMaterial(GameObject part, Material material)
    {
        if (material == null)
            return;

        foreach (Renderer renderer in part.GetComponentsInChildren<Renderer>(true))
        {
            // Replace all material slots on the tip's renderers.
            Material[] materials = renderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
                materials[i] = material;

            renderer.sharedMaterials = materials;
        }
    }

    public void StartBattle ()
    {
        SelectionData.P1Top = choices[0];
        SelectionData.P1Mid = choices[1];
        SelectionData.P1Tip = choices[2];

        SelectionData.P2Top = choices[3];
        SelectionData.P2Mid = choices[4];
        SelectionData.P2Tip = choices[5];

        int NextSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(NextSceneIndex);
        
    }

}
