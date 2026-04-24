using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Abilities/Ability")]
public class Ability : ScriptableObject
{
    public string abilityName;
    public float cooldownTime;
    public int level;
    public Sprite icon;
    public float boostAmount = 10f;

    public virtual void Use(GameObject user)
    {
        Rigidbody2D rb = user.GetComponent<Rigidbody2D>();
        PlayerCode playerCode = user.GetComponent<PlayerCode>();
        if (playerCode != null)
        {
            rb.AddForce(playerCode.direction * boostAmount, ForceMode2D.Impulse);
        }
    }
}
