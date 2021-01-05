using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this class will store the information about each copies
public class CopyInfo : MonoBehaviour {

    // width of this copy
    public int width;
    // height of this copy
    public int height;

    // store copy's tile information -> not yet located in space -> set as 1, located in space -> set as curFill
    public int[,] tileType = new int[32, 32];

    // number of tile's this copy has
    public int tileNum;
    // store copy's tile location
    public int[] tilesX = new int[1024];
    public int[] tilesY = new int[1024];

    public int rotation = 0;
}
