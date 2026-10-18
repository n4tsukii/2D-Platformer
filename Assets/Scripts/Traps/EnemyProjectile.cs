using System;
using UnityEngine;

public class EnemyProjectile : EnemyDamage
{
    [SerializeField] private float speed;
    [SerializeField] private float resetTime;
    private float lifetime;
    private Animator anim;
    private BoxCollider2D boxCollider;
    private float direction;
    private bool hit;
    private void OnEnable()
    {
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }
    public void ActivateProjectile(Vector3 localScale)
    {
        gameObject.SetActive(true);
        boxCollider.enabled = true;
        hit = false;
        direction = localScale.x;
        lifetime = 0;
    }

    private void Update()
    {
        if (hit) return;
        float movementSpeed = speed * Time.deltaTime * direction;
        transform.Translate(movementSpeed, 0, 0);
        lifetime += Time.deltaTime;
        
        if (lifetime > resetTime)
            gameObject.SetActive(false);
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        hit = true;
        base.OnTriggerEnter2D(collision);
        boxCollider.enabled = false;
        if (anim != null)
            anim.SetTrigger("explode");
        else
            gameObject.SetActive(false);
    }

    private void Deactive()
    {
        gameObject.SetActive(false);
    }
}
