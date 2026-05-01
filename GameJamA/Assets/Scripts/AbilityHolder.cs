using UnityEngine;
using TMPro;
using System.Text;
using System.Text;

public class AbilityHolder : MonoBehaviour
{
    private GameObject Image1;
    private GameObject Image2;
    private UnityEngine.UI.Image Cooldown1;
    private UnityEngine.UI.Image Cooldown2;
    private TextMeshProUGUI levelText1;
    private TextMeshProUGUI levelText2;
    private PlayerCode playerCode;
    private void Start()
    {
        Image1 = transform.Find("Image1").gameObject;
        Image2 = transform.Find("Image2").gameObject;
        Cooldown1 = Image1.transform.Find("Cooldown1").GetComponent<UnityEngine.UI.Image>();
        Cooldown2 = Image2.transform.Find("Cooldown2").GetComponent<UnityEngine.UI.Image>();
        levelText1 = GameObject.Find("LevelText1").GetComponent<TextMeshProUGUI>();
        levelText2 = GameObject.Find("LevelText2").GetComponent<TextMeshProUGUI>();
        playerCode = GameObject.FindWithTag("Player")?.GetComponent<PlayerCode>();
    }

    private void FixedUpdate()
    {
        if (playerCode == null)        {
            playerCode = GameObject.FindWithTag("Player")?.GetComponent<PlayerCode>();
            if (playerCode == null) return; // Still can't find player, skip this frame
        }
        Ability ability1 = PlayerCode.ability1;
        Ability ability2 = PlayerCode.ability2;
        if (ability1 != null)
        {
            Image1.GetComponent<UnityEngine.UI.Image>().sprite = ability1.icon;
            Cooldown1.fillAmount = ability1.timer / (ability1.cooldownTime - (0.5f * ability1.level));
            levelText1.text = ConvertToRomanNumeral(ability1.level);
        }
        else
        {
            Cooldown1.fillAmount = 0f;
            levelText1.text = "";
        }
        if (ability2 != null)
        {
            Image2.GetComponent<UnityEngine.UI.Image>().sprite = ability2.icon;
            Cooldown2.fillAmount = ability2.timer / (ability2.cooldownTime - (0.5f * ability2.level));
            levelText2.text = ConvertToRomanNumeral(ability2.level);
            
        }
        else
        {
            Cooldown2.fillAmount = 0f;
            levelText2.text = "";
        }
    }

    // Source - https://stackoverflow.com/q/7040289
// Posted by Kavithova L, modified by community. See post 'Timeline' for change history
// Retrieved 2026-04-30, License - CC BY-SA 4.0

static string ConvertToRomanNumeral(int number)
    {
        if (number < 1) return string.Empty;
        if (number >= 100) return ro.C + ConvertToRomanNumeral(number - 100);
        if (number >= 90) return ro.X + ro.C + ConvertToRomanNumeral(number - 90);
        if (number >= 50) return ro.L + ConvertToRomanNumeral(number - 50);
        if (number >= 40) return ro.X + ro.L + ConvertToRomanNumeral(number - 40);
        if (number >= 10) return ro.X + ConvertToRomanNumeral(number - 10);
        if (number >= 9) return ro.I + ro.X + ConvertToRomanNumeral(number - 9);
        if (number >= 5) return ro.V + ConvertToRomanNumeral(number - 5);
        if (number >= 4) return ro.I + ro.V + ConvertToRomanNumeral(number - 4);
        return ro.I + ConvertToRomanNumeral(number - 1);         
    }

}

public static class ro
{
    public const string I = "I";
    public const string V = "V";
    public const string X = "X";
    public const string L = "L";
    public const string C = "C";
}
