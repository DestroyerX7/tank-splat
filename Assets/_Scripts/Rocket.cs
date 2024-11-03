using UnityEngine;

public class Rocket : Bullet
{
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        _rb.AddForce(transform.up * 25);
    }

    public override Vector2 CalculatePosAfterTime(Vector2 startPos, Vector2 startVelocity, float time)
    {
        float currentTime = 0;
        Vector2 currentPos = startPos;
        Vector2 currentVelocity = startVelocity;

        while (currentTime < time)
        {
            currentVelocity += 25 * Time.fixedDeltaTime * (Vector2)transform.up;
            currentPos += currentVelocity * Time.fixedDeltaTime;
            currentTime += Time.fixedDeltaTime;
        }

        return currentPos;
    }
}
