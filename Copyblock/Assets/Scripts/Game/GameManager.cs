using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// manage game and tiles
public class GameManager : MonoBehaviour {

    public static GameManager GM;

    // Use this for initialization
    void Awake () {
        GM = this;
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if((LevelDirector.LD.curTurn == 0 && LevelDirector.LD.isSelectStatus) || LevelDirector.LD.isLevelClear)
            {
                SceneManager.LoadScene("MainMenuScene");
            }
            else
            {
                LevelDirector.LD.Rollback();
            }
        }
    }

    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }
}
