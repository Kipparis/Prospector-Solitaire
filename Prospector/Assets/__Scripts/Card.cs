using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour {
    public string suit; // Масть карты (C,D,H или S)
    public int rank;    // Ранг карты
    public Color color = Color.black;   // Цвет чтобы окрасить знаки
    public string colS = "Black";   // Имя цвета

    // Список содержит все декораторы ио
    public List<GameObject> decoGOs = new List<GameObject>();
    // Лист содержит все значки
    public List<GameObject> pipGOs = new List<GameObject>();

    public GameObject back; // ио задней стороны
    public CardDefinition def;

    public bool faceUp {
        get { return (!back.activeSelf); }
        set { back.SetActive(!value); }
    }
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
