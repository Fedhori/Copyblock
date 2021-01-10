using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIDirector : MonoBehaviour {

    public static UIDirector UD;
    public Canvas canvas;

    //public GameObject[] copyDot = new GameObject[64];
    public GameObject[] fillDot = new GameObject[64];

    public GameObject fillDots;

    //public GameObject copyDotPrefab;
    public GameObject fillDotPrefab;

    public GameObject gameMenu;
    Vector2 gameMenuDismissPosition = new Vector2(0, -128.0f);
    Vector2 gameMenuPopupPosition = new Vector2(0, 128.0f);
    float gameMenuAnimationCycle = 0.2f;
    float gameMenuAnimationSpan = 0.0f;
    bool isPopup = false;
    bool isGameMenuMoving = false;

    public float dotSize;

    int fillDotTop = 0;

    // Use this for initialization
    void Start () {
        UD = this;
    }
	
	// Update is called once per frame
	void Update () {
        if (IsClickEmptySpace())
        {
            isGameMenuMoving = true;
            isPopup = !isPopup;
        }

        if (isGameMenuMoving)
        {
            if (isPopup)
            {
                PopupGameMenu();
            }
            else
            {
                DismissGameMenu();
            }

            gameMenuAnimationSpan += Time.deltaTime;
            if (gameMenuAnimationSpan > gameMenuAnimationCycle)
            {
                gameMenuAnimationSpan = 0.0f;
                isGameMenuMoving = false;
            }
        }
    }

    public void PopupGameMenu()
    {
        float t = gameMenuAnimationSpan / gameMenuAnimationCycle;
        if (t > 1.0f)
        {
            t = 1.0f;
        }

        gameMenu.GetComponent<RectTransform>().anchoredPosition
            = Vector2.Lerp(gameMenuDismissPosition, gameMenuPopupPosition, t);
    }

    public void DismissGameMenu()
    {
        float t = gameMenuAnimationSpan / gameMenuAnimationCycle;
        if (t > 1.0f)
        {
            t = 1.0f;
        }

        gameMenu.GetComponent<RectTransform>().anchoredPosition
            = Vector2.Lerp(gameMenuPopupPosition, gameMenuDismissPosition, t);
    }

    public bool IsClickEmptySpace()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

                if (hit.collider == null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void createFillDot(int actualFill)
    {
        GameObject dot = Instantiate(fillDotPrefab, fillDots.transform);
        dot.GetComponent<RectTransform>().anchoredPosition = new Vector2((3f / 2f * fillDotTop - (float)(actualFill - 1) * 3f / 4f) * dotSize, -3f / 2f * dotSize);
        dot.GetComponent<RectTransform>().sizeDelta = new Vector2(dotSize, dotSize);
        fillDot[fillDotTop] = dot;
        fillDotTop++;
    }

    public void destroyFillDot()
    {
        Destroy(fillDot[--fillDotTop]);
    }
}
