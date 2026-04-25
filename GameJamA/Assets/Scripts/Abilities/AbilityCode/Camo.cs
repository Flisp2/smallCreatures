using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Camo")]
public class Camo : Ability
{
    public int camoDuration = 5;

    public override void Use(GameObject user)
    {
        if (timer > 0f) return; 
        PlayerCode playerCode = user.GetComponent<PlayerCode>();
        SpriteRenderer sr = user.GetComponent<SpriteRenderer>();
        if (playerCode != null)
        {
            playerCode.isHidden = true;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.5f); // Make the player semi-transparent
            if (level >= 2)
            {
                playerCode.speed += 2f * (level - 1); // Increase speed by 2 for each level above 1
            }
            playerCode.StartCoroutine(UnhideCoroutine(playerCode, camoDuration + (2 * level)));
        }
        timer = cooldownTime - (0.1f * level);
    }

    private System.Collections.IEnumerator UnhideCoroutine(PlayerCode playerCode, float delay)
    {
        yield return new WaitForSeconds(delay);
        SpriteRenderer sr = playerCode.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f); // Restore the player's opacity
        }
        playerCode.isHidden = false;
        if (level >= 2)
        {
            playerCode.speed = playerCode.baseSpeed; // Revert speed increase
        }
    }
}
