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
                GameObject.Find("Choice1").GetComponent<AbilityHolder>().Ability = pickedAbilities[0];
                GameObject.Find("Choice2").GetComponent<AbilityHolder>().Ability = pickedAbilities[1];
            }
        }
    }
    public void HideUI()
    {
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
