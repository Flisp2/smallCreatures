using UnityEngine;

[CreateAssetMenu]
public class Boost : Ability
{
    public float boostAmount = 10f;

    public override void Use(GameObject user)
    {
        Rigidbody2D rb = user.GetComponent<Rigidbody2D>();
        PlayerCode playerCode = user.GetComponent<PlayerCode>();
        if (playerCode != null)
        {
            rb.AddForce(playerCode.direction * boostAmount, ForceMode2D.Impulse);
        }
    }
}
