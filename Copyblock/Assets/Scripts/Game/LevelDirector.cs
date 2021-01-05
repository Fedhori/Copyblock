using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// generate level and copies
public class LevelDirector : MonoBehaviour {

    public static LevelDirector LD;

    int[] dx = { 1, 0, -1, 0 };
    int[] dy = { 0, -1, 0, 1 };
    const int maxLength = 32;
    const int LimitCopy = 32;

    public int totalLevelNum;

    public LevelInfo customLevel;

    public LevelInfo[] EasyEasy;
    public LevelInfo[] EasyNormal;
    public LevelInfo[] EasyHard;
    public LevelInfo[] NormalEasy;
    public LevelInfo[] NormalNormal;
    public LevelInfo[] NormalHard;
    public LevelInfo[] HardEasy;
    public LevelInfo[] HardNormal;
    public LevelInfo[] HardHard;

    // store which color is linked to corresponding prefab
    public ColorToPrefab[] colorMappings;
    // first element of hintMappings color doesn't matter - just exist to fill first element.
    public ColorToPrefab[] hintMappings;

    public GameObject spacePrefab;
    public GameObject copyPrefab;
    public GameObject copiesParentPrefab;
    public GameObject hintsParentPrefab;

    // store space's tile information
    public GameObject[,] tileObj = new GameObject[maxLength, maxLength];
    public GameObject[,] hintObj = new GameObject[maxLength, maxLength];

    public Sprite[] TileSprites;

    public Color copyPrefabColor;
    public Color spacePrefabColor;
    public Color selectedPrefabColor;
    public Color hintPrefabColor;

    // -1 means unavailable, 0 means empty space, 1~n means, it has been occupied by some copy
    public int[,] tileType = new int[maxLength, maxLength];

    public int[,] hintType = new int[maxLength, maxLength];

    // while selecting, this array will store the each copies located in space has been selected or not 
    public int[] selectedCopyChk = new int[LimitCopy];

    public float tileSize;

    public Vector3 boardOffset;
    public float offsetX;
    public float offsetY;
    // initial position of space
    public float initialY;

    public int levelWidth;
    public int levelHeight;

    public bool isSelecting;
    public bool isCopyExist;

    int tileNum = 0;
    int[] tilesX = new int[1024];
    int[] tilesY = new int[1024];

    /*
    // current numer of copies
    public int curCopy = 0;
    // max number o copies
    public int maxCopy;
    */

    // current number of filled copies in space
    public int curFill = 0;
    // maximum available number of copies in space
    public int maxFill;

    public int actualFill;

    int lastX;
    int lastY;

    // Rollback section
    // store previous used copies
    public GameObject[] prevCopies = new GameObject[LimitCopy];
    public int[] prevCopiesPosX = new int[LimitCopy];
    public int[] prevCopiesPosY = new int[LimitCopy];
    // store whether current copy value is increased this turn or not
    public bool[] isCurCopyInc = new bool[LimitCopy];
    // check whether current status is select status, or filling status (selecting status means, you will going to select blocks)
    public bool isSelectStatus = true;
    // current turn
    public int curTurn = 0;
    public GameObject curCopyObj;

    public bool isLevelClear = false;

    public GameObject AllClearImage;

    public Camera cam;

