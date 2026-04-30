using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Spikes")]
public class Spikes : Ability
{
    public Sprite spikeSprite; 
    public float spikeDur = 3f;
    public override void Use(GameObject user)
    {
        if (timer > 0f) return;
        PlayerCode playerCode = user.GetComponent<PlayerCode>();
        if (playerCode != null)
        {
            GameObject spike = new GameObject("Spike");
            spike.transform.SetParent(user.transform);
            spike.transform.localPosition = Vector3.zero;
            SpriteRenderer sr = spike.AddComponent<SpriteRenderer>();
            sr.sprite = spikeSprite;
            sr.sortingOrder = -1;
            playerCode.StartCoroutine(pullbackspikes(playerCode, spikeDur + (2 * level)));
        } 

        timer = cooldownTime - (0.5f * level);
    }

    private System.Collections.IEnumerator pullbackspikes(PlayerCode playerCode, float delay)
    {
        yield return new WaitForSeconds(delay);
        Transform spikeTransform = playerCode.transform.Find("Spike");
        if (spikeTransform != null)
        {
            GameObject spike = spikeTransform.gameObject;
            Object.Destroy(spike);
        }
    }
}
