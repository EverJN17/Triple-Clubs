using System.Collections;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public SpriteRenderer sprite, border;
    public GameObject glow;
    public TileType type;
    public bool selected;
    public GameObject blockOver;

    public Color color;
    bool collapsing;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("HeightLine") && GameManager.Instance.uIManager.gameState == GameState.PLAYING)
        {
            GameManager.Instance.GameOver();
        }
        else if (collision.CompareTag("Tile"))
        {
            if (collision.gameObject.transform.position.y > transform.position.y)
                blockOver = collision.gameObject;
        }
    }

    void OnMouseDown()
    {
        //Debug.Log(type);
        if (GameManager.Instance.uIManager.gameState == GameState.PLAYING && !selected)
        {
            selected = GameManager.Instance.SelectTile(gameObject, type, color);
        }
        else if (GameManager.Instance.uIManager.gameState == GameState.PLAYING && selected)
        {
            GameManager.Instance.DeselectTile(gameObject);
            selected = false;
        }

        glow.SetActive(selected);
    }

    //reset tile
    public void ResetTile()
    {
        selected = false;
        glow.SetActive(selected);
    }

    //destroy tile
    public void DestroyTile()
    {
        if (blockOver != null)
            blockOver.GetComponent<Tile>().Collapse();

        Destroy(gameObject);
    }

    //collapse blocks
    public void Collapse()
    {
        StartCoroutine(DropTileDown());

        if (blockOver != null)
            blockOver.GetComponent<Tile>().Collapse();
    }

    //drop tile down
    IEnumerator DropTileDown()
    {
        collapsing = true;
        gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        yield return new WaitForSecondsRealtime(2f);
        gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        collapsing = false;
    }

    void Update()
    {
        if (GameManager.Instance.uIManager.gameState == GameState.PLAYING && !collapsing)
            transform.position = new Vector2(transform.position.x, transform.position.y + (GameManager.Instance.tileSpeed * Time.deltaTime));
    }

    public void SetTile(Color _color,int _type, Sprite _sprite)
    {
        color = _color;
        sprite.color = _color;
        border.color = _color;
        type = (TileType)_type;
        sprite.sprite = _sprite;
        collapsing = false;
    }
}