    // Use this for initialization
    void Start() {

        // use this two lines to flush all playthrough
        //PlayerPrefs.SetInt("curLevel", 0);
        //PlayerPrefs.SetInt("clearingLevel", 0);

        LD = this;
        GenerateLevel(PlayerPrefs.GetInt("curLevel"));
        this.transform.Translate(0f, initialY, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // if level cleared, and you press any screen, move to main menu, or next level
            if (isLevelClear)
            {
                // if you just cleared highest level
                if (PlayerPrefs.GetInt("curLevel", 0) == PlayerPrefs.GetInt("highestAccessableLevel", 0))
                {
                    // if cleared level is not the last level
                    if (PlayerPrefs.GetInt("highestAccessableLevel", 0) < totalLevelNum - 1)
                    {
                        // change the value of highest cleared level
                        PlayerPrefs.SetInt("highestAccessableLevel", PlayerPrefs.GetInt("highestAccessableLevel", 0) + 1);

                        // change current level to next level, and load game scene
                        PlayerPrefs.SetInt("curLevel", PlayerPrefs.GetInt("curLevel", 0) + 1);
                        GameManager.GM.LoadGameScene();
                    }
                    // last level clear? congratulation!
                    else
                    {
                        GameManager.GM.LoadMainMenuScene();
                    }
                }
                else
                {
                    // if cleared level is not the last level
                    if (PlayerPrefs.GetInt("curLevel", 0) < totalLevelNum - 1)
                    {
                        // change current level to next level, and load game scene
                        PlayerPrefs.SetInt("curLevel", PlayerPrefs.GetInt("curLevel", 0) + 1);
                        GameManager.GM.LoadGameScene();
                    }
                    // last level clear? congratulation!
                    else
                    {
                        GameManager.GM.LoadMainMenuScene();
                    }
                }
            }
            // if there is no copy, we can assume that current status is selecting status
            else if (isCopyExist == false)
            {
                isSelecting = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isSelecting)
            {
                if (tileNum != 0)
                {
                    int lowestX = 1024;
                    int lowestY = 1024;

                    // change selected tile's sprite to filled tile's sprite, and find the lowest X and lowest Y value
                    for (int i = 0; i < tileNum; i++)
                    {
                        // paint every gray tiles to space tiles
                        paintSpaceSprite(tilesX[i], tilesY[i], false);
                        if (tilesX[i] < lowestX)
                        {
                            lowestX = tilesX[i];
                        }
                        if (tilesY[i] < lowestY)
                        {
                            lowestY = tilesY[i];

                        }
                    }

                    // move every tilesX and tilesY value according to lowest X and lowest Y
                    for (int i = 0; i < tileNum; i++)
                    {
                        tilesX[i] -= lowestX;
                        tilesY[i] -= lowestY;
                    }
                    // to prevent diagonal direction connection check whether new copy blocks are all connected in 4 direction
                    if (isAllConnected(tilesX, tilesY, tileNum))
                    {
                        if (curFill < maxFill)
                        {
                            // generate new copy
                            curCopyObj = GenerateCopies(lowestX, lowestY, tileNum, tilesX, tilesY);
                            isCopyExist = true;

                            // now it's fill status, not select status
                            isSelectStatus = false;
                            // store copy's start position
                            prevCopiesPosX[curTurn] = lowestX;
                            prevCopiesPosY[curTurn] = lowestY;
                        }
                    }

                    // initialize check array
                    for (int i = 0; i < LimitCopy; i++)
                    {
                        selectedCopyChk[i] = 0;
                    }
                    tileNum = 0;
                }

                isSelecting = false;
            }
        }

        // while selecting
        if (isSelecting)
        {
            int startX;
            int startY;
            Vector3 logicalPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) + boardOffset;
            // get the logical coordinate the reason why I seperated it with ifelse statement is, (int) (-1.0f ~ 0f] will be casted to 0
            if (logicalPosition.x + tileSize / 2 >= 0f)
            {
                startX = (int)((logicalPosition.x + tileSize / 2) / tileSize);
            }
            else
            {
                startX = -1;
            }

            if (logicalPosition.y + tileSize / 2 >= 0f)
            {
                startY = (int)((logicalPosition.y + tileSize / 2) / tileSize);
            }
            else
            {
                startY = -1;
            }

            if (startX >= 0 && startY >= 0)
            {
                // user just dragged the occupying copy
                if (tileType[startX, startY] > 0)
                {
                    if (selectedCopyChk[tileType[startX, startY]] == 0)
                    {
                        // ok! you are selected, now you need to paint it!
                        selectedCopyChk[tileType[startX, startY]] = 1;
                        selectBFS(startX, startY, tileType[startX, startY]);
                    }
                }
            }

            lastX = startX;
            lastY = startY;
        }
    }

    void GenerateLevel(int levelNumber)
    {
        setStartValues(levelNumber);
        GenerateMap(levelNumber);
    }

    void setStartValues(int levelNumber)
    {
        totalLevelNum = EasyEasy.Length + EasyNormal.Length + EasyHard.Length + NormalEasy.Length
            + NormalNormal.Length + NormalHard.Length + HardEasy.Length + HardNormal.Length + HardHard.Length;

        LevelInfo levelInfo = GetLevelInfo(levelNumber);

        maxFill = levelInfo.maxFill;

        for (int i = 0; i < maxLength; i++)
        {
            for (int j = 0; j < maxLength; j++)
            {
                tileType[i, j] = -1;
            }
        }
    }

    // generate stage, parameter is level number
    void GenerateMap(int levelNumber)
    {
        LevelInfo levelInfo = GetLevelInfo(levelNumber);
        int width = levelInfo.map.width;
        int height = levelInfo.map.height;

        levelWidth = width;
        levelHeight = height;

        offsetX = tileSize * (width - 1) / 2f;
        offsetY = tileSize * (height - 1) / 2f;

        boardOffset = new Vector3(offsetX, offsetY - initialY, 0f);

        int length = colorMappings.Length;
        int hintLength = hintMappings.Length;
        int numOfCopies = 0;

        GameObject hintsParent = Instantiate(copiesParentPrefab);
        hintsParent.GetComponent<SortingGroup>().sortingOrder = 1;

        for (int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                Color pixelColor = levelInfo.map.GetPixel(i, j);
                for(int k = 0; k < length ; k++)
                {
                    if (colorMappings[k].color.Equals(pixelColor))
                    {
                        Vector2 position = new Vector2(tileSize * i - offsetX, tileSize * j - offsetY);

                        GameObject tileObject = Instantiate(spacePrefab, transform);
                        tileObject.transform.localPosition = position;
                        tileObj[i, j] = tileObject;
                        tileType[i, j] = k;

                        if (k == 0)
                        {
                            tileObject.GetComponent<SpriteRenderer>().color = spacePrefabColor;
                        }
                        else
                        {
                            tileObject.GetComponent<SpriteRenderer>().color = copyPrefabColor;
                        }

                        if(numOfCopies < k)
                        {
                            numOfCopies = k;
                        }
                    }
                }

                // handle hint color's as space color (black)
                for (int k = 1; k < hintLength; k++)
                {
                    if (hintMappings[k].color.Equals(pixelColor))
                    {
                        Vector2 position = new Vector2(tileSize * i - offsetX, tileSize * j - offsetY);

                        GameObject tileObject = Instantiate(spacePrefab, transform);
                        tileObject.transform.localPosition = position;
                        tileObj[i, j] = tileObject;
                        tileType[i, j] = 0;

                        tileObject.GetComponent<SpriteRenderer>().color = spacePrefabColor;

                        // generate hint objects
                        GameObject hintObject = Instantiate(spacePrefab, hintsParent.transform);
                        hintObject.transform.localPosition = position;
                        hintObj[i, j] = hintObject;
                        hintType[i, j] = k;

                        hintObject.GetComponent<SpriteRenderer>().color = hintPrefabColor;
                    }
                }
            }
        }

        curFill = numOfCopies;

        // get some actual fill (number of available fill when players can saw)
        actualFill = maxFill - curFill;
        for (int i = 0; i < actualFill; i++)
        {
            UIDirector.UD.createFillDot(actualFill);
        }

        // change it sprite shape by its surrounding
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                paintSpaceSprite(i, j, false);
                paintHintSprite(i, j);
            }
        }

        hideHint();
    }

    public GameObject GenerateCopies(int x, int y, int tileNum, int[] tilesX, int[] tilesY)
    {
        SoundManager.SM.playSelectSound();

        int highestX = 0;
        int highestY = 0;

        // generate copy object (parent object)
        GameObject parent = Instantiate(copiesParentPrefab);
        CopyInfo copyInfo = parent.GetComponent<CopyInfo>();
        // move parent's position
        parent.transform.position = new Vector3(x * tileSize, y * tileSize) - boardOffset;

        for (int i = 0; i < tileNum; i++)
        {
            // set world position of tile prefab
            Vector2 position = new Vector2(tileSize * tilesX[i], tileSize * tilesY[i]);
            // instantiate tile prefab to this copy
            GameObject childTile = Instantiate(copyPrefab, parent.transform);
            childTile.transform.localPosition = position;
            childTile.GetComponent<SpriteRenderer>().color = selectedPrefabColor;
            // save array position of tile prefab
            copyInfo.tilesX[i] = tilesX[i];
            copyInfo.tilesY[i] = tilesY[i];
            copyInfo.tileType[tilesX[i], tilesY[i]] = 1;

            if (tilesX[i] > highestX)
            {
                highestX = tilesX[i];
            }
            if (tilesY[i] > highestY)
            {
                highestY = tilesY[i];
            }
        }

        // set copy's width and height
        copyInfo.width = highestX + 1;
        copyInfo.height = highestY + 1;
        // store how many tile does this copy has
        copyInfo.tileNum = tileNum;

        paintCopySprite(parent);

        return parent;
    }

    public void paintTiles(int startX, int startY, int[] tilesX, int[] tilesY, int arrLength)
    {
        SoundManager.SM.playFillSound();

        // increase current fill number, and update it
        curFill++;
        UIDirector.UD.destroyFillDot();

        // fill the tileType with it's current curFill number
        for (int i = 0; i < arrLength; i++)
        {
            int x = startX + tilesX[i];
            int y = startY + tilesY[i];

            tileType[x, y] = curFill;
        }

        // paint tiles according to its surroundings
        for (int i = 0; i < arrLength; i++)
        {
            int x = startX + tilesX[i];
            int y = startY + tilesY[i];

            paintSpaceSprite(x, y, false);
        }

        bool isFail = false;
        for (int i = 0; i < levelWidth; i++)
        {
            for (int j = 0; j < levelHeight; j++)
            {
                if (tileType[i, j] == 0)
                {
                    isFail = true;
                }
            }
        }

        if (!isFail)
        {
            LevelClear();
        }

    }

    public void rotateCopy(GameObject copy)
    {

        CopyInfo copyInfo = copy.GetComponent<CopyInfo>();
        // store how many times does it rotate
        copyInfo.rotation = (copyInfo.rotation + 1) % 4;
        int width = copyInfo.width;
        int height = copyInfo.height;

        // clear dirty values
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                copyInfo.tileType[i, j] = 0;
            }
        }

        int highest = 0;
        int temp;

        // get the lowest value of x after rotate it
        for (int i = 0; i < copyInfo.tileNum; i++)
        {
            if (copyInfo.tilesY[i] > highest)
            {
                highest = copyInfo.tilesY[i];
            }
        }

        // change copy's tiles location value
        for (int i = 0; i < copyInfo.tileNum; i++)
        {
            temp = copyInfo.tilesX[i];
            copyInfo.tilesX[i] = -copyInfo.tilesY[i] + highest;
            copyInfo.tilesY[i] = temp;
            copyInfo.tileType[copyInfo.tilesX[i], copyInfo.tilesY[i]] = 1;
            copy.transform.GetChild(i).transform.localPosition = new Vector2(tileSize * copyInfo.tilesX[i], tileSize * copyInfo.tilesY[i]);
        }

        temp = copyInfo.width;
        copyInfo.width = copyInfo.height;
        copyInfo.height = temp;

        paintCopySprite(copy);
    }

    public void selectBFS(int startX, int startY, int targetNum)
    {
        int[,] chk = new int[maxLength, maxLength];
        Queue<int> q = new Queue<int>();

        q.Enqueue(startX);
        q.Enqueue(startY);
        chk[startX, startY] = 1;

        while (q.Count != 0)
        {
            int x = q.Dequeue();
            int y = q.Dequeue();

            tilesX[tileNum] = x;
            tilesY[tileNum] = y;
            tileNum++;
            paintSpaceSprite(x, y, true);

            for (int i = 0; i < 4; i++)
            {
                if (x + dx[i] >= 0 && y + dy[i] >= 0)
                {
                    if (tileType[x + dx[i], y + dy[i]] == targetNum && chk[x + dx[i], y + dy[i]] == 0)
                    {
                        q.Enqueue(x + dx[i]);
                        q.Enqueue(y + dy[i]);
                        chk[x + dx[i], y + dy[i]] = 1;
                    }
                }
            }
        }
    }

    // if isSelected is true, paint it with selected prefab color, if not paint it with copy prefab color
    public void paintSpaceSprite(int x, int y, bool isSelected)
    {
        if (tileObj[x, y] != null && tileType[x, y] > 0)
        {
            int value = 0;
            int targetNum = tileType[x, y];
            for (int i = 0; i < 4; i++)
            {
                if (x + dx[i] >= 0 && y + dy[i] >= 0)
                {
                    if (tileType[x + dx[i], y + dy[i]] == targetNum)
                    {
                        switch (i)
                        {
                            case 0:
                                value += 1;
                                break;
                            case 1:
                                value += 2;
                                break;
                            case 2:
                                value += 4;
                                break;
                            case 3:
                                value += 8;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            tileObj[x, y].GetComponent<SpriteRenderer>().sprite = TileSprites[value];
            if (isSelected)
            {
                tileObj[x, y].GetComponent<SpriteRenderer>().color = selectedPrefabColor;
            }
            else
            {
                tileObj[x, y].GetComponent<SpriteRenderer>().color = copyPrefabColor;

            }
        }
    }

    // if isSelected is true, paint it with selected prefab color, if not paint it with copy prefab color
    public void paintHintSprite(int x, int y)
    {
        if (hintObj[x, y] != null && hintType[x, y] > 0)
        {
            int value = 0;
            int targetNum = hintType[x, y];
            for (int i = 0; i < 4; i++)
            {
                if (x + dx[i] >= 0 && y + dy[i] >= 0)
                {
                    if (hintType[x + dx[i], y + dy[i]] == targetNum)
                    {
                        switch (i)
                        {
                            case 0:
                                value += 1;
                                break;
                            case 1:
                                value += 2;
                                break;
                            case 2:
                                value += 4;
                                break;
                            case 3:
                                value += 8;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            hintObj[x, y].GetComponent<SpriteRenderer>().sprite = TileSprites[value];
        }
    }

    public void hideHint()
    {
        for (int i = 0; i < levelWidth; i++)
        {
            for (int j = 0; j < levelHeight; j++)
            {
                if(hintObj[i, j] != null)
                {
                    hintObj[i, j].SetActive(false);
                }
            }
        }
    }

    public void showHint()
    {
        for (int i = 0; i < levelWidth; i++)
        {
            for (int j = 0; j < levelHeight; j++)
            {
                if (hintObj[i, j] != null)
                {
                    hintObj[i, j].SetActive(true);
                }
            }
        }
    }

    public void paintCopySprite(GameObject parent)
    {
        CopyInfo copyInfo = parent.GetComponent<CopyInfo>();
        int tileNum = copyInfo.tileNum;

        for (int childNum = 0; childNum < tileNum; childNum++)
        {
            int x = copyInfo.tilesX[childNum];
            int y = copyInfo.tilesY[childNum];
            int value = 0;
            int targetNum = copyInfo.tileType[x, y];
            for (int i = 0; i < 4; i++)
            {
                if (x + dx[i] >= 0 && y + dy[i] >= 0)
                {
                    if (copyInfo.tileType[x + dx[i], y + dy[i]] == targetNum)
                    {
                        switch (i)
                        {
                            case 0:
                                value += 1;
                                break;
                            case 1:
                                value += 2;
                                break;
                            case 2:
                                value += 4;
                                break;
                            case 3:
                                value += 8;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            parent.transform.GetChild(childNum).GetComponent<SpriteRenderer>().sprite = TileSprites[value];
        }
    }

    public bool isAllConnected(int[] tilesX, int[] tilesY, int tileNum)
    {
        bool[,] chk = new bool[maxLength, maxLength];
        bool[,] isExist = new bool[maxLength, maxLength];
        int cnt = 0;

        for (int i = 0; i < tileNum; i++)
        {
            isExist[tilesX[i], tilesY[i]] = true;
        }

        Queue<int> q = new Queue<int>();

        q.Enqueue(tilesX[0]);
        q.Enqueue(tilesY[0]);
        chk[tilesX[0], tilesY[0]] = true;

        while (q.Count != 0)
        {
            int x = q.Dequeue();
            int y = q.Dequeue();
            cnt++;

            for (int i = 0; i < 4; i++)
            {
                if (x + dx[i] >= 0 && y + dy[i] >= 0)
                {
                    if (isExist[x + dx[i], y + dy[i]] == true && chk[x + dx[i], y + dy[i]] == false)
                    {
                        q.Enqueue(x + dx[i]);
                        q.Enqueue(y + dy[i]);
                        chk[x + dx[i], y + dy[i]] = true;
                    }
                }
            }
        }

        if(cnt == tileNum)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Rollback()
    {
        SoundManager.SM.playRollbackSound();

        /* if you are in select status, 
         * and try to rollback, you need to change it to fill status,
         * which means, you need to make previous copy to it's previous position 
         */
        if (isSelectStatus)
        {
            // NO RETREAT!!!!!
            if(curTurn != 0)
            {
                // now, it's fill status
                isSelectStatus = false;
                // decrease current turn
                curTurn--;
                // now, the copy is exist
                isCopyExist = true;
                // set current copy object
                curCopyObj = prevCopies[curTurn];
                // move copy to it's start position
                prevCopies[curTurn].transform.position = new Vector3(prevCopiesPosX[curTurn] * tileSize, prevCopiesPosY[curTurn] * tileSize) - boardOffset;
                // rollback space painting
                RollbackPainting(curFill);
                // current fill should be decreased
                curFill--;
                // create fill dot
                UIDirector.UD.createFillDot(actualFill);
            }
        }
        /* if you are in fill status,
         * and try to rollaback, you need to change it to select status,
         * which means, you need to destroy(in real) current copy
         */
        else
        {
            Destroy(curCopyObj);
            isCopyExist = false;
            isSelectStatus = true;
        }
    }

    public void RollbackPainting(int curFill)
    {
        for(int i = 0; i < maxLength; i++)
        {
            for(int j = 0; j < maxLength; j++)
            {
                if(tileType[i, j] == curFill)
                {
                    tileType[i, j] = 0;
                    tileObj[i, j].GetComponent<SpriteRenderer>().sprite = TileSprites[0];
                    tileObj[i, j].GetComponent<SpriteRenderer>().color = spacePrefabColor;
                }
            }
        }
    }

    public void LevelClear()
    {
        isLevelClear = true;
        SoundManager.SM.playStageClearSound();

        if (PlayerPrefs.GetInt("curLevel", 0) == totalLevelNum - 1)
        {
            AllClearImage.SetActive(true);
        }
    }

    public LevelInfo GetLevelInfo(int levelNumber)
    {
        if(levelNumber == -1)
        {
            cam.orthographicSize = 960;
            return customLevel;
        }

        if(levelNumber < EasyEasy.Length)
        {
            return EasyEasy[levelNumber];
        }
        else
        {
            levelNumber -= EasyEasy.Length;
        }

        if (levelNumber < EasyNormal.Length)
        {
            return EasyNormal[levelNumber];
        }
        else
        {
            levelNumber -= EasyNormal.Length;
        }

        if (levelNumber < EasyHard.Length)
        {
            return EasyHard[levelNumber];
        }
        else
        {
            levelNumber -= EasyHard.Length;
        }

        if (levelNumber < NormalEasy.Length)
        {
            return NormalEasy[levelNumber];
        }
        else
        {
            levelNumber -= NormalEasy.Length;
        }

        if (levelNumber < NormalNormal.Length)
        {
            return NormalNormal[levelNumber];
        }
        else
        {
            levelNumber -= NormalNormal.Length;
        }

        if (levelNumber < NormalHard.Length)
        {
            return NormalHard[levelNumber];
        }
        else
        {
            levelNumber -= NormalHard.Length;
        }

        if (levelNumber < HardEasy.Length)
        {
            return HardEasy[levelNumber];
        }
        else
        {
            levelNumber -= HardEasy.Length;
        }

        if (levelNumber < HardNormal.Length)
        {
            return HardNormal[levelNumber];
        }
        else
        {
            levelNumber -= HardNormal.Length;
        }

        if (levelNumber < HardHard.Length)
        {
            return HardHard[levelNumber];
        }
        else
        {
            // actually, if it reach here, something is wrong.. 
            return null;
        }
    }
}
