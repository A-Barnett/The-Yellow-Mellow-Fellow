using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private string highScoreFile = "scores.txt";
    [SerializeField] private GameObject retroPostProcessing;
    [SerializeField] private GameObject mainPostProcessing;
    [SerializeField] private Toggle retroToggle;
    [SerializeField] private Toggle darkToggle;
    [SerializeField] private Toggle twoPlayerToggle;
    [SerializeField] private GameObject retroLight;
    [SerializeField] private Material normalWallMat;
    [SerializeField] private Material darkWallMat;
    [SerializeField] private GameObject walls;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TextMeshProUGUI placeholderText;
    [SerializeField] private GameObject movementInst;
    private YellowFellowGame gameController;
    private bool darkLock;
    private bool retroLock;
    
    void Start()
    {
        gameController = GetComponentInParent<YellowFellowGame>();
        levelText.text = "Level " + PlayerPrefs.GetInt("Level", 1);
        twoPlayerToggle.isOn = PlayerPrefs.GetInt("TwoPlayer")==1;
        StyleChange();
    }

    public void StartButton()
    {
        gameController.StartGame();
    }
    public void LeaderboardButton()
    {
        gameController.StartHighScores();
    }
    public void ExitLeaderboardButton()
    {
        gameController.StartMainMenu();
    }
    public void ExitGameButton()
    {
        if (Application.isEditor)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
        else
        {
            Application.Quit();
        }
    }

    public void ReturnMainMenu()
    {
        PlayerPrefs.SetInt("Died",0);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void DeathInput()
    {
        string playerName = nameInput.text;
        if (playerName.Length > 8 )
        {
            nameInput.text = "";
            placeholderText.text = "Less than 9 characters";
        }else if (playerName.Length < 1)
        {
            nameInput.text = "";
        }
        else
        {
            PlayerPrefs.SetInt("Died",1); 
            using (StreamWriter writer = File.AppendText(highScoreFile)) 
            { 
                writer.WriteLine(playerName + " "+ PlayerPrefs.GetInt("Score"));
            }
            PlayerPrefs.SetInt("Score",0);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
        }
    }
    public void TwoPlayersToggle(bool modeOn)
    {
        PlayerPrefs.SetInt("TwoPlayer",modeOn? 1 : 0);
        gameController.ToggleTwoPlayer(modeOn);
        movementInst.SetActive(modeOn);
    }

    public void RetroModeToggle(bool modeOn)
    {
        if (!retroLock)
        {
            PlayerPrefs.SetInt("Style", modeOn ? 1 : 0);
            StyleChange();
        }
    }

    public void DarkModeToggle(bool modeOn)
    {
        if (!darkLock)
        {
            PlayerPrefs.SetInt("Style", modeOn ? 2 : 0);
            StyleChange();
        }
    }

    private void StyleChange()
    {
        darkLock = true;
        retroLock = true;
        retroToggle.isOn = PlayerPrefs.GetInt("Style") == 1;
        darkToggle.isOn = PlayerPrefs.GetInt("Style") == 2;
        darkLock = false;
        retroLock = false;
        retroPostProcessing.SetActive(PlayerPrefs.GetInt("Style") == 1);
        mainPostProcessing.SetActive(PlayerPrefs.GetInt("Style") == 0);
        RenderSettings.ambientIntensity = PlayerPrefs.GetInt("Style") == 0 ? 2.5f : 0f;
        retroLight.SetActive(PlayerPrefs.GetInt("Style") == 1);
        if (PlayerPrefs.GetInt("Style") == 2)
        {
            foreach (Renderer renderer in walls.GetComponentsInChildren<Renderer>())
            {
                renderer.material = darkWallMat;
            }
        }
        else
        {
            foreach (Renderer renderer in walls.GetComponentsInChildren<Renderer>())
            {
                renderer.material = normalWallMat;
            }
        }
    }
}
