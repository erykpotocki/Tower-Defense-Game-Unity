using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void LoadShop()
    {
        SceneManager.LoadScene("ShopScene");
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("MainActivity");
    }
}