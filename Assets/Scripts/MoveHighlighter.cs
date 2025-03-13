using System.Collections.Generic;
using UnityEngine;

public class MoveHighlighter : MonoBehaviour
{
    public GameObject highlightBluePrefab;
    public GameObject highlightRedPrefab;

    private List<GameObject> activeHighlights = new List<GameObject>();

    public static MoveHighlighter Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void ShowHighlights(GameObject selectedPawn, float cellSize)
    {
        if (selectedPawn == null) return;

        Vector3 currentPos = selectedPawn.transform.position;
        currentPos.y = 0;

        Vector3[] directions = new Vector3[4]
        {
             new Vector3(cellSize, 0, cellSize),
             new Vector3(-cellSize, 0, cellSize),
             new Vector3(cellSize, 0, -cellSize),
             new Vector3(-cellSize, 0, -cellSize)
        };

        if (selectedPawn.CompareTag("WhitePawn") || selectedPawn.CompareTag("BlackPawn"))
        {
            Vector3 moveDir1, moveDir2;
            if (selectedPawn.CompareTag("WhitePawn"))
            {
                moveDir1 = directions[3];
                moveDir2 = directions[2];
            }
            else
            {
                moveDir1 = directions[1];
                moveDir2 = directions[0];
            }
            CheckAndHighlightNormalMove(currentPos, moveDir1);
            CheckAndHighlightNormalMove(currentPos, moveDir2);

            CheckAndHighlightCaptureMove(currentPos, moveDir1, selectedPawn);
            CheckAndHighlightCaptureMove(currentPos, moveDir2, selectedPawn);
        }
        else if (selectedPawn.CompareTag("WhiteCrown") || selectedPawn.CompareTag("BlackCrown"))
        {
            int maxSteps = 8;
            foreach (Vector3 dir in directions)
            {
                CheckAndHighlightKingMoves(selectedPawn.transform.position, dir, maxSteps, selectedPawn);
            }
        }
    }
    private void CheckAndHighlightNormalMove(Vector3 startPos, Vector3 offset)
    {
        Vector3 pos = startPos + offset;
        if (!IsExistMoveSpot(pos)) return;

        Collider[] colliders = Physics.OverlapSphere(pos, 0.1f);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("WhitePawn") || col.CompareTag("BlackPawn") ||
                col.CompareTag("WhiteCrown") || col.CompareTag("BlackCrown"))
            {
                return;
            }
        }

        AddHighlightCell(pos, highlightBluePrefab);
    }
    private void CheckAndHighlightCaptureMove(Vector3 startPos, Vector3 offset, GameObject selectedPawn)
    {
        Vector3 enemyPos = startPos + offset;
        Vector3 landingPos = startPos + 2 * offset;

        if (!IsExistMoveSpot(enemyPos) || !IsExistMoveSpot(landingPos)) return;

        Collider[] collidersEnemy = Physics.OverlapSphere(enemyPos, 0.1f);
        GameObject enemyFound = null;
        foreach (Collider col in collidersEnemy)
        {
            if (selectedPawn.CompareTag("WhitePawn") && (col.CompareTag("BlackPawn") || col.CompareTag("BlackCrown")))
            {
                enemyFound = col.gameObject;
                break;
            }
            if (selectedPawn.CompareTag("BlackPawn") && (col.CompareTag("WhitePawn") || col.CompareTag("WhiteCrown")))
            {
                enemyFound = col.gameObject;
                break;
            }
        }
        if (enemyFound == null) return;

        Collider[] collidersLanding = Physics.OverlapSphere(landingPos, 0.1f);
        foreach (Collider col in collidersLanding)
        {
            if (!col.CompareTag("MoveSpot")) return;
        }

        AddHighlightCell(enemyPos, highlightRedPrefab);
        AddHighlightCell(landingPos, highlightBluePrefab);
    }
    void CheckAndHighlightKingMoves(Vector3 startPos, Vector3 direction, int maxSteps, GameObject selectedPawn)
    {
        Vector3 pos = startPos;
        pos.y = 0;
        bool enemyEncountered = false;

        for (int i = 1; i <= maxSteps; i++)
        {
            pos += direction;

            if(!IsExistMoveSpot(pos))
            {
                break;
            }


            Collider[] colliders = Physics.OverlapSphere(pos, 0.1f);
            List<Collider> pieces = new List<Collider>();
            foreach (Collider col in colliders)
            {
                if (!col.CompareTag("MoveSpot"))
                    pieces.Add(col);
            }

            if (pieces.Count == 0)
            {
                AddHighlightCell(pos, highlightBluePrefab);
            }
            else
            {
                bool foundEnemy = false;
                bool foundAlly = false;
                foreach (Collider col in pieces)
                {
                    if (selectedPawn.CompareTag("WhiteCrown"))
                    {
                        if (col.CompareTag("BlackPawn") || col.CompareTag("BlackCrown"))
                            foundEnemy = true;
                        else if (col.CompareTag("WhitePawn") || col.CompareTag("WhiteCrown"))
                            foundAlly = true;
                    }
                    else if (selectedPawn.CompareTag("BlackCrown"))
                    {
                        if (col.CompareTag("WhitePawn") || col.CompareTag("WhiteCrown"))
                            foundEnemy = true;
                        else if (col.CompareTag("BlackPawn") || col.CompareTag("BlackCrown"))
                            foundAlly = true;
                    }
                }

                if (foundAlly)
                {
                    break;
                }
                if (foundEnemy)
                {
                    if (!enemyEncountered)
                    {
                        Vector3 nextPos = pos + direction;
                        bool nextOnBoard = IsExistMoveSpot(nextPos);
                        if (!nextOnBoard)
                        {
                            break;
                        }

                        Collider[] nextColliders = Physics.OverlapSphere(nextPos, 0.1f);
                        bool nextFree = true;
                        foreach (Collider col in nextColliders)
                        {
                            if (!col.CompareTag("MoveSpot"))
                            {
                                nextFree = false;
                                break;
                            }
                        }
                        if (!nextFree)
                        {
                            break;
                        }

                        enemyEncountered = true;
                        AddHighlightCell(pos, highlightRedPrefab);
                        AddHighlightCell(nextPos, highlightBluePrefab);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
    public bool IsExistMoveSpot(Vector3 pos)
    {
        Collider[] colliders = Physics.OverlapSphere(pos, 0.1f);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("MoveSpot")) 
                return true;
        }
        return false;
    }
    private void AddHighlightCell(Vector3 pos, GameObject prefab)
    {
        GameObject highlight = Instantiate(prefab, new Vector3(pos.x, 0, pos.z), Quaternion.Euler(90, 0, 0));
        activeHighlights.Add(highlight);
    }
    public void ClearHighlights()
    {
        foreach (var highlight in activeHighlights)
        {
            Destroy(highlight);
        }
        activeHighlights.Clear();
    }
}
