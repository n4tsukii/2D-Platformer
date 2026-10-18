using UnityEngine;

public class HealthCollectable : MonoBehaviour
{
    [SerializeField] private float healthgain;
    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;
    private Animator anim;
    private BoxCollider2D boxCollider;

    void Awake()
    {
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (collision.GetComponent<Health>().currentHealth == collision.GetComponent<Health>().maxHealth)
            {
                // donothing
            }
            else
            {
                collision.GetComponent<Health>().addHealth(healthgain);
                SoundManager.instance.PlaySound(collectSound);
                anim.SetTrigger("collected");
                boxCollider.enabled = false;
            }
            
        }
    }
    public void Deactive()
    {
        gameObject.SetActive(false);
    }
}
