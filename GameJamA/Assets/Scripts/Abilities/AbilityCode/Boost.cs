using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Boost")]
public class Boost : Ability
{
    public float boostAmount = 10f;

    public override void Use(GameObject user)
    {
        if (timer > 0f) return; 
        Rigidbody2D rb = user.GetComponent<Rigidbody2D>();
        PlayerCode playerCode = user.GetComponent<PlayerCode>();
        if (playerCode != null)
        {
            rb.AddForce(playerCode.direction * (boostAmount + (10f * (level - 1))), ForceMode2D.Impulse);
        }
        timer = cooldownTime - (0.5f * level);
    }
}
