using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// SlotDef не подкласс монобехэвиор, так что ему не нужно отдельного файла
//[System.Serializable]
//public class SlotDef {
//    public float x;
//    public float y;
//    public bool faceUp = false;
//    public string layerName = "Default";
//    public int layerID = 0;
//    public int id;
//    public List<int> hiddenBy = new List<int>();
//    public string type = "slot";
//    public Vector2 stagger;
//}

public class Layout : MonoBehaviour {
    public PT_XMLReader xmlr;   // XML считыватель
    public PT_XMLHashtable xml; // Переменная для более быстрого доступа
    public Vector2 multiplier;  // Задаём отступ для стола
    // Ссылки на SlotDef
    public List<SlotDef> slotDefs;  // Все слоты ряд0 - ряд3
    public SlotDef drawPile;
    public SlotDef discardPile;
    // Содержит все возможные именна для слоёв
    public string[] sortingLayerNames = new string[] {"Row0", "Row1",
    "Row2","Row3","Discard","Draw" };

    // Эта фукнция вызывается чтобы считать с LayoutXML.xml файла
    public void ReadLayout(string xmlText) {
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText);    // XML Обработанно
        xml = xmlr.xml["xml"][0];   // Используем как сокращения

        // Считываем множитель
        multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"));

        // Считываем слоты
        SlotDef tSD;
        // slotsX сокращение к xml["slot"]
        PT_XMLHashList slotsX = xml["slot"];

        for (int i = 0; i < slotsX.Count; i++) {
            tSD = new SlotDef();
            if (slotsX[i].HasAtt("type")) {
                // Если у слота есть тип, передаём
                tSD.type = slotsX[i].att("type");
            } else {
                // Если нет это базовый слот
                tSD.type = "slot";
            }
            // Различные аттрибуты передаются в цифровую форму
            tSD.x = float.Parse(slotsX[i].att("x"));
            tSD.y = float.Parse(slotsX[i].att("y"));
            tSD.layerID = int.Parse(slotsX[i].att("layer"));
            // Переводит номер слоя в layerName
            tSD.layerName = sortingLayerNames[tSD.layerID];
            // Слои используются для правильной отрисовки карт, т.к. они все расположенны
            // На одной и той же z глубине

            switch (tSD.type) { 
                case "slot":
                    tSD.faceUp = (slotsX[i].att("faceup") == "1");
                    tSD.id = int.Parse(slotsX[i].att("id"));
                    if (slotsX[i].HasAtt("hiddenby")) {
                        string[] hiding = slotsX[i].att("hiddenby").Split(',');
                        foreach (string s in hiding) {
                            tSD.hiddenBy.Add(int.Parse(s));
                        }
                    }
                    slotDefs.Add(tSD);
                    break;
                case "drawpile":
                    tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"));
                    drawPile = tSD;
                    break;
                case "discardpile":
                    discardPile = tSD;
                    break;
                default:
                    break;
            }
        }
    }
}
