using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
    [SerializeField] private float AtkCD;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject[] arrows;

    [Header("Audio")]
    [SerializeField] private AudioClip arrowSound;
    private float CDTimer;

    private void Attack()
    {
        CDTimer = 0;
        SoundManager.instance.PlaySound(arrowSound);
        arrows[FindArrow()].transform.position = firePoint.position;
        arrows[FindArrow()].GetComponent<EnemyProjectile>().ActivateProjectile(transform.localScale);
    }

    private int FindArrow()
    {
        for (int i = 0; i < arrows.Length; i++)
        {
            if (!arrows[i].activeInHierarchy)
                return i;
        }
        return 0;
    }
    private void Update()
    {
        CDTimer += Time.deltaTime;
        if (CDTimer >= AtkCD) 
            Attack();
    }
}
