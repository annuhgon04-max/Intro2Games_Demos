// Spawn point example written with the support of GitHub Copilot.
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool instantiatePlayerIfMissing;
    [SerializeField] private GameObject playerPrefab;

    private void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            if (!instantiatePlayerIfMissing || playerPrefab == null)
            {
                Debug.LogWarning($"SpawnPoint could not find a player with the tag '{playerTag}'.", this);
                return;
            }

            player = Instantiate(playerPrefab, transform.position, transform.rotation);
        }

        // Place the player at this point before the scene begins.
        player.transform.position = transform.position;
    }
}