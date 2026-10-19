using System.Collections;
using UnityEngine;

namespace Platformer
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Parameters")]
        [SerializeField] private float speed;
        [SerializeField] private float jumpPower;

        [Header("Coyote Time")]
        [SerializeField] private float coyoteTime;
        private float coyoteCounter;

        [Header("Multiple Jumps")]
        [SerializeField] private int extraJumps;
        private int jumpCounter;

        [Header("Wall Jumping")]
        [SerializeField] private float wallJumpX;
        [SerializeField] private float wallJumpY;
        [SerializeField] private float wallJumpLockTime = 0.2f;

        [Header("Layers")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask wallLayer;

        [Header("Sounds")]
        [SerializeField] private AudioClip jumpSound;

        private Rigidbody2D body;
        private Animator anim;
        private BoxCollider2D boxCollider;
        private float wallJumpCooldown;
        private float horizontalInput;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            boxCollider = GetComponent<BoxCollider2D>();
            wallJumpCooldown = wallJumpLockTime;
        }

        private void Update()
        {
            if (Time.timeScale == 0) return;
            horizontalInput = Input.GetAxis("Horizontal");

            if (horizontalInput > 0.01f)
                transform.localScale = Vector3.one;
            else if (horizontalInput < -0.01f)
                transform.localScale = new Vector3(-1, 1, 1);

            anim.SetBool("run", horizontalInput != 0);
            anim.SetBool("grounded", isGrounded());

            if (Input.GetKeyDown(KeyCode.Space))
                Jump();

            if (Input.GetKeyUp(KeyCode.Space) && body.linearVelocity.y > 0 && wallJumpCooldown >= wallJumpLockTime)
                body.linearVelocity = new Vector2(body.linearVelocity.x, body.linearVelocity.y / 2);

            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                DropThroughPlatform();

            // Let the wall jump push the player away before steering resumes.
            if (wallJumpCooldown < wallJumpLockTime)
                wallJumpCooldown += Time.deltaTime;
            else if (onWall())
            {
                body.gravityScale = 0;
                body.linearVelocity = Vector2.zero;
                jumpCounter = extraJumps;
            }
            else
            {
                body.gravityScale = 7;
                body.linearVelocity = new Vector2(horizontalInput * speed, body.linearVelocity.y);

                if (isGrounded())
                {
                    coyoteCounter = coyoteTime;
                    jumpCounter = extraJumps;
                }
                else
                    coyoteCounter -= Time.deltaTime;
            }
        }

        private void Jump()
        {
            if (coyoteCounter <= 0 && !onWall() && jumpCounter <= 0) return; 

            SoundManager.instance.PlaySound(jumpSound);

            if (onWall())
                WallJump();
            else
            {
                if (isGrounded())
                    body.linearVelocity = new Vector2(body.linearVelocity.x, jumpPower);
                else
                {
                    if (coyoteCounter > 0)
                        body.linearVelocity = new Vector2(body.linearVelocity.x, jumpPower);
                    else
                    {
                        if (jumpCounter > 0)
                        {
                            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpPower);
                            jumpCounter--;
                        }
                    }
                }

                coyoteCounter = 0;
            }
        }

        private void WallJump()
        {
            body.gravityScale = 7;
            body.linearVelocity = new Vector2(-Mathf.Sign(transform.localScale.x) * wallJumpX, wallJumpY);
            wallJumpCooldown = 0;
        }

        private void DropThroughPlatform()
        {
            RaycastHit2D hit = GroundBelow();
            if (hit.collider != null && hit.collider.usedByEffector && hit.collider.GetComponent<PlatformEffector2D>())
                StartCoroutine(IgnorePlatform(hit.collider));
        }

        private IEnumerator IgnorePlatform(Collider2D platform)
        {
            Physics2D.IgnoreCollision(boxCollider, platform, true);
            // Long enough to clear the strip, short enough to still land on a platform three tiles lower.
            yield return new WaitForSeconds(0.2f);
            Physics2D.IgnoreCollision(boxCollider, platform, false);
        }

        private bool isGrounded()
        {
            // Moving up means passing through a one-way platform, not standing on it.
            return body.linearVelocity.y <= 0.01f && GroundBelow().collider != null;
        }
        private RaycastHit2D GroundBelow()
        {
            // Only a thin strip under the feet counts, so ceilings and side walls never refill the jumps.
            // Wall tops count as ground.
            Bounds bounds = boxCollider.bounds;
            Vector2 feet = new Vector2(bounds.center.x, bounds.min.y + 0.05f);
            Vector2 size = new Vector2(bounds.size.x * 0.9f, 0.1f);
            return Physics2D.BoxCast(feet, size, 0, Vector2.down, 0.1f, groundLayer | wallLayer);
        }
        private bool onWall()
        {
            RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.1f, wallLayer);
            return raycastHit.collider != null;
        }
        public bool canAttack()
        {
            return horizontalInput == 0 && isGrounded() && !onWall();
        }
    }
}
