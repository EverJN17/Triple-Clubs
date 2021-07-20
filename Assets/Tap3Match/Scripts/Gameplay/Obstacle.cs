using UnityEngine;

public class Obstacle : MonoBehaviour
{
    float obstacleSpeed;
    float amplitude;
    float cosSpeed;

    float angle;
    float startX;

    //when obstacle hit bottom collider, then destroy it
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Bottom"))
            Destroy(gameObject);
    }

    public void InitOBstacle(float _obstacleSpeed, float _amplitude, float _cosSpeed)
    {
        obstacleSpeed = _obstacleSpeed;
        amplitude = _amplitude;
        cosSpeed = _cosSpeed;
        startX = transform.position.x;
    }

    void Update()
    {
        transform.position = new Vector2(startX + (Mathf.Cos(angle) * amplitude), transform.position.y - (obstacleSpeed * Time.deltaTime));
        angle += Time.deltaTime * cosSpeed;
    }
}
