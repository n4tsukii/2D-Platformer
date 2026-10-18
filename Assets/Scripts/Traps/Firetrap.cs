using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class Firetrap : MonoBehaviour
{
    [Header("Firetrap Dmg")]
    [FormerlySerializedAs("dmg")]
    [SerializeField] private float damage;

    [Header("Firetrap Timers")]
    [SerializeField] private float activationDelay;
    [SerializeField] private float activeTime;
    
    [Header("Audio")]
    [SerializeField] private AudioClip firetrapSound;

    private Health playerHealth;
    private Animator anim;
    private SpriteRenderer spriteRend;
    private bool triggered, active;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRend = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        if (playerHealth != null && active)
            playerHealth.TakeDamage(damage);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            playerHealth = collision.GetComponent<Health>();
            if (!triggered)
                StartCoroutine("ActivateFiretrap");
            if (active)
                collision.GetComponent<Health>().TakeDamage(damage);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
            playerHealth = null;
    }
    private IEnumerator ActivateFiretrap()
    {
        triggered = true;
        spriteRend.color = Color.red;

        yield return new WaitForSeconds(activationDelay);
        spriteRend.color = Color.white;
        active = true;
        SoundManager.instance.PlaySound(firetrapSound);
        anim.SetBool("activated", true);
        
        yield return new WaitForSeconds(activeTime);
        active = false;
        triggered = false;
        anim.SetBool("activated", false);
    }
}
