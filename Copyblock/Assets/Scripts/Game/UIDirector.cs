using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDirector : MonoBehaviour {

    public static UIDirector UD;
    public Canvas canvas;

    //public GameObject[] copyDot = new GameObject[64];
    public GameObject[] fillDot = new GameObject[64];

    public GameObject fillDots;

    //public GameObject copyDotPrefab;
    public GameObject fillDotPrefab;

    public float dotSize;

    int fillDotTop = 0;

    // Use this for initialization
    void Start () {
        UD = this;
    }
	
	// Update is called once per frame
	void Update () {
		
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
