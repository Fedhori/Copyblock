 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragDrop : MonoBehaviour
{
    Vector3 parentOffset;
    Vector3 startPosition;

    CopyInfo copyInfo;

    float minMove = 10f;

    public bool isDragging = false;

    // Use this for initialization
    void Start () {
        copyInfo = GetComponentInParent<CopyInfo>();
    }

    public void OnMouseDown()
    {
        parentOffset = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.parent.position;
        startPosition = transform.parent.position;

        isDragging = true;
    }

    public void OnMouseUp()
    {
        // if it didn't dragged enough distance, rotate it
        if (Vector2.Distance(startPosition, transform.parent.position) < minMove)
        {
            SoundManager.SM.playRotateSound();
            LevelDirector.LD.rotateCopy(transform.parent.gameObject);
            isDragging = false;
        }
        // if it dragged enough distance, drag it
        else
        {
            Vector3 logicalPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) - parentOffset + LevelDirector.LD.boardOffset;

            int startX;
            int startY;

            // get the logical coordinate. The reason why I seperated it with if-else statement is, (int) (-1.0f ~ 0f] will be casted to 0
            if (logicalPosition.x + LevelDirector.LD.tileSize / 2 >= 0f)
            {
                startX = (int)((logicalPosition.x + LevelDirector.LD.tileSize / 2) / LevelDirector.LD.tileSize);
            }
            else
            {
                isDragging = false;
                return;
            }

            if (logicalPosition.y + LevelDirector.LD.tileSize / 2 >= 0f)
            {
                startY = (int)((logicalPosition.y + LevelDirector.LD.tileSize / 2) / LevelDirector.LD.tileSize);
            }
            else
            {
                isDragging = false;
                return;
            }

            // check whether the new copy is available in space
            bool isFail = false;
            for (int i = 0; i < copyInfo.tileNum; i++)
            {
                int x = startX + copyInfo.tilesX[i];
                int y = startY + copyInfo.tilesY[i];
                if (x >= 0 && y >= 0)
                {
                    if (LevelDirector.LD.tileType[x, y] != 0)
                    {
                        isFail = true;
                    }
                }
                else
                {
                    isFail = true;
                }
            }

            // if it succeed, make new copy
            if (isFail == false)
            {
                LevelDirector.LD.paintTiles(startX, startY, copyInfo.tilesX, copyInfo.tilesY, copyInfo.tileNum);
                // copy doesn't exist now. (Actually hided)
                LevelDirector.LD.isCopyExist = false;


                // rollback opearation
                // store copy information
                LevelDirector.LD.prevCopies[LevelDirector.LD.curTurn] = transform.parent.gameObject;
                // hide copy
                transform.parent.position = new Vector2(0, 5000);
                // rotate it to original state
                int rollbackRotation = (4 - transform.parent.GetComponent<CopyInfo>().rotation) % 4;
                for(int i = 0; i < rollbackRotation; i++)
                {
                    LevelDirector.LD.rotateCopy(transform.parent.gameObject);
                }
                // increase current turn
                LevelDirector.LD.curTurn++;
                // change current status to select
                LevelDirector.LD.isSelectStatus = true;
            }

            isDragging = false;
        }
    }

    void Update()
    {
        // if dragging the copy, drag it
        if (isDragging == true)
        {
            transform.parent.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) - parentOffset;
        }
    }
}
