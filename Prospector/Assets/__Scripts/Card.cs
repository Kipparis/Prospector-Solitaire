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

    // Список всех спрайт рендереров этого объекта и его детей
    public SpriteRenderer[] spriteRenderers;

    private void Start() {
        SetSortOrder(0);
    }

    public bool faceUp {
        get { return (!back.activeSelf); }
        set { back.SetActive(!value); }
    }

    // Рендеры есть? А если найду?
    public void PopulateSpriteRenderers() {
        // Если спрайт рендереры не существуют или пусти
        if(spriteRenderers == null || spriteRenderers.Length == 0) {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
    }

    // Задаёт сортирующий слой на всех спрайт рендерерах
    public void SetSortingLayerName(string tSLN) {
        PopulateSpriteRenderers();

        foreach (SpriteRenderer tSR in spriteRenderers) {
            tSR.sortingLayerName = tSLN;
        }
    }

    public void SetSortOrder(int sOrd) {
        PopulateSpriteRenderers();

        // Белый задний план карты sOrd
        // Указатели, значки, лицевая часть и т.д. sOrd + 1
        // Задняя часть карты sOrd + 2

        // Цикл  через все рендереры
        foreach (SpriteRenderer tSR in spriteRenderers) {
            if(tSR.gameObject == this.gameObject) {
                // Значит это лицевая часть
                tSR.sortingOrder = sOrd;
                continue;
            }
            // Каждый из детей именнован
            // switch основан на именах
            switch (tSR.gameObject.name) {
                case "back":
                    tSR.sortingOrder = sOrd + 2;
                    break;
                case "face":    // если имя "лицо"
                default:    // или ещё что нибудь
                    tSR.sortingOrder = sOrd + 1;
                    break;
            }
        }
    }

    virtual public void OnMouseUpAsButton() {
        //print(name);    // Когда нашали выводим её имя
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
