using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character Data", menuName = "Data/CharacterDataScriptableObject", order = 1)]
public class CharacterDataScriptableObject : ScriptableObject
{
    [SerializeField] public int maxHealth = 100;
    [SerializeField] public int startingSpeed = 100;
    [SerializeField] public float gameboardVelocity = 100f;
    [SerializeField] public int damage = 20;
    [SerializeField] public CharacterClassTypes typeOfChar;
    [SerializeField] public GameObject visualPrefab;
    [SerializeField] public Sprite icon;
}
