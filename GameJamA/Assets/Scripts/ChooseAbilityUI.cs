using UnityEngine;

public class ChooseAbilityUI : MonoBehaviour
{
    private Ability currentAbility;
    private CellCode cellCode;
    public void SetUp(Ability ability, CellCode cellCode)
    {
        this.currentAbility = ability;
        this.cellCode = cellCode;
    }
    public void ChooseAbility(int choice)
    {
        if (choice == 1)
        {
            PlayerCode.ability1 = currentAbility;
            PlayerCode.ability1.level = 1;
        }
        else if (choice == 2)
        {
            PlayerCode.ability2 = currentAbility;
            PlayerCode.ability2.level = 1;
        }
        this.gameObject.SetActive(false);
        cellCode.HideChoiceUI();
    }
}
