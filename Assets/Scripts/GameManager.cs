using UnityEngine;

// survives scene loads. carries the chosen difficulty and last-game result between scenes
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // this is called before any other Start, so we can make sure Instance is set here
    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private string _difficulty = "Easy";
    private string _lastPlay = "";

    // the difficulty chosen on the title screen: "Easy", "Medium", "Hard" or "Expert"
    public string GetDifficulty()
    {
        return _difficulty;
    }

    // sets the difficulty for the next game
    public void SetDifficulty(string difficulty)
    {
        _difficulty = difficulty;
    }

    // win/loss message from the last game, shown again on the title screen
    public string GetLastPlayMessage()
    {
        return _lastPlay;
    }

    // sets the message from the last game
    public void SetLastPlayMessage(string lastPlay)
    {
        _lastPlay = lastPlay;
    }
}
