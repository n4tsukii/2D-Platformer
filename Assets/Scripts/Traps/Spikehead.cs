using System;
using UnityEngine;

public class Spikehead : EnemyDamage
{
    [Header("Spikehead Attributes")]
    [SerializeField] private float speed;
    [SerializeField] private float range;
    [SerializeField] private float checkdelay;
    [SerializeField] private LayerMask playerLayer;

    [Header("Audio")]
    [SerializeField] private AudioClip impactSound;

    private float checkTimer;
    private int attackDirection;
    private Animator anim;
    private Vector3 destination;
    private bool attacking;
    private Vector3[] directions = new Vector3[4];
    private void OnEnable()
    {
        Stop();
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (attacking) 
            transform.Translate(destination * Time.deltaTime * speed);
        else
        {
            checkTimer += Time.deltaTime;
            if (checkTimer > checkdelay) 
                CheckForPlayer();
        }
    }

    private void CheckForPlayer()
    {
        CalculateDirection();
        for (int i = 0; i < directions.Length; i++)
        {
            Debug.DrawRay(transform.position, directions[i], Color.red);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directions[i], range, playerLayer);

            if (hit.collider != null && !attacking)
            {
                attacking = true;
                destination = directions[i].normalized; 
                attackDirection = i;
                checkTimer = 0;
            }
        }
    }
    private void CalculateDirection()
    {
        directions[0] = transform.right * range;
        directions[1] = -transform.right * range;
        directions[2] = transform.up * range;
        directions[3] = -transform.up * range;
    }
    private void Stop()
    {
        destination = transform.position;
        attacking = false;

    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (attacking)
        {
            SoundManager.instance.PlaySound(impactSound);
            switch (attackDirection)
            {
                case 0: anim.Play("hit right", 0, 0f); break;
                case 1: anim.Play("hit left", 0, 0f); break;
                case 2: anim.Play("hit top", 0, 0f); break;
                case 3: anim.Play("hit bottom", 0, 0f); break;
            }
        }
        base.OnTriggerEnter2D(collision);
        Stop();
    }
    
}
