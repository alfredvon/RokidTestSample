using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New CharacterBaseAttribute", menuName = "ScriptableObjects/CharacterBaseAttribute")]
public class CharacterBaseAttribute : ScriptableObject
{
    public int STR; // Strength ���� ����
    public int DEX; // Dexterity ���� ����
    public int CON; // Constitution ���� ����
    public int INT; // Intelligence ���� ����
    public int WIS; // Wisdom ���� ��֪
    public int CHA; // Charisma ���� ����
}
