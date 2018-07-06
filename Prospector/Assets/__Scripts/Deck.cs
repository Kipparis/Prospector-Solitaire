using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour {
    // Масть
    public Sprite suitClub;
    public Sprite suitDiamond;
    public Sprite suitHeart;
    public Sprite suitSpade;

    public Sprite[] faceSprites;
    public Sprite[] rankSprites;

    public Sprite cardBack;
    public Sprite cardBackGold;
    public Sprite cardFront;
    public Sprite cardFrontGold;

    // Предзагрузки
    public GameObject prefabSprite;
    public GameObject prefabCard;

    public bool ______________;

    public PT_XMLReader xmlr;
    public List<string> cardNames;
    public List<Card> cards;
    public List<Decorator> decorators;
    public List<CardDefinition> cardDefs;
    public Transform deckAnchor;
    public Dictionary<string, Sprite> dictSuits;

    // InitDeck Вызывается проспектором когда он готов
    public void InitDeck(string deckXMLText) {
        // Это создаст якорь для всех карт в иерархии
        if(GameObject.Find("_Deck") == null) {
            GameObject anchorGO = new GameObject("_Deck");
            deckAnchor = anchorGO.transform;
        }

        // Создаём словарь строка-спрайт
        dictSuits = new Dictionary<string, Sprite>() {
            {"C", suitClub },
            {"D", suitDiamond },
            {"H", suitHeart },
            {"S", suitSpade }
        };

        ReadDeck(deckXMLText);
        MakeCards();
    }

    // ReadDeck переводит XML файл в CardDefinitions
    public void ReadDeck(string deckXMLText) {
        xmlr = new PT_XMLReader();  // Создаём новую читалку
        xmlr.Parse(deckXMLText);    // Используем его чтобы считать

        // Это выводит тестовую линию чтобы показать как xmlr работает
        string s = "xml[0] decorator[0] ";
        s += "type=" + xmlr.xml["xml"][0]["decorator"][0].att("type");
        s += " x=" + xmlr.xml["xml"][0]["decorator"][0].att("x");
        s += " y=" + xmlr.xml["xml"][0]["decorator"][0].att("y");
        s += " scale=" + xmlr.xml["xml"][0]["decorator"][0].att("scale");
        // print(s);    // Закомментировали т.к. мы закончили с тестом

        // Считываем знаки для всех карт
        decorators = new List<Decorator>(); // Создаём список
        // Выбираем все декораторы из XML файла
        PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
        Decorator deco;
        for (int i = 0; i < xDecos.Count; i++) {
            // Для каждого декоратора в XML
            deco = new Decorator(); // Создаём новый декоратор
            // Копируем аттрибуты
            deco.type = xDecos[i].att("type");
            deco.flip = (xDecos[i].att("flip") == "1");
            deco.scale = float.Parse(xDecos[i].att("scale"));
            // loc создаётся в 0,0,0 так что нам нужно только изменить его
            deco.loc.x = float.Parse(xDecos[i].att("x"));
            deco.loc.y = float.Parse(xDecos[i].att("y"));
            deco.loc.z = float.Parse(xDecos[i].att("z"));
            // ДОбавляем временной декоратор в список
            decorators.Add(deco);
        }

        // Считываем расположение указателей для каждого ранга карты
        cardDefs = new List<CardDefinition>();  // Создаём список карт
        // Забираем PT_XMLHashList для всех карт в XML файле
        PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];
        for (int i = 0; i < xCardDefs.Count; i++) {
            // Для каждой из карт
            // Создаём новое значение
            CardDefinition cDef = new CardDefinition();
            // Передаём аттрибуты
            cDef.rank = int.Parse(xCardDefs[i].att("rank"));
            // Забираем все указатели для этих карт
            PT_XMLHashList xPips = xCardDefs[i]["pip"];
            if (xPips != null) {
                for (int j = 0; j < xPips.Count; j++) {
                    // Повторяем через все указатели
                    deco = new Decorator();
                    deco.type = "pip";
                    deco.flip = (xPips[j].att("flip") == "1");
                    deco.loc.x = float.Parse(xPips[j].att("x"));
                    deco.loc.y = float.Parse(xPips[j].att("y"));
                    deco.loc.z = float.Parse(xPips[j].att("z"));
                    if (xPips[j].HasAtt("scale")) {
                        deco.scale = float.Parse(xPips[j].att("scale"));
                    }
                    cDef.pips.Add(deco);
                }
            }
            // Карты выше 10 имеют лицевую часть
            if (xCardDefs[i].HasAtt("face")) {
                cDef.face = xCardDefs[i].att("face");
            }
            cardDefs.Add(cDef);
        }
    }

    public CardDefinition GetCardDefinition(int rank) {
        // Ищем через все значения
        foreach (CardDefinition cd in cardDefs) {
            if(cd.rank == rank) {
                return (cd);
            }
        }
        return (null);
    }

    // Создаёт игровые объекты карт
    public void MakeCards() {
        // cardNames - имена карт чтобы их создать
        cardNames = new List<string>();
        string[] letters = new string[] { "C", "D", "H", "S" };
        foreach (string s in letters) {
            for (int i = 0; i < 13; i++) {  // cardDefs.Count
                cardNames.Add(s + (i + 1));
            }
        }

        // Создаём лист содержащий все карты
        cards = new List<Card>();
        // Несколько переменных которые будут переиспользованны
        Sprite tS = null;
        GameObject tGO = null;
        SpriteRenderer tSR = null;

        for (int i = 0; i < cardNames.Count; i++) {
            // Циклим через все именна карт что мы только что сделали
            GameObject cGO = Instantiate(prefabCard) as GameObject;
            // Задаём родителя
            cGO.transform.parent = deckAnchor;
            Card card = cGO.GetComponent<Card>();

            // Это просто будет собирать карты чтобы они были в хорошеньком порядке
            cGO.transform.localPosition = new Vector3((i % 13) * 3, i / 13 * 4, 0);

            // Придаём базовые значения карте
            card.name = cardNames[i];
            card.suit = card.name[0].ToString();
            card.rank = int.Parse(card.name.Substring(1));
            if(card.suit == "D" || card.suit == "H") {
                card.colS = "Red";
                card.color = Color.red;
            }
            // Находим обозначение для этой карты
            card.def = GetCardDefinition(card.rank);

            // Добавляем декораторы
            foreach (Decorator deco in decorators) {
                if(deco.type == "suit") {
                    // Создаём спрайт
                    tGO = Instantiate(prefabSprite) as GameObject;
                    // Достаём спрайт рендерер
                    tSR = tGO.GetComponent<SpriteRenderer>();
                    // Задаём спрайт
                    tSR.sprite = dictSuits[card.suit];
                } else {
                    // Это не масть значит ранг
                    tGO = Instantiate(prefabSprite) as GameObject;
                    tSR = tGO.GetComponent<SpriteRenderer>();
                    // Достаём нужный спрайт чтоб показать ранг
                    tS = rankSprites[card.rank];
                    tSR.sprite = tS;
                    // Задаём цвет
                    tSR.color = card.color;
                }
                // Делаем декорации выше карты
                tSR.sortingOrder = 1;
                // Делаем декорацию детём карты
                tGO.transform.parent = cGO.transform;
                // Читаем позицию на основе DeckXML
                tGO.transform.localPosition = deco.loc;
                // Переворачиваем если нужно
                if (deco.flip) {
                    tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
                }
                // Можно убрать условие и просто умножить....
                if(deco.scale != 1) {
                    tGO.transform.localScale = Vector3.one * deco.scale;
                }
                // Именнуем
                tGO.name = deco.type;
                // Добавляем декоратор в лист карт
                card.decoGOs.Add(tGO);
            }

            // Добавляем значки
            foreach (Decorator pip in card.def.pips) {
                // Создаёт спрайт
                tGO = Instantiate(prefabSprite) as GameObject;
                // Задаём родителя
                tGO.transform.parent = cGO.transform;
                // Задаём позицию
                tGO.transform.localPosition = pip.loc;
                // Переворачиваем если нужно
                if (pip.flip) {
                    tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
                }
                // Увеличиваем если нужно
                if(pip.scale != 1) {
                    tGO.transform.localScale = Vector3.one * pip.scale;
                }
                // Даём имя
                tGO.name = "pip";
                // Извлекаем спрайт рендерер
                tSR = tGO.GetComponent<SpriteRenderer>();
                // Придаём туда спрайт
                tSR.sprite = dictSuits[card.suit];
                // Рендерится над картой
                tSR.sortingOrder = 1;
                // Добавляем это в список карты
                card.pipGOs.Add(tGO);
            }

            // ДОбавляем лицевую сторону
            if(card.def.face != "") { // Если у карты есть лицо
                tGO = Instantiate(prefabSprite) as GameObject;
                tSR = tGO.GetComponent<SpriteRenderer>();
                tS = GetFace(card.def.face + card.suit);    // Находим лицевой спрайт
                tSR.sprite = tS;    // Придаём его к карте
                tSR.sortingOrder = 1;   // Делаем так чтоб рендерирлся выше всех
                tGO.transform.parent = cGO.transform;
                tGO.transform.localPosition = Vector3.zero;
                tGO.name = "face";
            }

            // Добавляем заднюю часть карты
            tGO = Instantiate(prefabSprite) as GameObject;
            tGO.transform.parent = cGO.transform;
            tGO.transform.localPosition = Vector3.zero;
            tSR = tGO.GetComponent<SpriteRenderer>();
            tSR.sprite = cardBack;
            tSR.sortingOrder = 2;   // Выше всех
            card.name = "back";
            card.back = tGO;

            // Базово вверх
            card.faceUp = true;    // Используем свойство

            cards.Add(card);
        }
    }

    // Находим нужную лицевую карту
    public Sprite GetFace(string faceS) {
        foreach (Sprite tS in faceSprites) {
            if (tS.name == faceS) {
                return (tS);
            }
        }
        // Если ничего не найдено возвращаем null
        return (null);
    }
}
