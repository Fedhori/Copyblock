using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour {

	public void ShowHint()
    {
        LevelDirector LD = LevelDirector.LD;

        LD.showHint();
    }
}
