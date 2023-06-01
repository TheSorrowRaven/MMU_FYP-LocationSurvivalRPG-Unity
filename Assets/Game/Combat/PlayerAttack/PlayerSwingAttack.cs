using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSwingAttack : MonoBehaviour
{
    private static PlayerSwingAttack instance;
    public static PlayerSwingAttack Instance => instance;

    private static G G => G.Instance;
    private static CombatPlayer CombatPlayer => CombatPlayer.Instance;

    public Camera PACam;
    public Animator Animator;
    public AudioSource SwingAudio;

    [System.NonSerialized] private bool isAnimating;
    [System.NonSerialized] private float animationTimeCount;

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
        if (isAnimating)
        {
            animationTimeCount -= Time.deltaTime;
            if (animationTimeCount < 0)
            {
                if (CombatPlayer.TrySphereCastZombie(new(Screen.width / 2, Screen.height / 2), out CombatZombie zombie))
                {
                    CombatPlayer.HitZombieWithWeapon(zombie, Player.Instance.MeleeDamage);
                }
                isAnimating = false;
            }
        }

    }

    public void TrySwingWeapon()
    {
        if (cooldownCount >= cooldown)
        {
            SwingWeapon();
        }
    }

    private void SwingWeapon()
    {
        if (Player.Instance.TryConsumeStaminaToSwingWeapon())
        {
            const float hitTime = 0.1333333333f;

            isAnimating = true;
            animationTimeCount = hitTime;
            Animator.SetTrigger("Swing");
            SwingAudio.Play();

            cooldownCount = 0;
        }
    }

}
