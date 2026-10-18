using UnityEngine;

public class MeleeEnemy : MonoBehaviour
{
    [Header("Attack Parameters ")]
    [SerializeField] private float atkCD;
    [SerializeField] private float range;
    [SerializeField] private int damage;

    [Header("Collider Parameters")]
    [SerializeField] private float colliderDistance;
    [SerializeField] private BoxCollider2D boxCollider;

    [Header("Player Layer")]
    [SerializeField] private LayerMask playerLayer;
    
    [Header("Audio")]
    [SerializeField] private AudioClip AttackSound;
    
    private Health playerHealth;
    private Animator anim;
    private EnemyPetrol enemyPetrol;

    private float cooldownTimer = Mathf.Infinity;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        enemyPetrol = GetComponentInParent<EnemyPetrol>();
    }
    private void Update()
    {
        cooldownTimer += Time.deltaTime;

        if (PlayerInSignt())
            if (cooldownTimer >= atkCD)
            {
                cooldownTimer = 0;
                SoundManager.instance.PlaySound(AttackSound);
                anim.SetTrigger("meleeAtk");
            }
        
        if (enemyPetrol != null) 
            enemyPetrol.enabled = !PlayerInSignt();
    }

    private bool PlayerInSignt()
    {
    
        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center - transform.right * range * transform.localScale.x * colliderDistance, 
            new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z), 
            0, 
            Vector2.left, 
            0, 
            playerLayer);
        if (hit.collider != null)
            playerHealth = hit.transform.GetComponent<Health>();
        return hit.collider != null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            boxCollider.bounds.center - transform.right * range * transform.localScale.x * colliderDistance, 
            new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z));
    }
    
    private void DamagePlayer()
    {
        if (PlayerInSignt())
        {
            playerHealth.TakeDamage(damage);
        }
    }
}
