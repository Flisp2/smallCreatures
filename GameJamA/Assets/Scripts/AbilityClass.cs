using UnityEngine;

public class Ability : ScriptableObject
{
    public string abilityName;
    public float cooldownTime;
    public int level;

    public virtual void Use(GameObject user)
    {
        // Implement ability activation logic here
    }

}
