using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; set; }

    public UIManager uIManager;
    public ScoreManager scoreManager;
    public GameObject[] tiles;
    public GameObject selectedTiles;
    public GameObject maxHeightLine;

    [Header("Game settings")]
    public bool randomColor; //false: same number same color, true: same number random color
    [Space(5)]
    public Color[] colorTable;
    [Space(5)]
    public GameObject tilePrefab;
    [Space(5)]
    public Sprite[] tilesSprites;
    [Space(5)]
    [Range(6,9)]
    public int tilesPerLine;
    [Space(5)]
    [Range(.05f,.25f)]
    public float tileSpeed;
    [Space(5)]
    public bool spawning, canSelect;

    List<GameObject> selectedTilesList = new List<GameObject>();
    float tileSize;
    Vector3 screenSize;
    GameObject lastTile;

    void Awake()
    {
        DontDestroyOnLoad(this);

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        Physics2D.gravity = new Vector2(0, -9.8f);

        Application.targetFrameRate = 30;

        maxHeightLine.SetActive(false);

        CalculateTileSize();

        CreateLine();
        spawning = true;
        canSelect = true;
    }

    void Update()
    {
        if (uIManager.gameState == GameState.PLAYING && Input.GetMouseButton(0))
        {
            if (uIManager.IsButton())
                return;

            selectedTiles.SetActive(true);
            maxHeightLine.SetActive(true);
        }
        else if (spawning)
        {
            if (lastTile.transform.position.y > -screenSize.y)
                CreateLine();
        }
    }

    //calculate tile size
    void CalculateTileSize()
    {
        screenSize = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        //Debug.Log(screenSize);
        //Debug.Log(tilePrefab.GetComponent<SpriteRenderer>().bounds.size.x);
        tileSize = (2 * screenSize.x) / tilesPerLine;
    }

    //create one line of tiles
    public void CreateLine()
    {
        GameObject tempTile;
        int randomTile;
        Color color;
        float spawnPosition;

        if (lastTile == null)
            spawnPosition = 3 - screenSize.y - tilePrefab.GetComponent<SpriteRenderer>().bounds.size.y / 2; //spawn first tile line 2 units higher than bottom screen
        else
            spawnPosition = lastTile.transform.position.y - lastTile.GetComponent<SpriteRenderer>().bounds.size.y;

        for (int i = 0; i < tilesPerLine; i++)
        {
            tempTile = Instantiate(tilePrefab);
            tempTile.transform.localScale = new Vector2(tileSize, tileSize);
            tempTile.transform.position = new Vector2( -screenSize.x + ((i + .5f) * tempTile.GetComponent<SpriteRenderer>().bounds.size.x), spawnPosition);
            randomTile = Random.Range(0, System.Enum.GetValues(typeof(TileType)).Length);

            if (randomColor)
                color = colorTable[Random.Range(0, colorTable.Length)];
            else
                color = colorTable[randomTile];

            tempTile.GetComponent<Tile>().SetTile(color, randomTile, tilesSprites[randomTile]);

            if (i == 0)
                lastTile = tempTile;
        }
    }

    //deselect selected tiles
    public void DeselectTiles()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].SetActive(false);
        }

        GameObject[] tiles2 = GameObject.FindGameObjectsWithTag("Tile");

        foreach (GameObject item in tiles2)
        {
            if (item.GetComponent<Tile>().selected)
                item.GetComponent<Tile>().ResetTile();
        }

        selectedTilesList.Clear();
    }

    //deselect selected tile
    public void DeselectTile(GameObject _tile)
    {
        if (selectedTilesList.Contains(_tile))
        {
            selectedTilesList.Remove(_tile);

        }
        UpdateGuiList();
    }

    public bool SelectTile(GameObject _tile, TileType _type, Color _color)
    {
        if (!canSelect || selectedTilesList.Count == 3)
            return false;

        //check if type can be added
        for (int i = 0; i < selectedTilesList.Count; i++)
        {
            if (selectedTilesList[i].GetComponent<Tile>().type != _type)
            {
                AudioManager.Instance.PlayEffects(AudioManager.Instance.wrongColor);
                return false;
            }
        }

        //add tile to selected list
        selectedTilesList.Add(_tile);
        AudioManager.Instance.PlayEffects(AudioManager.Instance.pickColor);

        UpdateGuiList();

        if (selectedTilesList.Count == 3)
            StartCoroutine(ClearSelectedTiles());

        return true;

    }

    //update gui list of selected tiles
    void UpdateGuiList()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].SetActive(false);
        }

        //update gui selected tiles list
        for (int i = 0; i < selectedTilesList.Count; i++)
        {
            tiles[i].GetComponent<Image>().color = selectedTilesList[i].GetComponent<Tile>().color;
            tiles[i].transform.GetChild(0).GetComponent<Image>().color = selectedTilesList[i].GetComponent<Tile>().color;
            tiles[i].transform.GetChild(0).GetComponent<Image>().sprite = tilesSprites[(int)selectedTilesList[i].GetComponent<Tile>().type];
            tiles[i].SetActive(true);
        }
    }

    //clear selected tiles
    IEnumerator ClearSelectedTiles()
    {
        canSelect = false;

        AudioManager.Instance.PlayEffects(AudioManager.Instance.clearBlocks);

        ScoreManager.Instance.UpdateScore(3);

        yield return new WaitForSecondsRealtime(.5f);

        foreach (GameObject item in selectedTilesList)
        {
            if (item.GetComponent<Tile>().selected)
            {
                item.GetComponent<Tile>().DestroyTile();
            }
        }

        selectedTilesList.Clear();

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].SetActive(false);
        }

        canSelect = true;
    }

    //restart game, reset scores
    public void RestartGame()
    {
        if (uIManager.gameState == GameState.PAUSED)
            Time.timeScale = 1;

        uIManager.ShowGameplay();
        scoreManager.ResetCurrentScore();

        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");

        foreach (GameObject item in tiles)
        {
            Destroy(item);
        }

        lastTile = null;
        DeselectTiles();
        selectedTiles.SetActive(true);
        maxHeightLine.SetActive(true);
        spawning = true;
        canSelect = true;
        CreateLine();
    }

    //clear all blocks from scene
    public void ClearScene()
    {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");

        foreach (GameObject item in tiles)
        {
            Destroy(item);
        }

        lastTile = null;
        DeselectTiles();
        spawning = true;
        canSelect = true;
        CreateLine();
    }

    //show game over gui
    public void GameOver()
    {
        if (uIManager.gameState == GameState.PLAYING)
        {
            selectedTilesList.Clear();
            AudioManager.Instance.PlayEffects(AudioManager.Instance.gameOver);
            uIManager.ShowGameOver();
            scoreManager.UpdateScoreGameover();
        }
    }
}
