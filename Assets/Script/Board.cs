using JetBrains.Annotations;
using Match3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    [SerializeField]
    private LevelData levelData;

    private List<Goal> runtimeGoals;

    private int width;

    private int height;

    private int movesLeft;

    [SerializeField]
    private GameObject[] gemPrefabs;

    [SerializeField]
    private GameObject[] gemBonusPrefabs;

    [SerializeField]
    private GameObject[] blockPrefabs;

    [SerializeField]
    private GameObject tilePrefabs;

    [SerializeField]
    private float tileSpacing = 1f;

    [SerializeField]
    private Sprite[] borderTiles;

    private GemType gemBonus;

    public GemType GemBonus
    {
        get { return gemBonus; }
        set {  gemBonus = value; }
    }

    private int rocketCount;
    private int bombCount;

    private Gem[,] allGems;

    private List<Rope> allRopes = new List<Rope>();

    private List<WoodenCrate> allCrate = new List<WoodenCrate>();

    private Gem selectedGem;

    private bool firstSpawn = true;

    public List<Goal> RuntimeGoals => runtimeGoals;

    public GameObject[] GemPrefabs => gemPrefabs;
    
    public GameObject[] GemBonusPrefabs => gemBonusPrefabs;

    public Gem SelectedGem => selectedGem;

    private BoardState currentState = BoardState.Processing;

    private char[,] map;

    private bool test = false;

    private bool isEndingProcessing = false;

    private bool collapseMoved;
    private bool fillSpawned;
    public const string KEY_BOMB = "Bomb";
    public const string KEY_COLOR = "Color";
    public const string KEY_VERTICAL = "Vertical";
    public const string KEY_HORIZONTAL = "Horizontal";
    public const string MAIN_VOL = "MainVol";
    public const string MUSIC_VOL = "MusicVol";
    public const string SFX_VOL = "SfxVol";
    public const string COIN_QUANTITY = "CoinQuantity";


    public event System.Action<int> OnMovesChanged;

    public event System.Action<int> OnGemDestroyed;

    public event System.Action<bool> OnGameEnding;

    public event System.Action<GemType> UpdateBonusItem;

    public static Board Instance { get; private set; }

    public class MatchResult
    {
        public List<Gem> gems = new List<Gem>();
        public bool isHorizontal;
        public bool isVertical;
        public bool isTLshape;
    }

    public BoardState CurrentState
    {
        get => currentState;
        set
        {
            if(currentState == BoardState.Ending)
            {
                return;
            }
            else if (currentState == value) return;
            else
            {
                if (value == BoardState.Ending)
                {
                    if (isEndingProcessing) return;
                    PlayerPrefs.Save();
                    isEndingProcessing = true;
                    StartCoroutine(WaitForEnd());
                }
                else
                {
                    currentState = value;
                }
            }
        }
    }

    IEnumerator WaitForEnd()
    {
        yield return new WaitUntil(() => AllGemsStopped() && currentState == BoardState.Idle);
        currentState = BoardState.Ending;
        OnGameEnding?.Invoke(FinishGoal());
    }

    public int MovesLeft
    {
        get
        {
            return movesLeft;
        }
        set
        {
            movesLeft = value;
            OnMovesChanged?.Invoke(movesLeft);
            if(movesLeft <= 0)
            {
                CurrentState = BoardState.Ending;
            }
        }
    }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        width = levelData.Width;
        height = levelData.Height;
        allGems = new Gem[width, height];
        map = new char[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                map[x, y] = levelData.Map[y][x];
            }
        }
        SetupBoard();
        FitCamera();
        //Debug.Log(allGems.Length);
        CurrentState = BoardState.Idle;
        rocketCount = width * height / 6;
        bombCount = width * height / 10;
        runtimeGoals = new List<Goal>();
        foreach (var g in levelData.Goals)
        {
            if (g == null) continue;
            runtimeGoals.Add(new Goal(g.GoalType, g.GemType, g.Target));
        }
        movesLeft = levelData.Movesleft;
    }

    public void SpawnGemBonusItem(int x, int y)
    {
        allGems[x, y].Types = GemBonus;
        UpdateBonusItem?.Invoke(GemBonus);
        GemBonus = GemType.Blue;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioManager.Instance.PlayMusicBG();
    }

    // Update is called once per frame
    void Update()
    {
        //if (AllGemsStopped() && !test)
        //{
        //    test = true;
        //    allGems[3, 3].Types = GemType.SmallBomb;
        //    //allGems[4, 3].Types = GemType.SmallBomb;
        //}
    }

    bool FinishGoal()
    {
        foreach(var gem in RuntimeGoals)
        {
            if(gem.Target > gem.Current)
            {
                return false;
            }
        }
        return true;
    }

    public void RemoveRope(int x, int y)
    {
        Rope tempRope = findRope(x, y);
        if (tempRope != null)
        {
            allRopes.Remove(tempRope);
            Destroy(tempRope.gameObject);
            map[tempRope.X, tempRope.Y] = '1';
        }
    }

    public void RemoveCrate(int x, int y)
    {
        WoodenCrate tempCrate = findCrate(x, y);
        if (tempCrate != null)
        {
            allCrate.Remove(tempCrate);
            Destroy(tempCrate.gameObject);
            map[tempCrate.X, tempCrate.Y] = '1';
        }
    }

    void SetupBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == '1')
                {
                    SpawnGem(x, y);
                    SpawnTile(x, y);
                }
                else if(map[x, y] == '3')
                {
                    SpawnRope(x, y);
                    SpawnGem(x, y);
                    SpawnTile(x, y);
                }
                else if (map[x, y] == '2')
                {
                    SpawnCrate(x, y);
                    SpawnTile(x, y);
                }
            }
        }
        firstSpawn = false;
    }

    void SpawnRope(int x, int y)
    {
        Vector2 spawnPosition = GetWorldPosition(x, y);
        GameObject ropeObject = Instantiate(blockPrefabs[0], spawnPosition, Quaternion.identity);
        Rope rope = ropeObject.GetComponent<Rope>();
        rope.Initialize(x, y);
        allRopes.Add(rope);
        rope.transform.parent = this.transform;
    }

    void SpawnCrate(int x, int y)
    {
        Vector2 spawnPosition = GetWorldPosition(x, y);
        GameObject crateObject = Instantiate(blockPrefabs[1], spawnPosition, Quaternion.identity);
        WoodenCrate crate = crateObject.GetComponent<WoodenCrate>();
        crate.Initialize(x, y);
        allCrate.Add(crate);
        crate.transform.parent = this.transform;
    }

    void SpawnGem(int x, int y)
    {
        Vector2 spawnPosition = GetWorldPosition(x, height + UnityEngine.Random.Range(1,3));
        Vector2 targetPost = GetWorldPosition(x, y);
        int randomIndex = UnityEngine.Random.Range(0, gemPrefabs.Length);
        int finalIndex = randomIndex;
        while(CausesMatch(x, y, (GemType)finalIndex) && firstSpawn)
        {
            finalIndex++;
            if(finalIndex == gemPrefabs.Length)
            {
                finalIndex = 0;
            }
            if(finalIndex == randomIndex)
            {
                break;
            }
        }
        GameObject gemObj = Instantiate(gemPrefabs[finalIndex], spawnPosition, Quaternion.identity);
        Gem gem = gemObj.GetComponent<Gem>();
        gem.Initialize(x, y);
        allGems[x, y] = gem;
        gemObj.transform.parent = this.transform;
        StartCoroutine(gem.MoveTo(targetPost));
    }

    void SpawnTile(int x, int y)
    {
        Vector2 spawnPosition = GetWorldPosition(x, y);
        GameObject tileObject =  Instantiate(tilePrefabs, spawnPosition, Quaternion.identity);
        tileObject.transform.parent = this.transform;
        int mask = GetMask(x, y);
        Transform border = tileObject.transform.Find("Border");
        border.GetComponent<SpriteRenderer>().sprite = borderTiles[mask];

    }

    void FitCamera()
    {
        float boardWidth = width * tileSpacing;
        float boardHeight = height * tileSpacing;
        Camera cam = Camera.main;
        float screenRatio = (float)Screen.width / Screen.height;
        float targetRatio = boardWidth/boardHeight;
        if(screenRatio >= targetRatio)  
        {
            cam.orthographicSize = boardHeight / 2f + 2f;
        }
        else
        {
            float difference = targetRatio / screenRatio;
            cam.orthographicSize = boardHeight/2f * difference +2f;
        }
    }

    int GetMask(int x, int y)
    {
        int mask = 0;
        if(HasTile(x, y + 1))
        {
            mask += 1;
        }
        if(HasTile(x + 1, y))
        {
            mask += 2;
        }
        if(HasTile(x, y - 1))
        {
            mask += 4;
        }
        if(HasTile(x - 1, y))
        {
            mask += 8;
        }
        return mask;
    }

    bool HasTile(int x, int y)
    {
        if(x < 0 || x >= width || y < 0 || y >= height)
        {
            return false;
        }
        return map[x, y] != '0';
    }
    
    bool IsAdjacent(Gem a, Gem b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y) == 1;
    }

    public void SelectGem(Gem gem)
    {
        if(gem == null)
        {
            if(selectedGem != null)
            {
                selectedGem.DeSelect();
                selectedGem = null;
            }
        }
        else if(selectedGem == null)
        {
            selectedGem = gem;
            selectedGem.Select();
            AudioManager.Instance.OnClickGem();
        }
        else if(selectedGem == gem)
        {
            selectedGem.DeSelect();
            selectedGem = null;
        }
        else if (IsAdjacent(gem, selectedGem))
        {
            StartCoroutine(Swap(selectedGem, gem));
            selectedGem.DeSelect();
            selectedGem = null;
        }
        else
        {
            selectedGem.DeSelect();
            selectedGem = gem;
            selectedGem.Select();
            AudioManager.Instance.OnClickGem();
        }
    }

    IEnumerator Swap(Gem gem1, Gem gem2)
    {
        AudioManager.Instance.OnSwap();
        bool matchSth = false;
        CurrentState = BoardState.Swapping;
        int gem1X = gem1.X;
        int gem1Y = gem1.Y;
        int gem2X = gem2.X;
        int gem2Y = gem2.Y;
        allGems[gem1X, gem1Y] = gem2;
        allGems[gem2X, gem2Y] = gem1;
        gem1.SetCoordinates(gem2X, gem2Y);
        gem2.SetCoordinates(gem1X, gem1Y);
        Vector2 pos1 = gem1.transform.position;
        Vector2 pos2 = gem2.transform.position;
        float duration = 0.2f;
        float time = 0;
        while (time < duration)
        {
            float t = time / duration;
            t = t * t * (3f - 2f * t);//smothStep
            gem1.transform.position = Vector2.Lerp(pos1, pos2, t);
            gem2.transform.position = Vector2.Lerp(pos2, pos1, t);
            time += Time.deltaTime;
            yield return null;
        }
        gem1.transform.position = pos2;
        gem2.transform.position = pos1;
        if(gem1.IsGemBonus && gem2.IsGemBonus)
        {
            matchSth = true;
            CreatCombo(gem1, gem2);
        }
        else
        {
            var matches1 = CheckMatchAt(gem1);
            var matches2 = CheckMatchAt(gem2);
            if (gem1.Types == GemType.Color || gem2.Types == GemType.Color)
            {
                matchSth = true;
                if (gem1.Types == GemType.Color)
                {
                    ClearColor(gem1, gem2);
                }
                else
                {
                    ClearColor(gem2, gem1);
                }
            }

            else
            {
                if (gem1.Types == GemType.Vertical)
                {
                    matchSth = true;
                    ClearColumn(gem1.X, gem1.Y);
                }
                else if (gem1.Types == GemType.Horizontal)
                {
                    matchSth = true;
                    ClearRow(gem1.X, gem1.Y);
                }
                else if (gem1.Types == GemType.SmallBomb)
                {
                    matchSth = true;
                    ClearArea(gem1.X, gem1.Y, 1);
                }
                if (gem2.Types == GemType.Vertical)
                {
                    matchSth = true;
                    ClearColumn(gem2.X, gem2.Y);
                }
                else if (gem2.Types == GemType.Horizontal)
                {
                    matchSth = true;
                    ClearRow(gem2.X, gem2.Y);
                }
                else if (gem2.Types == GemType.SmallBomb)
                {
                    matchSth = true;
                    ClearArea(gem2.X, gem2.Y, 1);
                }
                if (matches1.Count >= 3 || matches2.Count >= 3)
                {
                    matchSth = true;
                    if (matches1.Count >= 3)
                    {
                        SpawnGemBonus(matches1, gem1);
                        DestroyMatches(new List<Gem>(matches1),true);
                    }
                    if (matches2.Count >= 3)
                    {
                        SpawnGemBonus(matches2, gem2);
                        DestroyMatches(new List<Gem>(matches2), true);
                    }
                }
            }
        }
        if (matchSth)
        {
            StartCoroutine(ProcessBoard());
            MovesLeft--;
        }
        else
        {
            allGems[gem1X, gem1Y] = gem1;
            allGems[gem2X, gem2Y] = gem2;
            gem1.SetCoordinates(gem1X, gem1Y);
            gem2.SetCoordinates(gem2X, gem2Y);
            time = 0;
            while (time < duration)
            {
                float t = time / duration;
                t = t * t * (3f - 2f * t);
                gem1.transform.position = Vector2.Lerp(pos2, pos1, t);
                gem2.transform.position = Vector2.Lerp(pos1, pos2, t);
                time += Time.deltaTime;
                yield return null;
            }
            gem1.transform.position = pos1;
            gem2.transform.position = pos2;
            CurrentState = BoardState.Idle;
        }
    }

    void CreatCombo(Gem gem1, Gem gem2)
    {
        if (gem1.Types == GemType.Vertical)
        {
            if (gem2.Types == GemType.Vertical)
            {
                if (gem1.X == gem2.X)
                {
                    ClearColumn(gem1.X, gem1.Y);
                    if (gem1.X + 1 < width)
                    {
                        ClearColumn(gem1.X + 1, gem1.Y);
                    }
                    if (gem1.X - 1 >= 0)
                    {
                        ClearColumn(gem1.X - 1, gem1.Y);
                    }
                }
                else
                {
                    ClearColumn(gem1.X, gem1.Y);
                    ClearColumn(gem2.X, gem2.Y);
                    if (gem1.X < gem2.X)
                    {
                        if (gem1.X - 1 >= 0)
                        {
                            ClearColumn(gem1.X - 1, gem1.Y);
                        }
                        else if (gem2.X + 1 < width)
                        {
                            ClearColumn(gem2.X + 1, gem2.Y);
                        }
                    }
                    else
                    {
                        if (gem2.X - 1 >= 0)
                        {
                            ClearColumn(gem2.X - 1, gem2.Y);
                        }
                        else if (gem1.X + 1 < width)
                        {
                            ClearColumn(gem1.X + 1, gem1.Y);
                        }
                    }
                }
            }
            else if (gem2.Types == GemType.Horizontal)
            {
                ClearColumn(gem1.X, gem1.Y);
                ClearRow(gem2.X, gem2.Y);
                
            }
            else if (gem2.Types == GemType.SmallBomb)
            {
                for (int i = -1; i < 2; i++)
                {
                    for (int z = -1; z < 2; z++)
                    {
                        int X = gem2.X + i;
                        int Y = gem2.Y + z;
                        if (X < 0 || X >= width || Y < 0 || Y >= height || allGems[X, Y] == null)
                        {
                            continue;
                        }
                        GemType type = UnityEngine.Random.value < 0.7f ? GemType.Vertical : GemType.Horizontal;
                        allGems[X, Y].Types = type;
                    }
                }
                for (int i = -1; i < 2; i++)
                {
                    for (int z = -1; z < 2; z++)
                    {
                        int X = gem2.X + i;
                        int Y = gem2.Y + z;
                        if (X < 0 || X >= width || Y < 0 || Y >= height || allGems[X, Y] == null)
                        {
                            continue;
                        }
                        else
                        {
                            if (allGems[X, Y].Types == GemType.Vertical)
                            {
                                ClearColumn(X, Y);
                            }
                            else
                            {
                                ClearRow(X, Y);
                            }
                        }
                    }
                }
            }
            else if (gem2.Types == GemType.Color)
            {
                List<Gem> selected = ColorComboSpawn(rocketCount, gem1);
                foreach (Gem gem in selected)
                {
                    if(gem.Types == GemType.Vertical)
                    {
                        ClearColumn(gem.X, gem.Y);
                    }
                    else
                    {
                        ClearRow(gem.X, gem.Y);
                    }
                }
                allGems[gem2.X, gem2.Y] = null;
                Destroy(gem2.gameObject);
            }
        }
        else if (gem1.Types == GemType.Horizontal)
        {
            if (gem2.Types == GemType.Vertical)
            {
                ClearRow(gem1.X, gem1.Y);
                ClearColumn(gem2.X, gem2.Y);
            }
            else if (gem2.Types == GemType.Horizontal)
            {
                if (gem1.Y == gem2.Y)
                {
                    ClearColumn(gem1.X, gem1.Y);
                    if (gem1.X + 1 < height)
                    {
                        ClearColumn(gem1.X, gem1.Y + 1);
                    }
                    if (gem1.X - 1 >= 0)
                    {
                        ClearColumn(gem1.X, gem1.Y - 1);
                    }
                }
                else
                {
                    ClearColumn(gem1.X, gem1.Y);
                    ClearColumn(gem2.X, gem2.Y);
                    if (gem1.X < gem2.X)
                    {
                        if (gem1.Y - 1 >= 0)
                        {
                            ClearColumn(gem1.X, gem1.Y - 1);
                        }
                        else if (gem2.Y + 1 < height)
                        {
                            ClearColumn(gem2.X, gem2.Y + 1);
                        }
                    }
                    else
                    {
                        if (gem2.Y - 1 >= 0)
                        {
                            ClearColumn(gem2.X, gem2.Y - 1);
                        }
                        else if (gem1.X + 1 < height)
                        {
                            ClearColumn(gem1.X, gem1.Y + 1);
                        }
                    }
                }
            }
            else if (gem2.Types == GemType.SmallBomb)
            {
                for (int i = -1; i < 2; i++)
                {
                    for (int z = -1; z < 2; z++)
                    {
                        int X = gem2.X + i;
                        int Y = gem2.Y + z;
                        if (X < 0 || X >= width || Y < 0 || Y >= height || allGems[X, Y] == null)
                        {
                            continue;
                        }
                        GemType type = UnityEngine.Random.value < 0.7f ? GemType.Horizontal : GemType.Vertical;
                        allGems[X, Y].Types = type;
                    }
                }
                for (int i = -1; i < 2; i++)
                {
                    for (int z = -1; z < 2; z++)
                    {
                        int X = gem2.X + i;
                        int Y = gem2.Y + z;
                        if (X < 0 || X >= width || Y < 0 || Y >= height || allGems[X, Y] == null)
                        {
                            continue;
                        }
                        else
                        {
                            if (allGems[X, Y].Types == GemType.Vertical)
                            {
                                ClearColumn(X, Y);
                            }
                            else
                            {
                                ClearRow(X, Y);
                            }
                        }
                    }
                }
            }
            else if (gem2.Types == GemType.Color)
            {
                List<Gem> selected = ColorComboSpawn(rocketCount, gem1);
                foreach (Gem gem in selected)
                {
                    if (gem.Types == GemType.Vertical)
                    {
                        ClearColumn(gem.X, gem.Y);
                    }
                    else
                    {
                        ClearRow(gem.X, gem.Y);
                    }
                }
                allGems[gem2.X, gem2.Y] = null;
                Destroy(gem2.gameObject);
            }
        }
        else if (gem1.Types == GemType.SmallBomb)
        {
            if (gem2.Types == GemType.Vertical)
            {
                for (int i = -1; i < 2; i++)
                {
                    for (int z = -1; z < 2; z++)
                    {
                        int X = gem1.X + i;
                        int Y = gem1.Y + z;
                        if (X < 0 || X >= width || Y < 0 || Y >= height || allGems[X, Y] == null)
                        {
                            continue;
                        }
                        GemType type = UnityEngine.Random.value < 0.7f ? GemType.Vertical : GemType.Horizontal;
                        allGems[X, Y].Types = type;
                    }
                }
                for (int i = -1; i < 2; i++)
                {
                    for (int z = -1; z < 2; z++)
                    {
                        int X = gem2.X + i;
                        int Y = gem2.Y + z;
                        if (X < 0 || X >= width || Y < 0 || Y >= height || allGems[X, Y] == null)
                        {
                            continue;
                        }
                        else
                        {
                            if (allGems[X, Y].Types == GemType.Vertical)
                            {
                                ClearColumn(X, Y);
                            }
                            else
                            {
                                ClearRow(X, Y);
                            }
                        }
                    }
                }
            }
            else if (gem2.Types == GemType.Horizontal)
            {
                for (int i = -1; i < 2; i++)
                {
                    for (int z = -1; z < 2; z++)
                    {
                        int X = gem1.X + i;
                        int Y = gem1.Y + z;
                        if (X < 0 || X >= width || Y < 0 || Y >= height || allGems[X, Y] == null)
                        {
                            continue;
                        }
                        GemType type = UnityEngine.Random.value < 0.7f ? GemType.Horizontal : GemType.Vertical;
                        allGems[X, Y].Types = type;
                    }
                }
                for (int i = -1; i < 2; i++)
                {
                    for (int z = -1; z < 2; z++)
                    {
                        int X = gem2.X + i;
                        int Y = gem2.Y + z;
                        if (X < 0 || X >= width || Y < 0 || Y >= height || allGems[X, Y] == null)
                        {
                            continue;
                        }
                        else
                        {
                            if (allGems[X, Y].Types == GemType.Vertical)
                            {
                                ClearColumn(X, Y);
                            }
                            else
                            {
                                ClearRow(X, Y);
                            }
                        }
                    }
                }
            }
            else if (gem2.Types == GemType.SmallBomb)
            {
                ClearArea(gem1.X, gem1.Y, 2);
            }
            else if (gem2.Types == GemType.Color)
            {
                List<Gem> selected = ColorComboSpawn(bombCount, gem1);
                foreach (Gem gem in selected)
                {
                    ClearArea(gem.X, gem.Y, 1);
                }
            }
        }
        else if (gem1.Types == GemType.Color)
        {
            if (gem2.Types == GemType.Vertical)
            {
                List<Gem> selected = ColorComboSpawn(rocketCount, gem2);
                foreach (Gem gem in selected)
                {
                    if (gem.Types == GemType.Vertical)
                    {
                        ClearColumn(gem.X, gem.Y);
                    }
                    else
                    {
                        ClearRow(gem.X, gem.Y);
                    }
                }
                allGems[gem1.X, gem1.Y] = null;
                Destroy(gem1.gameObject);
            }
            else if (gem2.Types == GemType.Horizontal)
            {
                List<Gem> selected = ColorComboSpawn(rocketCount, gem2);
                foreach (Gem gem in selected)
                {
                    if (gem.Types == GemType.Vertical)
                    {
                        ClearColumn(gem.X, gem.Y);
                    }
                    else
                    {
                        ClearRow(gem.X, gem.Y);
                    }
                }
                allGems[gem1.X, gem1.Y] = null;
                Destroy(gem1.gameObject);
            }
            else if (gem2.Types == GemType.SmallBomb)
            {
                List<Gem> selected = ColorComboSpawn(bombCount, gem2);
                foreach (Gem gem in selected)
                {
                    ClearArea(gem.X, gem.Y, 1);
                }
            }
            else if (gem2.Types == GemType.Color)
            {
                WoodenCrate tempCrate;
                Rope tempRope;
                for (int i = 0; i < width; i++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (map[i, y] == '2')
                        {
                            tempCrate = findCrate(i, y);
                            if(tempCrate != null)
                            {
                                tempCrate.CrateHealth--;
                            }
                            continue;
                        }
                        else if (map[i, y] == '3')
                        {
                            tempRope = findRope(i, y);
                            if(tempRope != null)
                            {
                                tempRope.RopeState--;
                            }
                            continue;
                        }
                            Destroy(allGems[i, y].gameObject);
                        allGems[i, y] = null;
                    }
                }
            }
        }
    }

    void Suffle<T>(List<T> list)
    {
        for(int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    List<Gem> GetValidCells()
    {
        List<Gem> cells = new List<Gem>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allGems[i, j] == null || allGems[i, j].IsGemBonus || map[i, j] == '2' || map[i, j] == '3')
                {
                    continue;
                }
                cells.Add(allGems[i, j]);
            }
        }
        return cells;
    }

    List<Gem> ColorComboSpawn(int count, Gem gemCollab)
    {
        List<Gem> list = GetValidCells();
        List<Gem> selected = new List<Gem>();
        Suffle<Gem>(list);
        if(gemCollab.Types == GemType.SmallBomb)
        {
            if(list.Count < bombCount)
            {
                count = list.Count;
            }
        }
        else
        {
            if(list.Count < rocketCount)
            {
                count = list.Count;
            }
        }
            for (int i = 0; i < count; i++)
            {

                if (i == 0)
                {
                    selected.Add(gemCollab);
                }
                else
                {
                    bool ok = false;
                    foreach (var gem in list)
                    {
                        if (Mathf.Abs(gem.X - list[i].X) + Mathf.Abs(gem.Y - list[i].Y) <= 1)
                        {
                            ok = true;
                            break;
                        }
                    }
                    if (ok)
                    {
                        selected.Add(list[i]);
                    }
                }

                if (selected.Count == count)
                {
                    break;
                }
            }
        foreach (var gem in selected)
        {
            if(gemCollab.Types == GemType.Vertical)
            {
                gem.Types = UnityEngine.Random.value < 0.7f ? GemType.Vertical : GemType.Horizontal;
            }
            else if(gemCollab.Types == GemType.Horizontal)
            {
                gem.Types = UnityEngine.Random.value < 0.7f ? GemType.Horizontal : GemType.Vertical;
            }
            else
            {
                gem.Types = GemType.SmallBomb;
            }
        }
        return selected;
    }

    Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(
            x * tileSpacing - (width * tileSpacing) / 2f + tileSpacing / 2f,
            y * tileSpacing - (height * tileSpacing) / 2f + tileSpacing / 2f);
    }

    List<Gem> GetMatchHorizontal(int x, int y)
    {
        List<Gem> match = new List<Gem>();
        Gem start = allGems[x, y];
        if(start == null || start.IsGemBonus)
        {
            return match;
        }
        match.Add(start);
        int left = x - 1;
        while(left >= 0 && allGems[left, y] != null && allGems[left, y].Types == start.Types)
        {
            match.Add(allGems[left, y]);
            left--;
        }
        int right = x + 1;
        while (right < width && allGems[right, y] != null && allGems[right, y].Types == start.Types)
        {
            match.Add(allGems[right, y]);
            right++;
        }
        return match;
    }

    List<Gem> GetMatchVertical(int x, int y)
    {
        List<Gem> match = new List<Gem>();
        Gem start = allGems[x, y];
        if(start == null || start.IsGemBonus)
        {
            return match;
        }
        match.Add(start);
        int down = y - 1;
        while(down >= 0 && allGems[x, down] != null && allGems[x, down].Types == start.Types)
        {
            match.Add(allGems[x, down]);
            down--;
        }
        int up = y + 1;
        while(up < height && allGems[x, up] != null && allGems[x, up].Types == start.Types)
        {
            match.Add(allGems[x, up]);
            up++;
        }
        return match;
    }

    HashSet<Gem> CheckMatchAt(Gem gem)
    {
        HashSet<Gem> matches = new HashSet<Gem>();
        var horizontal = GetMatchHorizontal(gem.X, gem.Y);
        var vertical = GetMatchVertical(gem.X, gem.Y);
        if (horizontal.Count >= 3 && vertical.Count >= 3)
        {
            foreach(var x in horizontal)
            {
                matches.Add(x);
            }
            foreach(var y in vertical)
            {
                matches.Add(y);
            }
        }
        else if(horizontal.Count >= 3)
        {
            foreach(var x in horizontal)
            {
                matches.Add(x);
            }
        }
        else if (vertical.Count >= 3)
        {
            foreach (var y in vertical)
            {
                matches.Add(y);
            }
        }
        return matches;
    }

    List<HashSet<Gem>> FindMatches()
    {
        List<HashSet<Gem>> matches = new List<HashSet<Gem>>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Gem gem = allGems[x, y];
                if (gem == null || gem.IsGemBonus)
                {
                    continue;
                }
                if (x == 0 || allGems[x - 1, y]?.Types != allGems[x, y].Types)
                {
                    var horizontal = GetMatchHorizontal(x, y);
                    if(horizontal.Count >= 3)
                    {
                        matches.Add(new HashSet<Gem>(horizontal));
                    }
                }
                if(y == 0 || allGems[x, y - 1]?.Types != allGems[x, y].Types)
                {
                    var vertical = GetMatchVertical(x, y);
                    if(vertical.Count >= 3)
                    {
                        matches.Add(new HashSet<Gem>(vertical));
                    }
                }
            }
        }
        if (matches.Count > 1)
        {
            MergeMatches(matches);
            //Debug.Log("error");
        }
        return matches;
    }

    List<HashSet<Gem>> MergeMatches(List<HashSet<Gem>> matches)
    {
        bool merged = false;
        for(int i = 0; i < matches.Count - 1; i++)
        {
            for(int y = i+1; y < matches.Count; y++)
            {
                if (matches[i].Overlaps(matches[y]))
                {
                    matches[i].UnionWith(matches[y]);
                    matches.RemoveAt(y);
                    merged = true;
                    break;
                }
            }
        }
        if (merged)
        {
            return MergeMatches(matches);
        }
        return matches;
    }

    void DestroyMatches(List<Gem> matches, bool canBreakCrate)
    {
        HashSet<WoodenCrate> crates= new HashSet<WoodenCrate>();
        foreach(var gem in matches)
        {
            if (map[gem.X, gem.Y] != '1')
            {
                if (map[gem.X, gem.Y] == '3')
                {
                    Rope tempRope = null;
                    foreach (var rope in allRopes)
                    {
                        if (rope.X == gem.X && rope.Y == gem.Y)
                        {
                            tempRope = rope; break;
                        }
                    }
                    if (tempRope != null)
                    {
                        tempRope.RopeState--;
                        if(tempRope.RopeState == 0)
                        {
                            for(int i = 0; i < RuntimeGoals.Count; i++)
                            {
                                if (RuntimeGoals[i].GoalType == GoalType.Rope)
                                {
                                    RuntimeGoals[i].Current++;
                                    OnGemDestroyed?.Invoke(i);
                                    if (FinishGoal())
                                    {
                                        CurrentState = BoardState.Ending;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                continue;
            }
            else if(canBreakCrate)
            {
                crates = listCrate(gem.X, gem.Y);
                if (crates.Count > 0)
                {
                    foreach(var crate in crates)
                    {
                        crate.CrateHealth--;
                        if (crate.CrateHealth == 0)
                        {
                            for(int i = 0; i <RuntimeGoals.Count; i++)
                            {
                                if (RuntimeGoals[i].GoalType == GoalType.Crate)
                                {
                                    RuntimeGoals[i].Current++;
                                    OnGemDestroyed?.Invoke(i);
                                    if (FinishGoal())
                                    {
                                        CurrentState = BoardState.Ending;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            for(int i = 0; i < RuntimeGoals.Count; i++)
            {
                if (allGems[gem.X, gem.Y] == null)
                {
                    continue;
                }
                if (RuntimeGoals[i].GemType == allGems[gem.X, gem.Y].Types)
                {
                    RuntimeGoals[i].Current++;
                    if (FinishGoal())
                    {
                        CurrentState = BoardState.Ending;
                    }
                    OnGemDestroyed?.Invoke(i); break;
                }
            }
            allGems[gem.X, gem.Y] = null;
            Destroy(gem.gameObject);
        }
    }

    WoodenCrate findCrate(int x, int y)
    {
        foreach (var crate in allCrate)
        {
            if(x == crate.X && y == crate.Y) { return crate; }
        }
        return null;
    }

    Rope findRope(int x, int y)
    {
        foreach(var rope in allRopes)
        {
            if(x == rope.X && y == rope.Y)
            {
                return rope;
            }
        }
        return null;
    }

    HashSet<WoodenCrate> listCrate(int x, int y)
    {
        HashSet<WoodenCrate>crates = new HashSet<WoodenCrate>();
        WoodenCrate tempCrate;
        if (x + 1 < height)
        {
            if(map[x+1, y] == '2')
            {
                tempCrate = findCrate(x + 1, y);
                if(tempCrate != null)
                {
                    crates.Add(tempCrate);
                }
            }
        }
        if(y + 1 < height)
        {
            if (map[x, y+1] == '2')
            {
                tempCrate = findCrate(x, y+1);
                if (tempCrate != null)
                {
                    crates.Add(tempCrate);
                }
            }
        }
        if(x - 1 >= 0)
        {
            if (map[x - 1,y] == '2')
            {
                tempCrate = findCrate(x - 1, y);
                if (tempCrate != null)
                {
                    crates.Add(tempCrate);
                }
            }
        }
        if(y - 1 >= 0)
        {
            if (map[x, y - 1] == '2')
            {
                tempCrate = findCrate(x, y - 1);
                if (tempCrate != null)
                {
                    crates.Add(tempCrate);
                }
            }
        }
        return crates;
    }

    bool CausesMatch(int x, int y, GemType type)
    {
        if (x >= 2 &&
            allGems[x - 1, y] != null &&
            allGems[x - 2, y] != null &&
            allGems[x - 1, y].Types == type &&
            allGems[x - 2, y].Types == type)
        {
            return true;
        }
        if(y >= 2 &&
            allGems[x, y - 1] != null &&
            allGems[x, y - 2] != null &&
            allGems[x, y - 1].Types == type &&
            allGems[x, y - 2].Types == type)
        {
            return true;
        }
        return false;
    }

    IEnumerator CollapseColumsStep()
    {
        collapseMoved = false;
        bool moved = true;
        do
        {
            moved = false;
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    if(allGems[x, y] != null && map[x, y] == '1')
                    {
                        int tryDrop = TryDropGem(x, y);
                        if (tryDrop != 0)
                        {
                            if(tryDrop == 1)
                            {
                                StartCoroutine(allGems[x, y].MoveTo(GetWorldPosition(x, y - 1)));
                                allGems[x, y - 1] = allGems[x, y];
                                allGems[x, y - 1].SetCoordinates(x, y - 1);
                                allGems[x, y] = null;
                            }
                            else if (tryDrop == 2)
                            {
                                StartCoroutine(allGems[x, y].MoveTo(GetWorldPosition(x - 1, y - 1)));
                                allGems[x - 1, y - 1] = allGems[x, y];
                                allGems[x - 1, y - 1].SetCoordinates(x - 1, y - 1);
                                allGems[x, y] = null;
                            }
                            else
                            {
                                StartCoroutine(allGems[x, y].MoveTo(GetWorldPosition(x + 1, y - 1)));
                                allGems[x + 1, y - 1] = allGems[x, y];
                                allGems[x + 1, y - 1].SetCoordinates(x + 1, y - 1);
                                allGems[x, y] = null;
                            }
                            moved = true;
                            collapseMoved = true;
                        }
                    }
                }
            }
            yield return null;
        } while (moved);
    }

    private int TryDropGem(int x, int y)
    {
        if(CanFall(x, y, x, y - 1))
        {
            return 1; //down
        }
        bool leftFall = CanFall(x, y, x - 1, y - 1);
        bool rightFall = CanFall(x, y, x + 1, y - 1);
        if (leftFall && rightFall)
        {
            if (UnityEngine.Random.value > 0.5f)
            {
                return 2; //left
            }
            else
            {

                return 3; //right
            }
        }
        if (leftFall)
        {
            return 2;
        }
        if (rightFall)
        {
            return 3;
        }
        return 0;
    }

    private bool CanFall(int fromX, int fromY, int toX, int toY)
    {
        if(toX >= width || toX < 0 || toY >= height || toY < 0)
        {
            return false;
        }
        if (allGems[toX, toY] != null || map[toX, toY] != '1')
        {
            return false;
        }
        if(fromX != toX) 
        {
            if (map[toX, fromY] != '3' && map[toX, fromY] != '2')
            {
                return false;
            }
        }
        return true;
    }

    IEnumerator FillBoard()
    {
        for(int x = 0; x < width; x++)
        {
            fillSpawned = false;
            for(int y = height-1; y>= 0; y--)
            {
                if (allGems[x, y] == null && map[x, y] == '1')
                {
                    fillSpawned = true;
                    SpawnGem(x, y);

                }
                else
                {
                    break;
                }
            }
        }
        yield return null;
    }

    IEnumerator ProcessBoard()
    {
        bool changed;
        do
        {
            changed = false;
            yield return StartCoroutine(CollapseColumsStep());
            yield return new WaitUntil(() => AllGemsStopped());
            if (collapseMoved) changed = true;
            yield return StartCoroutine(FillBoard());
            yield return new WaitUntil(() => AllGemsStopped());
            if(fillSpawned) changed = true;
            yield return StartCoroutine(CollapseColumsStep());
            yield return new WaitUntil(() => AllGemsStopped());
            if (collapseMoved) changed = true;
        } while (changed);


        var matches = FindMatches();
        if(matches.Count > 0)
        {
            //Debug.Log("Still match");

            foreach(var match in matches)
            {
                SpawnGemBonus(match, null);
                DestroyMatches(new List<Gem>(match), true);
            }
            yield return StartCoroutine(ProcessBoard());
        }
        else
        {
            CurrentState = BoardState.Idle;
        }
    }

    bool isHorizontal(HashSet<Gem> match)
    {
        return match.All(g => g.Y == match.First().Y);
    }

    bool isVertical(HashSet<Gem> match) 
    {
        return match.All(g => g.X == match.First().X);
    }

    bool AllGemsStopped()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                if (allGems[x, y] != null && allGems[x, y].IsMoving)
                {
                    return false;
                }
            }
        }
        return true;
    }

    void SpawnGemBonus(HashSet<Gem> match, Gem gem)
    {
        if (gem == null && match.Count > 3)
        {
            if(isHorizontal(match) || isVertical(match))
            {
                var sorted = match.OrderBy(gem => gem.X).ThenBy(gem => gem.Y).ToList();
                int middleIndex = match.Count / 2;
                gem = sorted.ElementAt(middleIndex);
                if(map[gem.X, gem.Y] == '3')
                {
                    bool found = false;
                    for(int i = middleIndex+1; i < match.Count; i++)
                    {
                        gem = sorted.ElementAt(i);
                        if(map[gem.X, gem.Y] != '3')
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        for (int i = middleIndex - 1; i >= 0; i--)
                        {
                            gem = sorted.ElementAt(i);
                            if (map[gem.X, gem.Y] != '3')
                            {
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
                        gem = sorted.ElementAt(match.Count / 2);
                    }
                }
            }
            else
            {
                int maxConnection = 0;
                foreach(Gem tempGem in match)
                {
                    if (map[tempGem.X, tempGem.Y] == '3')
                    {
                        continue;
                    }
                    int count = 0;
                    foreach (Gem otherGem in match)
                    {
                        if(otherGem == tempGem || map[otherGem.X, otherGem.Y] == '3')
                        {
                            continue;
                        }
                        if(Mathf.Abs(tempGem.X - otherGem.X) == 1 && tempGem.Y == otherGem.Y ||
                            Mathf.Abs(tempGem.Y - otherGem.Y) == 1 && tempGem.X == otherGem.X)
                        {
                            count++;
                        }
                    }
                    if(count > maxConnection)
                    {
                        maxConnection = count;
                        gem = tempGem;
                    }
                }
            }
            if(gem == null)
            {
                gem = match.First();
            }
        }
        if(match.Count > 4)
        {
            if (isHorizontal(match) || isVertical(match))
            {
                gem.Types = GemType.Color;
            }
            else
            {
               gem.Types = GemType.SmallBomb;
            }
            match.Remove(gem);
        }
        else if(match.Count == 4)
        {
            if (isHorizontal(match))
            {
                gem.Types = GemType.Horizontal;
            }
            else
            {
                gem.Types = GemType.Vertical;
            }
            match.Remove(gem);
        }
    }

    void ClearColumn(int x, int y) 
    {
        if (allGems[x, y] == null || allGems[x, y].IsTriggered)
        {
            return;
        }
        allGems[x, y].IsTriggered = true;
        List<Gem> gemList = new List<Gem>();
        for(int i = 0; i < height; i++)
        {
            if (allGems[x, i] == null)
            {
                if (map[x, i] == '2')
                {
                    WoodenCrate crate = findCrate(x, i);
                    if (crate != null)
                    {
                        crate.CrateHealth--;
                    }
                }
                continue;
            }
            if (allGems[x, i].Types == GemType.Color)
            {
                continue;
            }
            if (allGems[x, i].Types == GemType.Vertical && i != y)
            {
                if (!allGems[x, i].IsTriggered)
                {
                    ClearColumn(x, i);
                }
                continue;
            }
            if (allGems[x, i].Types == GemType.Horizontal)
            {
                if (!allGems[x, i].IsTriggered)
                {
                    ClearRow(x, i);
                }
                continue;
            }
            if (allGems[x, i].Types == GemType.SmallBomb)
            {
                if (!allGems[x, i].IsTriggered)
                {
                    ClearArea(x, i, 1);
                }
                continue;
            }
            gemList.Add(allGems[x, i]);
        }
        AudioManager.Instance.OnRocket();
        DestroyMatches(gemList, false);
    }

    void ClearRow(int x, int y)
    {
        if (allGems[x, y] == null || allGems[x, y].IsTriggered)
        {
            return;
        }
        allGems[x, y].IsTriggered = true;
        List<Gem> gemList = new List<Gem>();
        for (int i = 0; i < width; i++)
        {
            if (allGems[i, y] == null)
            {
                if (map[i, y] == '2')
                {
                    WoodenCrate crate = findCrate(i, y);
                    if (crate != null)
                    {
                        crate.CrateHealth--;
                    }
                }
                continue;
            }
            if (allGems[i, y].Types == GemType.Color)
            {
                continue;
            }
            if (allGems[i, y].Types == GemType.Vertical)
            {
                if (!allGems[i, y].IsTriggered)
                {
                    ClearColumn(i, y);
                }
                continue;
            }
            if (allGems[i, y].Types == GemType.Horizontal && i != x)
            {
                if(!allGems[i, y].IsTriggered)
                {
                    ClearRow(i, y);
                }
                continue;
            }
            if (allGems[i, y].Types == GemType.SmallBomb)
            {
                if (!allGems[i, y].IsTriggered)
                {
                    ClearArea(i, y, 1);
                }
                continue;
            }
            gemList.Add(allGems[i, y]);
        }
        AudioManager.Instance.OnRocket();
        DestroyMatches(gemList, false);
    }

    void ClearColor(Gem gemColor, Gem gem)
    {
        List<Gem> gemList = new List<Gem>();
        if (gem.Types == GemType.SmallBomb) 
        {
            return;
        }
        else if (gem.Types == GemType.Vertical)
        {
            return;
        }
        else if(gem.Types == GemType.Horizontal)
        {
            return;
        }
        else if(gem.Types == GemType.Color)
        {
            return;
        }
        else
        {
            for (int i = 0; i < width; i++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (allGems[i, y] == null)
                    {
                        continue;
                    }
                    else if (allGems[i, y].Types == gem.Types)
                    {
                        gemList.Add(allGems[i, y]);
                    }
                }
            }
        }
        gemList.Add(gemColor);
        DestroyMatches(gemList, false);
    }

    void ClearArea(int x, int y, int radius)
    {
        if (allGems[x, y] == null || allGems[x, y].IsTriggered)
        {
            return;
        }
        allGems[x, y].IsTriggered = true;
        List<Gem> gemList = new List<Gem>();
        for(int i = - radius; i < 1 + radius; i++)
        {
            for(int z = - radius; z < 1 + radius; z++)
            {
                int X = x + i;
                int Y = y + z;
                if(X >= width || X < 0 || Y >= height || Y < 0)
                {
                    continue;
                }
                if (allGems[X, Y] == null)
                {
                    continue;
                }
                if (allGems[X, Y].Types == GemType.Vertical)
                {
                    if (!allGems[X, Y].IsTriggered)
                    {
                        ClearColumn(X, Y);
                    }
                    continue;                   
                }
                if (allGems[X, Y].Types == GemType.Horizontal)
                {
                    if (!allGems[X, Y].IsTriggered)
                    {
                        ClearRow(X, Y);
                    }
                    continue;
                }
                if (allGems[X, Y].Types == GemType.SmallBomb && X != x && Y != y)
                {
                    if(!allGems[X, Y].IsTriggered)
                    {
                        ClearArea(X, Y, 1);
                    }
                    continue;
                }
                gemList.Add(allGems[X, Y]);
            }
        }
        AudioManager.Instance.OnBomb();
        DestroyMatches(gemList, true);
    }
}
