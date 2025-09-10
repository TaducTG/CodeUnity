using UnityEngine;

public class OptionUIManager : MonoBehaviour
{
    public GameObject OptionPanel;
    private bool isOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleOption();
        }
    }

    public void ToggleOption()
    {
        isOpen = !isOpen;
        OptionPanel.SetActive(isOpen);

        if (isOpen)
        {
            Time.timeScale = 0f; // dừng game
        }
        else
        {
            Time.timeScale = 1f; // tiếp tục game
        }
    }
}