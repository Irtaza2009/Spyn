using UnityEngine;

public class EliminationBoundary : MonoBehaviour
{
    [SerializeField] private MatchManager matchManager;

    private void OnTriggerEnter(Collider other)
    {
        SpynerController fallenPlayer = other.GetComponentInParent<SpynerController>();

        if (fallenPlayer != null)
        {
            matchManager.PlayerFell(fallenPlayer);
        }
    }
}
