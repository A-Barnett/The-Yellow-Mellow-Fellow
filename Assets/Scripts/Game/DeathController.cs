using TMPro;
using UnityEngine;

public class DeathController : MonoBehaviour
{
   [SerializeField] private GameObject firstGameUI;
   [SerializeField] private GameObject winGameUI;
   [SerializeField] private TextMeshProUGUI winScoreText;
   private void Awake()
   {
      PlayerPrefs.SetInt("lives", 6);
     
      if (PlayerPrefs.GetInt("Died")==2)
      {
         firstGameUI.SetActive(false);
         winGameUI.SetActive(true);
         winScoreText.text = PlayerPrefs.GetInt("Score").ToString();
      }
      else
      {
         firstGameUI.SetActive(true);
         winGameUI.SetActive(false);
      }
   }

   //Reset PlayerPrefs
   private void OnApplicationQuit()
   {
      PlayerPrefs.SetInt("Died",0);
      PlayerPrefs.SetInt("Score",0);
      PlayerPrefs.SetInt("Style",0);
      PlayerPrefs.SetInt("Level",1);
      PlayerPrefs.SetInt("lives", 6);
      PlayerPrefs.SetInt("TwoPlayer", 0);
   }
}
