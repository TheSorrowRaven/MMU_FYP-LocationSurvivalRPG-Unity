using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerShootAttack : MonoBehaviour
{
    private static PlayerShootAttack instance;
    public static PlayerShootAttack Instance => instance;

    private static G G => G.Instance;
    private static CombatPlayer CombatPlayer => CombatPlayer.Instance;

    public Camera PlayerCam;
    public Animator Animator;
    public ParticleSystem ShootPS;
    public AudioSource ShootAudio;


    [SerializeField] private float cooldown;
    [System.NonSerialized] private float cooldownCount;

    private void Awake()
    {
        instance = this;
    }


    private void Update()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }
        cooldownCount += Time.deltaTime;

    }

    public void TryShootWeapon(RangedItem rangedItem)
    {
        if (cooldownCount >= cooldown)
        {
            ShootWeapon(rangedItem);
        }
    }

    private void ShootWeapon(RangedItem rangedItem)
    {
        const float distance = 1000f;

        if (UIInventory.Instance.InventoryHasItem(rangedItem.Ammo) <= 0)
        {
            // no ammo
            Debug.Log("No Ammo");
            return;
        }

        Ray ray = PlayerCam.ScreenPointToRay(new(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, distance, 1 << 8))
        {
            Transform tr = hit.collider.transform.parent;
            if (tr.TryGetComponent(out CombatZombie zombie))
            {
                CombatPlayer.HitZombieWithWeapon(zombie, Player.Instance.RangedDamage);
                Debug.Log($"Shot a zombie");
            }
            else
            {
                Debug.LogError("error??");
            }
        }
        else
        {
            Debug.Log("Miss");
        }
        Debug.DrawRay(ray.origin, ray.direction * 1000, Color.cyan, 1);
        UIInventory.Instance.RemoveFromInventory(rangedItem.Ammo);
        Animator.SetTrigger("Shoot");
        ShootPS.Play();
        ShootAudio.Play();
        cooldownCount = 0;
    }


}
