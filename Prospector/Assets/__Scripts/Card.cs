using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour {
    // Это реализуем позже
}

[System.Serializable]
public class Decorator {    // Типо знак
    // Класс содержит сведения о любом знаке или указатель
    public string type; // Для указатель тип pip
    public Vector3 loc; // Позиция спрайта на карте
    public bool flip = false;   // Нужно ли перевернуть спрайт вертикально
    public float scale = 1f;    // Увеличение спрайта
}

[System.Serializable]
public class CardDefinition {
    // Содержит инфу о любом ранге из карт
    public string face; // Спрайт чтобы показать лицевую часть (>10)
    public int rank;    // Ранг
    public List<Decorator> pips = new List<Decorator>();    // указатели
    // Так как знаки (маленькие значки под рангом) одинаковы во всех картах
    // указатели появляются только на нумерованных картах
}
