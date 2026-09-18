using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag) || string.IsNullOrEmpty(targetSceneName)) return;
        SceneManager.LoadScene(targetSceneName);
    }
}
