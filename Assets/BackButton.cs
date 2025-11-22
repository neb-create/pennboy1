using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
