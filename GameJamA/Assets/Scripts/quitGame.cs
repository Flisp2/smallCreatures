using UnityEngine;

public class quitGame : MonoBehaviour
{
    public void Quit()
    {
        Debug.Log("Quitting the game.");
        Application.Quit();
    }
}
