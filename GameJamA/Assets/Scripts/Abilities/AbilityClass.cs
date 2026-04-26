using UnityEngine;

public class Ability : ScriptableObject
{
    public string abilityName;
    public float cooldownTime;
    public float timer;
    public int level;
    public Sprite icon;

    public virtual void Use(GameObject user)
    {

    }
    public void UpdateTimer()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
        }
    }
}
