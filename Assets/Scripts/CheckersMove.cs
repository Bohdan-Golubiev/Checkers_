using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class CheckersMove : MonoBehaviour
{
    private GameObject selectedPawn = null;
    private Vector3 originalPosition;
    private float liftHeight;
    private float cellSize;
    private float diagonalStep;

    private Vector3 whiteGraveyard;
    private Vector3 blackGraveyard;

    private bool isWhiteTurn = true;

    public GameObject desk;
    public GameObject cell;
    public GameObject crownWhite;
    public GameObject crownBlack;

    public AudioClip defoultMove;
    public AudioClip captureMove;
    private AudioSource audioSource;
    void Start()
    {
        cellSize = cell.transform.localScale.x;
        liftHeight = cellSize;
        diagonalStep = cellSize * Mathf.Sqrt(2);

        audioSource = gameObject.AddComponent<AudioSource>();

        Vector3 globalSize = desk.GetComponent<Renderer>().bounds.size;
        Vector3 deskPosition = desk.transform.position;
        whiteGraveyard = new Vector3(deskPosition.x - globalSize.x/1.5f, 0, deskPosition.z - globalSize.z/3f );
        blackGraveyard = new Vector3(deskPosition.x + globalSize.x/1.5f, 0, deskPosition.z + globalSize.z/3f);
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
                    MoveHighlighter.Instance.ClearHighlights();
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

    void ShowHighlightsForSelectedPiece()
    {
        MoveHighlighter.Instance.ClearHighlights();
        MoveHighlighter.Instance.ShowHighlights(selectedPawn, cellSize);
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
                    MoveHighlighter.Instance.ClearHighlights();
                }
            }
            else if (Mathf.Abs(moveDirection.magnitude - (2 * diagonalStep)) < 0.1f)
            {
                if (CanCaptureEnemy(moveDirection, out GameObject enemyPawn))
                {
                    CaptureEnemy(enemyPawn, target.transform.position);
                    EndTurn();
                    MoveHighlighter.Instance.ClearHighlights();
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
        MoveHighlighter.Instance.ClearHighlights();
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

        Vector3 middlePosition = selectedPawn.transform.position + moveDirection / 2;

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
            whiteGraveyard.z += cellSize / 2f;
        }
        else if (enemyPawn.CompareTag("BlackPawn")|| enemyPawn.CompareTag("BlackCrown"))
        {
            enemyPawn.transform.position = blackGraveyard;
            enemyPawn.transform.rotation = Quaternion.Euler(0, 90, -90);
            blackGraveyard.z -= cellSize / 2f;
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

        if (!isWhite && !isBlack) return;

        Vector3 pawnPos = selectedPawn.transform.position;
        pawnPos.y = 0;
        Vector3[] diagonalDirections =
        {
        new Vector3(-cellSize, 0, -cellSize),
        new Vector3(cellSize, 0, -cellSize),
        };

        if (isBlack)
        {
            diagonalDirections[0] *= -1;
            diagonalDirections[1] *= -1;
        }

        bool hasMoveSpots = false;

        foreach (Vector3 direction in diagonalDirections)
        {
            Vector3 checkPos = pawnPos + direction;
            if (MoveHighlighter.Instance.IsExistMoveSpot(checkPos))
            {
                hasMoveSpots = true;
                break;
            }
        }

        if (!hasMoveSpots)
        {
            Destroy(selectedPawn);
            GameObject newKing = Instantiate(isWhite ? crownWhite : crownBlack, pawnPos, Quaternion.identity);
            newKing.tag = isWhite ? "WhiteCrown" : "BlackCrown";
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
