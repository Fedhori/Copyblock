using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuBtnDirector : MonoBehaviour {

    public int btnNum;

	public void CallLevel()
    {
        float tileSize = MainMenuManager.MMM.tileSize;
        int tileWidth = MainMenuManager.MMM.tileWidth;
        int LevelNum = (int)(transform.GetComponent<RectTransform>().anchoredPosition.x / tileSize) +
            (int)(Mathf.Abs(transform.GetComponent<RectTransform>().anchoredPosition.y) / tileSize) * tileWidth;

        SoundManager.SM.playSelectSound();
        MainMenuManager.MMM.CallLevel(btnNum);
    }
}
