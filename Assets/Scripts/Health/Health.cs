using System.Collections;
using UnityEngine;


public class Health : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float startingHealth;

    [Header("iframe")]
    [SerializeField] private float IframeDuration;
    [SerializeField] private int numberOfFlashes;

    [Header("Components")]
    [SerializeField] private Behaviour[] components;

    [Header("Audio")]
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip dieSound;

    private SpriteRenderer spriteRenderer;
    public float maxHealth {get; private set;}
    public float currentHealth {get; private set;}
    private Animator anim;
    private bool dead, invulnerable;

    

    private void Awake()
    {
        dead = false;
        maxHealth = startingHealth;
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Update()
    {

    }
    public void TakeDamage(float _damage)
    {
        if (invulnerable || dead || _damage <= 0) return;
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);
        if (currentHealth > 0)
        {
            SoundManager.instance.PlaySound(hurtSound);
            anim.SetTrigger("hurt");
            StartCoroutine("Invulerability");
        } else
        {
            if (!dead)
            {
                anim.SetBool("grounded", true);
                anim.SetTrigger("die");
                SoundManager.instance.PlaySound(dieSound);
                foreach (Behaviour component in components)
                    if (component != null)
                        component.enabled = false;
                
                dead = true;
            }
        }
    }
    public void addHealth(float _heart) {
        currentHealth = Mathf.Clamp(currentHealth + _heart, 0, startingHealth);
    }

    public void Respawn()
    {
        StopAllCoroutines();
        dead = false;
        invulnerable = false;
        currentHealth = startingHealth;
        spriteRenderer.color = Color.white;
        anim.ResetTrigger("die");
        anim.ResetTrigger("hurt");
        anim.Play("Idle");

        foreach (Behaviour component in components)
            if (component != null)
                component.enabled = true;

        StartCoroutine(Invulerability());
    }
    private IEnumerator Invulerability()
    {
        invulnerable = true;
        for (int i = 0; i < numberOfFlashes; i++)
        {
            spriteRenderer.color = new Color(1, 0, 0, 0.5f);
            yield return new WaitForSeconds(IframeDuration / (numberOfFlashes * 2));
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(IframeDuration / (numberOfFlashes * 2));
        }
        invulnerable = false;
    }

    private void Deactive()
    {
        gameObject.SetActive(false);
    }
}
