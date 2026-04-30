using UnityEngine;

public class ChoiceHolder : MonoBehaviour
{
    public Ability Ability { get; set; }
    private GameObject choiceButton;
    private GameObject choiceText;

    private void Start()
    {
        choiceButton = transform.Find("ChoiceButton").Find("ChoiceButtonImage").gameObject;
        choiceText = transform.Find("ChoiceText").gameObject;
    }
    private void Update()
    {
        var image = choiceButton.GetComponent<UnityEngine.UI.Image>();
        var textMesh = choiceText.GetComponent<TMPro.TextMeshProUGUI>();
        if (Ability != null && Ability.icon != null && Ability.abilityName != null)
        {
            image.sprite = Ability.icon;
            textMesh.text = Ability.abilityName;
        } 
        else
        {
            image.sprite = null;
            textMesh.text = "TEMP";
        }
    }
}
