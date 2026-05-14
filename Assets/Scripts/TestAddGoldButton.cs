using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestAddGoldButton : MonoBehaviour
{
    private const string TotalGoldKey = "TotalGold";

    [Header("Button")]
    [SerializeField] private Button addGoldButton;

    [Header("Test Gold")]
    [SerializeField] private int goldToAdd = 100000;

    private void Start()
    {
        if (addGoldButton == null)
            addGoldButton = GetComponent<Button>();

        if (addGoldButton != null)
            addGoldButton.onClick.AddListener(AddTestGold);
    }

    private void OnDestroy()
    {
        if (addGoldButton != null)
            addGoldButton.onClick.RemoveListener(AddTestGold);
    }

    private void AddTestGold()
    {
        int currentGold = PlayerPrefs.GetInt(TotalGoldKey, 0);
        currentGold += goldToAdd;

        PlayerPrefs.SetInt(TotalGoldKey, currentGold);
        PlayerPrefs.Save();

        Debug.Log("TEST: Dodano gold. Aktualny gold: " + currentGold);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}