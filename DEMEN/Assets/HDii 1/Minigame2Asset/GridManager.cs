using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [HideInInspector]
    public bool canInput = true;
    [Header("Grid size")]
    public int rows = 8;
    public int cols = 8;
    public float gemSize = 1.1f; // world unit per tile

    [Header("Prefabs & Sprites")]
    public GameObject gemPrefab;
    public Sprite[] gemSprites; // order matches GemType enum

    [Header("Animation")]
    public float swapDuration = 0.15f;
    public float fallDurationPerCell = 0.05f;

    private Gem[,] grid;
    private bool isShifting = false;
    [Header("Win / Lose Settings")]
    public int targetScore = 3000;            // mốc để win
    public GameObject winPanel;
    public GameObject losePanel;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioSource audioSource;
    public static GridManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        InitGrid();
    }

    public void InitGrid()
    {
        grid = new Gem[rows, cols];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                SpawnGemAt(r, c);
            }
        }
        // ensure no initial matches
        StartCoroutine(ClearInitialMatchesRoutine());
    }

    Vector3 WorldPos(int r, int c)
    {
        // origin at GridManager position, top-left style
        Vector3 origin = transform.position;
        // we'll place row 0 at top (y = 0), row increases downward
        float x = origin.x + c * gemSize;
        float y = origin.y - r * gemSize;
        return new Vector3(x, y, 0f);
    }

    void SpawnGemAt(int r, int c)
    {
        GameObject go = Instantiate(gemPrefab, WorldPos(r, c), Quaternion.identity, transform);
        Gem gem = go.GetComponent<Gem>();
        gem.SetPosition(r, c);
        // choose random type and sprite
        int typeIndex = Random.Range(0, System.Enum.GetValues(typeof(GemType)).Length);
        gem.type = (GemType)typeIndex;
        gem.SetSprite(gemSprites[typeIndex]);
        grid[r, c] = gem;
    }

    IEnumerator ClearInitialMatchesRoutine()
    {
        // If any matches exist immediately after spawning, re-roll those gems until none
        bool any = true;
        while (any)
        {
            var matches = FindAllMatches();
            if (matches.Count == 0) { any = false; break; }
            foreach (var g in matches)
            {
                // pick a different type than neighbors (simple reassign)
                int tries = 0;
                int maxTries = gemSprites.Length * 2;
                do
                {
                    int newType = Random.Range(0, gemSprites.Length);
                    g.type = (GemType)newType;
                    g.SetSprite(gemSprites[newType]);
                    tries++;
                } while (tries < maxTries && CheckMatchAt(g.row, g.col));
            }
            yield return null;
        }
    }

    bool CheckMatchAt(int r, int c)
    {
        // return true if gem at r,c is in a match of >=3 (horizontal or vertical)
        Gem g = grid[r, c];
        if (g == null) return false;
        GemType t = g.type;
        int count = 1;
        // left
        for (int cc = c - 1; cc >= 0; cc--)
        {
            if (grid[r, cc].type == t) count++; else break;
        }
        // right
        for (int cc = c + 1; cc < cols; cc++)
        {
            if (grid[r, cc].type == t) count++; else break;
        }
        if (count >= 3) return true;

        count = 1;
        // up
        for (int rr = r - 1; rr >= 0; rr--)
        {
            if (grid[rr, c].type == t) count++; else break;
        }
        // down
        for (int rr = r + 1; rr < rows; rr++)
        {
            if (grid[rr, c].type == t) count++; else break;
        }
        return (count >= 3);
    }

    public IEnumerator TrySwap(Gem a, Gem b, System.Action<bool> callback)
    {
        if (!canInput) { callback?.Invoke(false); yield break; }
        if (isShifting) { callback?.Invoke(false); yield break; }
        if (a == null || b == null) { callback?.Invoke(false); yield break; }
        // ensure adjacency
        if (Mathf.Abs(a.row - b.row) + Mathf.Abs(a.col - b.col) != 1) { callback?.Invoke(false); yield break; }

        // swap in grid
        SwapInGrid(a, b);
        // animate swap
        yield return StartCoroutine(AnimateSwap(a, b));
        // check matches
        var matches = FindAllMatches();
        if (matches.Count == 0)
        {
            // undo
            SwapInGrid(a, b);
            yield return StartCoroutine(AnimateSwap(a, b));
            callback?.Invoke(false);
        }
        else
        {
            // clear matches and cascade
            yield return StartCoroutine(ClearAndCollapseRoutine());
            MoveManager.Instance.UseMove();
            CheckEndGame();
            callback?.Invoke(true);

        }
    }

    void SwapInGrid(Gem a, Gem b)
    {
        // swap references and update row/col
        int ar = a.row, ac = a.col;
        int br = b.row, bc = b.col;
        grid[ar, ac] = b;
        grid[br, bc] = a;
        a.SetPosition(br, bc);
        b.SetPosition(ar, ac);
    }

    IEnumerator AnimateSwap(Gem a, Gem b)
    {
        Vector3 posA = WorldPos(a.row, a.col);
        Vector3 posB = WorldPos(b.row, b.col);
        float t = 0f;
        Vector3 startA = a.transform.position;
        Vector3 startB = b.transform.position;
        while (t < swapDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Min(1f, t / swapDuration);
            a.transform.position = Vector3.Lerp(startA, posA, p);
            b.transform.position = Vector3.Lerp(startB, posB, p);
            yield return null;
        }
        a.transform.position = posA;
        b.transform.position = posB;
    }

    List<Gem> FindAllMatches()
    {
        List<Gem> matched = new List<Gem>();
        // rows
        for (int r = 0; r < rows; r++)
        {
            int matchLen = 1;
            for (int c = 1; c < cols; c++)
            {
                if (grid[r, c].type == grid[r, c - 1].type)
                {
                    matchLen++;
                }
                else
                {
                    if (matchLen >= 3)
                    {
                        for (int k = 0; k < matchLen; k++) matched.Add(grid[r, c - 1 - k]);
                    }
                    matchLen = 1;
                }
            }
            if (matchLen >= 3)
            {
                for (int k = 0; k < matchLen; k++) matched.Add(grid[r, cols - 1 - k]);
            }
        }
        // cols
        for (int c = 0; c < cols; c++)
        {
            int matchLen = 1;
            for (int r = 1; r < rows; r++)
            {
                if (grid[r, c].type == grid[r - 1, c].type)
                {
                    matchLen++;
                }
                else
                {
                    if (matchLen >= 3)
                    {
                        for (int k = 0; k < matchLen; k++) matched.Add(grid[r - 1 - k, c]);
                    }
                    matchLen = 1;
                }
            }
            if (matchLen >= 3)
            {
                for (int k = 0; k < matchLen; k++) matched.Add(grid[rows - 1 - k, c]);
            }
        }
        // remove duplicates
        List<Gem> unique = new List<Gem>();
        foreach (var g in matched) if (!unique.Contains(g)) unique.Add(g);
        return unique;
    }

    IEnumerator ClearAndCollapseRoutine()
    {
        isShifting = true;
        while (true)
        {
            var matches = FindAllMatches();
            if (matches.Count == 0) break;

            // ---- TÍNH ĐIỂM CHO TỪNG NHÓM MATCH ----
            Dictionary<GemType, int> groupCount = new Dictionary<GemType, int>();

            foreach (var g in matches)
            {
                if (!groupCount.ContainsKey(g.type))
                    groupCount[g.type] = 0;

                groupCount[g.type]++;
            }

            // Gửi điểm theo từng nhóm
            foreach (var kv in groupCount)
            {
                ScoreManager.Instance.AddScore(kv.Value);
            }

            // ---- XOÁ GEM ----
            foreach (var g in matches)
            {
                StartCoroutine(ScaleDownAndDestroy(g.gameObject, 0.12f));
                grid[g.row, g.col] = null;
            }
            // wait a bit
            yield return new WaitForSeconds(0.12f + 0.02f);
            // collapse columns
            yield return StartCoroutine(CollapseColumns());
            // spawn new gems to fill
            yield return StartCoroutine(FillEmptySpaces());
            // loop to check new matches (cascade)
        }
        isShifting = false;
    }

    IEnumerator ScaleDownAndDestroy(GameObject go, float dur)
    {
        float t = 0f;
        Vector3 start = go.transform.localScale;
        while (t < dur)
        {
            t += Time.deltaTime;
            float p = Mathf.Min(1f, t / dur);
            go.transform.localScale = Vector3.Lerp(start, Vector3.zero, p);
            yield return null;
        }
        Destroy(go);
    }

    IEnumerator CollapseColumns()
    {
        // for each column, move gems down to fill nulls
        for (int c = 0; c < cols; c++)
        {
            int writeRow = rows - 1; // start from bottom
            for (int r = rows - 1; r >= 0; r--)
            {
                if (grid[r, c] != null)
                {
                    if (writeRow != r)
                    {
                        // move grid[r,c] to writeRow,c
                        Gem g = grid[r, c];
                        grid[writeRow, c] = g;
                        g.SetPosition(writeRow, c);
                        grid[r, c] = null;
                    }
                    writeRow--;
                }
            }
        }
        // animate positions
        List<Coroutine> running = new List<Coroutine>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (grid[r, c] != null)
                {
                    Gem g = grid[r, c];
                    Vector3 target = WorldPos(r, c);
                    float dist = Mathf.Abs(g.transform.position.y - target.y) / gemSize;
                    float dur = Mathf.Max(0.05f, dist * fallDurationPerCell);
                    StartCoroutine(MoveToPosition(g.transform, target, dur));
                }
            }
        }
        // wait a bit for animations
        yield return new WaitForSeconds(0.05f + rows * fallDurationPerCell);
    }

    IEnumerator MoveToPosition(Transform t, Vector3 target, float dur)
    {
        Vector3 start = t.position;
        float time = 0f;
        while (time < dur)
        {
            time += Time.deltaTime;
            float p = Mathf.Min(1f, time / dur);
            t.position = Vector3.Lerp(start, target, p);
            yield return null;
        }
        t.position = target;
    }

    IEnumerator FillEmptySpaces()
    {
        for (int c = 0; c < cols; c++)
        {
            int emptyCount = 0;
            for (int r = rows - 1; r >= 0; r--)
            {
                if (grid[r, c] == null) emptyCount++;
            }
            for (int i = 0; i < emptyCount; i++)
            {
                int spawnRow = -1 - i; // start above the grid
                GameObject go = Instantiate(gemPrefab, WorldPos(spawnRow, c), Quaternion.identity, transform);
                Gem gem = go.GetComponent<Gem>();
                int typeIndex = Random.Range(0, gemSprites.Length);
                gem.type = (GemType)typeIndex;
                gem.SetSprite(gemSprites[typeIndex]);
                gem.SetPosition(spawnRow, c);
                // animate falling into correct spot
            }
        }
        // now assign them into grid and animate falling into place
        for (int c = 0; c < cols; c++)
        {
            int writeRow = rows - 1;
            for (int r = rows - 1; r >= 0; r--)
            {
                if (grid[r, c] == null)
                {
                    // find highest gem with row < 0 in column c
                    Gem topGem = null;
                    int topRow = int.MinValue;
                    foreach (Transform child in transform)
                    {
                        Gem g = child.GetComponent<Gem>();
                        if (g != null && g.col == c && g.row < 0)
                        {
                            if (g.row > topRow) { topRow = g.row; topGem = g; }
                        }
                    }
                    if (topGem != null)
                    {
                        topGem.SetPosition(r, c);
                        grid[r, c] = topGem;
                    }
                }
                else
                {
                    // already filled
                }
            }
        }
        // animate all gems to their new positions
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Gem g = grid[r, c];
                if (g != null)
                {
                    Vector3 target = WorldPos(r, c);
                    float dist = Mathf.Abs(g.transform.position.y - target.y) / gemSize;
                    float dur = Mathf.Max(0.05f, dist * fallDurationPerCell);
                    StartCoroutine(MoveToPosition(g.transform, target, dur));
                }
            }
        }
        // wait for fall animations
        yield return new WaitForSeconds(0.05f + rows * fallDurationPerCell);
    }
    public void CheckEndGame()
    {
        int currentScore = ScoreManager.Instance.GetScore();
        int movesLeft = MoveManager.Instance.GetMoves();

        // WIN
        if (currentScore >= targetScore)
        {
            if (audioSource && winSound)
                audioSource.PlayOneShot(winSound);

            if (winPanel)
                winPanel.SetActive(true);

            canInput = false;   // KHÓA SWAP
            Debug.Log("YOU WIN!");
            return;
        }

        // LOSE (hết lượt nhưng điểm chưa đủ)
        if (movesLeft <= 0)
        {
            if (movesLeft <= 0)
            {
                if (currentScore < targetScore)
                {
                    if (audioSource && loseSound)
                        audioSource.PlayOneShot(loseSound);

                    if (losePanel)
                        losePanel.SetActive(true);

                    canInput = false;   // KHÓA SWAP
                    Debug.Log("YOU LOSE!");
                }
            }
        }
    }
    public void ExitGame()
    {
        SceneManagerHelper.Instance.ReturnToPreviousScene();
    }

}
