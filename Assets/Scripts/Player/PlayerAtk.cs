using UnityEngine;

public class PlayerAtk : MonoBehaviour
{
    [SerializeField] private float atkCD;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject[] FireballPool;

    [SerializeField] private AudioClip fireballSound;


    private Animator anim;
    private PlayerMovement playerMovement;
    private float CDtimer = Mathf.Infinity;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }
    void Update()
    {
        if (Time.timeScale == 0) return;

        if (Input.GetMouseButton(0) && CDtimer > atkCD && playerMovement.canAtk())
            Attack(); 
        CDtimer += Time.deltaTime;

    }
    private void Attack()
    {
        SoundManager.instance.PlaySound(fireballSound);
        anim.SetTrigger("attack");
        CDtimer = 0;

        FireballPool[FindFireball()].transform.position = firePoint.position;
        FireballPool[FindFireball()].GetComponent<Projectile>().setDirection(Mathf.Sign(transform.localScale.x));
    }
    private int FindFireball()
    {
        for (int i = 0; i < FireballPool.Length; i++)
        {
            if (!FireballPool[i].activeInHierarchy)
                return i;
        }
        return 0;
    }
}
