using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {

    public static MainMenuManager MMM;

    public GameObject levelBtns;
    public GameObject levelBtnPrefab;

    public float tileSize;
    public int tileWidth;
    public int highestAccessableLevel;

    public int maxLevelNum;

    // Use this for initialization
    void Awake() {
        MMM = this;

        // get the level which player's highest cleared level\\
        highestAccessableLevel = PlayerPrefs.GetInt("highestAccessableLevel", 0);

        for (int i = 0; i < maxLevelNum; i++)
        {
            GameObject levelBtn = Instantiate(levelBtnPrefab, levelBtns.transform);
            levelBtn.GetComponent<MainMenuBtnDirector>().btnNum = i;
            levelBtn.GetComponentInChildren<Text>().text = (i+1).ToString();

            // cleared level button will painted to orange
            if (i > highestAccessableLevel)
            {
                levelBtn.GetComponent<Button>().interactable = false;
                levelBtn.GetComponentInChildren<Transform>().gameObject.SetActive(false);
            }
        }
    }

    public void CallLevel(int levelNum)
    {
        PlayerPrefs.SetInt("curLevel", levelNum);
        SceneManager.LoadScene("GameScene");
    }
}
