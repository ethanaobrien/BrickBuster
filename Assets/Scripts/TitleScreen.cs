using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

// the title / menu scene. one button per difficulty; clicking stores the choice in GameManager and loads the game
public class TitleScreen : MonoBehaviour
{
    public Button[] buttons;
    public string[] buttonDifficulties;
    public TextMeshProUGUI text;

    // sets up camera colors, button listeners, and on-screen text
    void Start()
    {
        Camera.main.backgroundColor = Color.black;
        AddButtonListeners();
        SetMessageText();
    }

    // wires each button to store its difficulty and load the game scene
    private void AddButtonListeners()
    {
        for (var i = 0; i < buttons.Length; i++)
        {
            var button = buttons[i];
            var difficulty = buttonDifficulties[i];
            button.onClick.AddListener(() =>
            {
                GameManager.Instance.SetDifficulty(difficulty);
                SceneManager.LoadScene("GameScene");
            });
        }
    }

    // shows the result of the previous game
    private void SetMessageText()
    {
        var lastMessage = GameManager.Instance.GetLastPlayMessage();
        Debug.Log(lastMessage);
        text.text = lastMessage;
    }
}
