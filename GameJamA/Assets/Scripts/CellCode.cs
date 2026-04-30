using System.Collections.Generic;
using UnityEngine;


public class CellCode : MonoBehaviour
{
    public GameObject Pause;
    public GameObject Choice;
    private List<Ability> abilities = new List<Ability>();
    private List<Ability> pickedAbilities = new List<Ability>();
    private void Awake()
    {
        Pause.SetActive(false);
        Choice.SetActive(false);
        string path = Application.dataPath + "/Scripts/Abilities/AbilityObjects";
        foreach (string file in System.IO.Directory.GetFiles(path, "*.asset"))
        {
            string relativePath = "Assets" + file.Replace(Application.dataPath, "").Replace("\\", "/");
            Ability ability = UnityEditor.AssetDatabase.LoadAssetAtPath<Ability>(relativePath);
            if (ability != null)
            {
                abilities.Add(ability);
            }
        }
        Debug.Log($"Loaded {abilities.Count} abilities.");
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerCode playerCode = collision.GetComponent<PlayerCode>();
            if (playerCode != null)
            {
                Pause.SetActive(true);
                Choice.SetActive(true);
                pickedAbilities.Clear();
                RandomAbility(playerCode);
                RandomAbility(playerCode);
                Time.timeScale = 0f;
                ChoiceHolder choice1 = GameObject.Find("Choice1").GetComponent<ChoiceHolder>();
                ChoiceHolder choice2 = GameObject.Find("Choice2").GetComponent<ChoiceHolder>();
                choice1.Ability = pickedAbilities[0];
                choice2.Ability = pickedAbilities[1];
                choice1.enabled = true;
                choice2.enabled = true;

            }
        }
    }
    public void HideUI(GameObject chosenAbilityObject)
    {
        Time.timeScale = 1f;
        Ability chosenAbility = chosenAbilityObject.GetComponent<ChoiceHolder>().Ability;
        PlayerCode playerCode = GameObject.FindWithTag("Player").GetComponent<PlayerCode>();
        if (playerCode.ability1 == null && playerCode.ability2 == null)
        {
            playerCode.ability1 = chosenAbility;
        } 
        else if (playerCode.ability1 != null && playerCode.ability2 == null)
        {
            playerCode.ability2 = chosenAbility;
        } 
        else if (playerCode.ability1 != null && playerCode.ability2 != null)
        {
            Debug.LogWarning("Player already has 2 abilities. Cannot assign more.");
        }
        Pause.SetActive(false);
        Choice.SetActive(false);    
    }
    private void RandomAbility(PlayerCode playerCode)
    {
        List<Ability> available = abilities.FindAll(a => !pickedAbilities.Contains(a));
        if (available.Count == 0) return;
        Ability picked = available[Random.Range(0, available.Count)];
        pickedAbilities.Add(picked);
    }
}
