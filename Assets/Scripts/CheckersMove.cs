using System.Collections.Generic;
using UnityEngine;

public class CheckersMove : MonoBehaviour
{
    private GameObject selectedPawn = null;
    private Vector3 originalPosition;
    private float liftHeight = 0.2f;
    private float cellSize = 0.21f;
    private float diagonalStep;

    private Vector3 whiteGraveyard = new Vector3(-0.5f, 0, -1f);
    private Vector3 blackGraveyard = new Vector3(2f, 0, 0.5f);

    private bool isWhiteTurn = true;

    public GameObject crownWhite;
    public GameObject crownBlack;

    public GameObject highlightBluePrefab;
    public GameObject highlightRedPrefab;
    private List<GameObject> activeHighlights = new List<GameObject>();

    public AudioClip defoultMove;
    public AudioClip captureMove;
    private AudioSource audioSource;
    void Start()
    {
        diagonalStep = cellSize * Mathf.Sqrt(2);
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (isWhiteTurn && hit.collider.CompareTag("WhitePawn") || 
                    !isWhiteTurn && hit.collider.CompareTag("BlackPawn")||
                    isWhiteTurn && hit.collider.CompareTag("WhiteCrown")||
                    !isWhiteTurn && hit.collider.CompareTag("BlackCrown"))
                {
                    ClearHighlights();
                    SelectPawn(hit.collider.gameObject);
                    ShowHighlightsForSelectedPiece();
                }
                else if (hit.collider.CompareTag("MoveSpot") && selectedPawn != null)
                {
                    TryMoveOrCapture(hit.collider.gameObject);
                }
            }
        }
    }
    void ClearHighlights()
    {
        foreach (GameObject go in activeHighlights)
        {
            Destroy(go);
        }
        activeHighlights.Clear();
    }

    void SelectPawn(GameObject pawn)
    {
        if (selectedPawn != null)
        {
            selectedPawn.transform.position = originalPosition;
        }
        selectedPawn = pawn;
        originalPosition = selectedPawn.transform.position;
        selectedPawn.transform.position += Vector3.up * liftHeight;
    }

    void ShowHighlightsForSelectedPiece() // подсвет
    {
        if (selectedPawn == null) return;

        if (selectedPawn.CompareTag("WhitePawn") || selectedPawn.CompareTag("BlackPawn"))
        {
            Vector3 currentPos = selectedPawn.transform.position;
            Vector3 moveDir1, moveDir2;
            if (selectedPawn.CompareTag("WhitePawn"))
            {
                moveDir1 = new Vector3(-cellSize, 0, -cellSize);
                moveDir2 = new Vector3(cellSize, 0, -cellSize);
            }
            else
            {
                moveDir1 = new Vector3(-cellSize, 0, cellSize);
                moveDir2 = new Vector3(cellSize, 0, cellSize);
            }
            CheckAndHighlightNormalMove(currentPos, moveDir1);
            CheckAndHighlightNormalMove(currentPos, moveDir2);

            CheckAndHighlightCaptureMove(currentPos, moveDir1);
            CheckAndHighlightCaptureMove(currentPos, moveDir2);
        }
        else if (selectedPawn.CompareTag("WhiteCrown") || selectedPawn.CompareTag("BlackCrown"))
        {
            Vector3[] directions = new Vector3[4]
            {
                new Vector3(cellSize, 0, cellSize),
                new Vector3(-cellSize, 0, cellSize),
                new Vector3(cellSize, 0, -cellSize),
                new Vector3(-cellSize, 0, -cellSize)
            };
            int maxSteps = 8;
            foreach (Vector3 dir in directions)
            {
                CheckAndHighlightKingMoves(selectedPawn.transform.position, dir, maxSteps);
            }
        }
    }
    void CheckAndHighlightKingMoves(Vector3 startPos, Vector3 direction, int maxSteps)
    {
        Vector3 pos = startPos;
        pos.y = 0;
        bool enemyEncountered = false;

        for (int i = 1; i <= maxSteps; i++)
        {
            pos += direction;

            Collider[] boardColliders = Physics.OverlapSphere(pos, 0.1f);
            bool onBoard = false;
            foreach (Collider col in boardColliders)
            {
                if (col.CompareTag("MoveSpot"))
                {
                    onBoard = true;
                    break;
                }
            }
            if (!onBoard)
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
                Vector3 highlightPos = new Vector3(pos.x, 0f, pos.z);
                Quaternion highlightRot = Quaternion.Euler(90, 0, 0);
                GameObject highlight = Instantiate(highlightBluePrefab, highlightPos, highlightRot);
                activeHighlights.Add(highlight);
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
                        Collider[] nextBoard = Physics.OverlapSphere(nextPos, 0.1f);
                        bool nextOnBoard = false;
                        foreach (Collider col in nextBoard)
                        {
                            if (col.CompareTag("MoveSpot"))
                            {
                                nextOnBoard = true;
                                break;
                            }
                        }
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
                        Vector3 enemyHighlightPos = new Vector3(pos.x, 0f, pos.z);
                        Quaternion highlightRot = Quaternion.Euler(90, 0, 0);
                        GameObject enemyHighlight = Instantiate(highlightRedPrefab, enemyHighlightPos, highlightRot);
                        activeHighlights.Add(enemyHighlight);

                        Vector3 landingHighlightPos = new Vector3(nextPos.x, 0f, nextPos.z);
                        GameObject landingHighlight = Instantiate(highlightBluePrefab, landingHighlightPos, highlightRot);
                        activeHighlights.Add(landingHighlight);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
    void CheckAndHighlightNormalMove(Vector3 startPos, Vector3 offset)
    {
        Vector3 pos = startPos + offset;
        pos.y = 0;

        Collider[] moveSpotColliders = Physics.OverlapSphere(pos, 0.1f);
        bool hasMoveSpot = false;
        foreach (Collider col in moveSpotColliders)
        {
            if (col.CompareTag("MoveSpot"))
            {
                hasMoveSpot = true;
                break;
            }
        }
        if (!hasMoveSpot)
            return;

        Collider[] colliders = Physics.OverlapSphere(pos, 0.1f);
        bool cellEmpty = true;
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("WhitePawn") || col.CompareTag("BlackPawn") ||
                col.CompareTag("WhiteCrown") || col.CompareTag("BlackCrown"))
            {
                cellEmpty = false;
                break;
            }
        }
        if (cellEmpty)
        {
            Vector3 highlightPos = new Vector3(pos.x, 0, pos.z);
            Quaternion highlightRot = Quaternion.Euler(90, 0, 0);
            GameObject highlight = Instantiate(highlightBluePrefab, highlightPos, highlightRot);
            activeHighlights.Add(highlight);
        }
    }
    void CheckAndHighlightCaptureMove(Vector3 startPos, Vector3 offset)
    {
        Vector3 enemyPos = startPos + offset;
        Vector3 landingPos = startPos + 2 * offset;
        enemyPos.y = 0;
        landingPos.y = 0;

        Collider[] moveSpotEnemy = Physics.OverlapSphere(enemyPos, 0.1f);
        bool enemyPosHasMoveSpot = false;
        foreach (Collider col in moveSpotEnemy)
        {
            if (col.CompareTag("MoveSpot"))
            {
                enemyPosHasMoveSpot = true;
                break;
            }
        }
        if (!enemyPosHasMoveSpot)
            return;
        Collider[] collidersEnemy = Physics.OverlapSphere(enemyPos, 0.1f);
        GameObject enemyFound = null;
        foreach (Collider col in collidersEnemy)
        {
            if (selectedPawn.CompareTag("WhitePawn"))
            {
                if (col.CompareTag("BlackPawn") || col.CompareTag("BlackCrown"))
                {
                    enemyFound = col.gameObject;
                    break;
                }
            }
            else if (selectedPawn.CompareTag("BlackPawn"))
            {
                if (col.CompareTag("WhitePawn") || col.CompareTag("WhiteCrown"))
                {
                    enemyFound = col.gameObject;
                    break;
                }
            }
        }
        if (enemyFound == null)
            return;

        Collider[] moveSpotLanding = Physics.OverlapSphere(landingPos, 0.1f);
        bool landingHasMoveSpot = false;
        foreach (Collider col in moveSpotLanding)
        {
            if (col.CompareTag("MoveSpot"))
            {
                landingHasMoveSpot = true;
                break;
            }
        }
        if (!landingHasMoveSpot)
            return;

        Collider[] collidersLanding = Physics.OverlapSphere(landingPos, 0.1f);
        bool landingEmpty = true;
        foreach (Collider col in collidersLanding)
        {
            if (col.CompareTag("WhitePawn") || col.CompareTag("BlackPawn") ||
                col.CompareTag("WhiteCrown") || col.CompareTag("BlackCrown"))
            {
                landingEmpty = false;
                break;
            }
        }
        if (landingEmpty)
        {
            Vector3 enemyHighlightPos = new Vector3(enemyPos.x, 0f, enemyPos.z);
            Quaternion highlightRot = Quaternion.Euler(90, 0, 0);
            GameObject enemyHighlight = Instantiate(highlightRedPrefab, enemyHighlightPos, highlightRot);
            activeHighlights.Add(enemyHighlight);

            Vector3 landingHighlightPos = new Vector3(landingPos.x, 0f, landingPos.z);
            GameObject landingHighlight = Instantiate(highlightBluePrefab, landingHighlightPos, highlightRot);
            activeHighlights.Add(landingHighlight);
        }
    }
    void TryMoveOrCapture(GameObject target)
    {
        if (selectedPawn.CompareTag("WhitePawn") || selectedPawn.CompareTag("BlackPawn"))
        {
            Vector3 moveDirection = target.transform.position - selectedPawn.transform.position;
            moveDirection.y = 0;

            if (Mathf.Abs(moveDirection.magnitude - diagonalStep) < 0.1f)
            {
                if ((selectedPawn.CompareTag("WhitePawn") && moveDirection.z < 0) ||
                    (selectedPawn.CompareTag("BlackPawn") && moveDirection.z > 0))
                {
                    MovePawn(target.transform.position);
                    EndTurn();
                    PlaySound(defoultMove, 0.5f);
                    ClearHighlights();
                }
            }
            else if (Mathf.Abs(moveDirection.magnitude - (2 * diagonalStep)) < 0.1f)
            {
                if (CanCaptureEnemy(moveDirection, out GameObject enemyPawn))
                {
                    CaptureEnemy(enemyPawn, target.transform.position);
                    EndTurn();
                    ClearHighlights();
                }
            }
        }
        else if (selectedPawn.CompareTag("WhiteCrown") || selectedPawn.CompareTag("BlackCrown"))
        {
            TryMoveOrCaptureKing(target);
        }
    }

    void TryMoveOrCaptureKing(GameObject target)
    {
        GameObject king = selectedPawn;
        Vector3 startPos = selectedPawn.transform.position;
        Vector3 targetPos = target.transform.position;
        Vector3 diff = targetPos - startPos;
        diff.y = 0;

        if (diff.magnitude < 0.1f)
            return;
        if (Mathf.Abs(Mathf.Abs(diff.x) - Mathf.Abs(diff.z)) > 0.1f)
            return;

        int steps = Mathf.RoundToInt(Mathf.Abs(diff.x) / cellSize);
        float stepX = Mathf.Sign(diff.x);
        float stepZ = Mathf.Sign(diff.z);
        Vector3 stepDir = new Vector3(stepX * cellSize, 0, stepZ * cellSize);

        int enemyCount = 0;
        GameObject enemyCandidate = null;
        Vector3 currentPos = startPos;
        for (int i = 1; i < steps; i++)
        {
            currentPos += stepDir;
            Collider[] colliders = Physics.OverlapSphere(currentPos, 0.15f);
            foreach (Collider col in colliders)
            {
                if (col.transform.root.gameObject == selectedPawn)
                {
                    continue;
                }
                bool isEnemy = false;
                if (selectedPawn.CompareTag("WhiteCrown"))
                {
                    if (col.CompareTag("BlackPawn") || col.CompareTag("BlackCrown"))
                        isEnemy = true;
                    else if (col.CompareTag("WhitePawn") || col.CompareTag("WhiteCrown"))
                        return;
                }
                else if (selectedPawn.CompareTag("BlackCrown"))
                {
                    if (col.CompareTag("WhitePawn") || col.CompareTag("WhiteCrown"))
                        isEnemy = true;
                    else if (col.CompareTag("BlackPawn") || col.CompareTag("BlackCrown"))
                        return;
                }
                if (isEnemy)
                {
                    enemyCount++;
                    enemyCandidate = col.gameObject;
                }
            }
        }
        if (enemyCount > 1)
            return;             
        if (enemyCount == 1)
        {
            CaptureEnemy(enemyCandidate, target.transform.position);
            TextSwap.Instance.EndTurn();
            selectedPawn = king;
        }
        else
        { 
            PlaySound(defoultMove, 0.5f);
        }

        MovePawn(targetPos);
        ClearHighlights();
        EndTurn();
    }
    void MovePawn(Vector3 targetPosition)
    {
        targetPosition.y = originalPosition.y;
        selectedPawn.transform.position = targetPosition;
        PromoteToCrown();
        selectedPawn = null;

        TextSwap.Instance.EndTurn();
    }
    bool CanCaptureEnemy(Vector3 moveDirection, out GameObject enemyPawn)
    {
        enemyPawn = null;

        // Находим среднюю точку между шашками
        Vector3 middlePosition = selectedPawn.transform.position + moveDirection / 2;

        // Ищем шашку в этой позиции
        Collider[] colliders = Physics.OverlapSphere(middlePosition, 0.15f);

        foreach (Collider col in colliders)
        {
            if ((selectedPawn.CompareTag("WhitePawn") &&
                 (col.CompareTag("BlackPawn") || col.CompareTag("BlackCrown"))) ||
                 (selectedPawn.CompareTag("BlackPawn") &&
                 (col.CompareTag("WhitePawn") || col.CompareTag("WhiteCrown"))))
            {
                enemyPawn = col.gameObject;

                middlePosition.y = enemyPawn.transform.position.y;

                // Проверяем, совпадают ли глобальные координаты
                if (Vector3.Distance(middlePosition, enemyPawn.transform.position) < 0.1f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void CaptureEnemy(GameObject enemyPawn, Vector3 newPosition)
    {

        if (enemyPawn.CompareTag("WhitePawn")|| enemyPawn.CompareTag("WhiteCrown"))
        {
            enemyPawn.transform.position = whiteGraveyard;
            enemyPawn.transform.rotation = Quaternion.Euler(0, 90, 90);
            whiteGraveyard.z += 0.1f;
        }
        else if (enemyPawn.CompareTag("BlackPawn")|| enemyPawn.CompareTag("BlackCrown"))
        {
            enemyPawn.transform.position = blackGraveyard;
            enemyPawn.transform.rotation = Quaternion.Euler(0, 90, -90);
            blackGraveyard.z -= 0.1f;
        }
        PlaySound(captureMove, 0.3f);
        MovePawn(newPosition);
        GameManager.Instance.RemoveChecker(enemyPawn.tag);
        enemyPawn.tag = "Untagged"; 
    }

    void EndTurn()
    {
        isWhiteTurn = !isWhiteTurn;
    }
    void PromoteToCrown()
    {
        bool isWhite = selectedPawn.CompareTag("WhitePawn");
        bool isBlack = selectedPawn.CompareTag("BlackPawn");
        Vector3 pawnPos = selectedPawn.transform.position;

        if (isWhite && pawnPos.z <= -1.05f)
        {
            Destroy(selectedPawn);
            GameObject newKing = Instantiate(crownWhite, pawnPos, Quaternion.Euler(0, 0, 0));
            newKing.tag = "WhiteCrown";
        }
        else if (isBlack && pawnPos.z >= 0.42f)
        {
            Destroy(selectedPawn);
            GameObject newKing = Instantiate(crownBlack, pawnPos, Quaternion.Euler(0, 0, 0));
            newKing.tag = "BlackCrown";
        }
    }
    void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
}
