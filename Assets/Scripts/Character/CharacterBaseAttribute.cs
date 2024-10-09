using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New CharacterBaseAttribute", menuName = "ScriptableObjects/CharacterBaseAttribute")]
public class CharacterBaseAttribute : ScriptableObject
{
    public int STR; // Strength —— 力量
    public int DEX; // Dexterity —— 敏捷
    public int CON; // Constitution —— 体质
    public int INT; // Intelligence —— 智力
    public int WIS; // Wisdom —— 感知
    public int CHA; // Charisma —— 魅力
}
