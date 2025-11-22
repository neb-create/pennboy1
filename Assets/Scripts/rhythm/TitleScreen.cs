using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour {
    public void QuitButton()
    {
        Debug.Log("Quitting Application");
        Application.Quit();
    }

    public void OptionsButton()
    {
        SceneManager.LoadScene("OptionsScreen");
    }

    public void SelectSongButton()
    {
        SceneManager.LoadScene("Nico's Scene Copy");
    }

    public void GachaButton()
    {
        SceneManager.LoadScene("Gacha");
    }
}
