using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioClip musicClip;
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (musicSource != null && musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }
    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void openSettings()
    {
        
    }
}
