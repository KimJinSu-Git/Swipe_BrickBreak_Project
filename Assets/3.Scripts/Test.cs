using UnityEngine;
using Bird.Ball;

namespace Bird.Test
{
    public class TestShooter : MonoBehaviour
    {
        [SerializeField] private BallManager ballManager;
        [SerializeField] private float shootPower = 800f;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ShootBall();
            }
        }

        private void ShootBall()
        {
            GameObject ball = ballManager.GetBall();
            ball.transform.position = Vector3.zero;

            if (ball.TryGetComponent(out Rigidbody2D rb))
            {
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(new Vector2(1, 1).normalized * shootPower);
            }
        }
    }
}