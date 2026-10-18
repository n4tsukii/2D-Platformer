using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    [Header("Attack Parameters ")]
    [SerializeField] private float atkCD;
    [SerializeField] private float range;
    [SerializeField] private int damage;

    [Header("Collider Parameters")]
    [SerializeField] private float colliderDistance;
    [SerializeField] private BoxCollider2D boxCollider;

    [Header("Ranged Attack")]
    [SerializeField] private Transform firepoint;
    [SerializeField] private GameObject[] fireballs;

    [Header("Player Layer")]
    [SerializeField] private LayerMask playerLayer;
    private float cooldownTimer = Mathf.Infinity;

    [Header("Audio")]
    [SerializeField] private AudioClip AttackSound;

    private Animator anim;
    private EnemyPetrol enemyPetrol;

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
                anim.SetTrigger("rangedAtk");
            }
        
        if (enemyPetrol != null) 
            enemyPetrol.enabled = !PlayerInSignt();
    }

    private bool PlayerInSignt()
    {
        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance, 
            new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z), 
            0, 
            Vector2.left, 
            0, 
            playerLayer);
        return hit.collider != null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance, 
            new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z));
    }
    
    private void rangedAttack()
    {
        cooldownTimer = 0;
        fireballs[FindFireball()].transform.position = firepoint.position;
        fireballs[FindFireball()].transform.localScale = new Vector3 (transform.localScale.x, 1, 1);
        SoundManager.instance.PlaySound(AttackSound);
        fireballs[FindFireball()].GetComponent<EnemyProjectile>().ActivateProjectile(transform.localScale);
    }

    private int FindFireball()
    {
        for (int i = 0; i < fireballs.Length; i++)
        {
            if (!fireballs[i].activeInHierarchy)
                return i;
        }
        return 0;
    }
}
