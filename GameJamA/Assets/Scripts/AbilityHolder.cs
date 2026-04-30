using UnityEngine;

public class AbilityHolder : MonoBehaviour
{
    private Ability ability1;
    private Ability ability2;
    private GameObject Image1;
    private GameObject Image2;
    private UnityEngine.UI.Image Cooldown1;
    private UnityEngine.UI.Image Cooldown2;
    private PlayerCode playerCode;
    private void Start()
    {
        Image1 = transform.Find("Image1").gameObject;
        Image2 = transform.Find("Image2").gameObject;
        Cooldown1 = Image1.transform.Find("Cooldown1").GetComponent<UnityEngine.UI.Image>();
        Cooldown2 = Image2.transform.Find("Cooldown2").GetComponent<UnityEngine.UI.Image>();
        playerCode = GameObject.FindWithTag("Player").GetComponent<PlayerCode>();
    }

    private void FixedUpdate()
    {
        ability1 = playerCode.ability1;
        ability2 = playerCode.ability2;
        if (ability1 != null)
        {
            Image1.GetComponent<UnityEngine.UI.Image>().sprite = ability1.icon;
            Cooldown1.fillAmount = ability1.timer / (ability1.cooldownTime - (0.5f * ability1.level));
        }
        if (ability2 != null)
        {
            Image2.GetComponent<UnityEngine.UI.Image>().sprite = ability2.icon;
            Cooldown2.fillAmount = ability2.timer / (ability2.cooldownTime - (0.5f * ability2.level));
            
        }
    }
}
