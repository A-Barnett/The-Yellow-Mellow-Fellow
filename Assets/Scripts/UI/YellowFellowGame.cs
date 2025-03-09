using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class YellowFellowGame : MonoBehaviour
{
    [SerializeField] GameObject highScoreUI;
    [SerializeField] GameObject mainMenuUI;
    [SerializeField] GameObject gameUI;
    [SerializeField] GameObject escUI;
    [SerializeField] GameObject deathInputUI;
    [SerializeField] GameObject player2;
    [SerializeField] private PlayerController playerObject;
    [SerializeField] private PlayerController player2object;
    [SerializeField] private TextMeshProUGUI deathScoreText;
    private GameObject[] pellets;
    private bool escLock;
    private bool gameRunning;
    

    // Start is called before the first frame update
    void Start()
    {
        pellets = GameObject.FindGameObjectsWithTag("Pellet");
        StartMainMenu();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("escape") && gameRunning)
        {
            if (!escLock)
            {
                 escUI.SetActive(true);
                 Time.timeScale = 0;
                 escLock = true;
            }
            else
            {
                escUI.SetActive(false);
                Time.timeScale = 1;
                escLock = false;
            }
        }
        int pelletsEaten = playerObject.GetPelletsEaten() + player2object.GetPelletsEaten();
        if (pelletsEaten == pellets.Length)
        {
            PlayerPrefs.SetInt("Died",2);
            PlayerPrefs.SetInt("Level", PlayerPrefs.GetInt("Level")+1);
            PlayerPrefs.SetInt("Score",PlayerPrefs.GetInt("Score")+playerObject.GetScore());
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    public void StartMainMenu()
    {
        mainMenuUI.gameObject.SetActive(true);
        highScoreUI.gameObject.SetActive(false);
        gameUI.gameObject.SetActive(false);
        Time.timeScale = 0;
    }
    public void StartHighScores()
    {
        mainMenuUI.gameObject.SetActive(false);
        highScoreUI.gameObject.SetActive(true);
        gameUI.gameObject.SetActive(false);
    }
    public void StartGame()
    {
        mainMenuUI.gameObject.SetActive(false);
        highScoreUI.gameObject.SetActive(false);
        gameUI.gameObject.SetActive(true);
        Time.timeScale = 1;
        gameRunning = true;
    }
    public void StartDeathInput()
    {
        Time.timeScale = 0;
        gameUI.SetActive(false);
        deathInputUI.SetActive(true);
        PlayerPrefs.SetInt("Died",1);
        deathScoreText.text = PlayerPrefs.GetInt("Score").ToString();
    }
    public void ToggleTwoPlayer(bool enable)
    {
        player2.SetActive(enable);
    }
}
