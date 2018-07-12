using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SlotDef {
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public List<int> hiddenBy = new List<int>();    // Не используется в этой игре
    public float rot;   // Поворот руки
    public string type = "slot";
    public Vector2 stagger;
    public int player;  // Номер игрока руки
    public Vector3 pos; // Позиция полученна из х, у и множителя
}

public class BartokLayout : MonoBehaviour {
    // Фигня для чтения XML
    public PT_XMLReader xmlr;
    public PT_XMLHashtable xml; // Для быстрого доступа

    public Vector2 multiplier;  // Задаёт отступ для стола
    // Ссылка на позицию
    public List<SlotDef> slotDefs;  // Руки
    public SlotDef drawPile;
    public SlotDef discardPile;
    public SlotDef target;

	// Функция вызывается чтобы считать LayoutXML.xml файл
    public void ReadLayout(string xmlText) {
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText);    // XML Переданн
        xml = xmlr.xml["xml"][0];   // Быстрый доступ

        // Считываем множитель, который задаёт отступ
        multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"));

        // Считываем слоты
        SlotDef tSD;
        // slotsX используется как сокращение ко всем слотам
        PT_XMLHashList slotsX = xml["slot"];

        for (int i = 0; i < slotsX.Count; i++) {
            tSD = new SlotDef();    // Создаём новый объект класса
            if (slotsX[i].HasAtt("type")) {
                tSD.type = slotsX[i].att("type");
            } else {
                tSD.type = "slot";
            }

            // Различные аттрибуты передаются в числовые поля
            tSD.x = float.Parse(slotsX[i].att("x"));
            tSD.y = float.Parse(slotsX[i].att("y"));
            tSD.pos = new Vector3(tSD.x * multiplier.x, tSD.y * multiplier.y, 0);

            // Сортирующие слоя
            tSD.layerID = int.Parse(slotsX[i].att("layer"));
            tSD.layerName = tSD.layerID.ToString();

            // Добавляем аттрибуты в зависимости от типа слота
            switch (tSD.type) {
                case "slot":
                    // Уже всё добавленно
                    break;
                case "drawpile":
                    tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"));
                    drawPile = tSD;
                    break;
                case "discardpile":
                    discardPile = tSD;
                    break;
                case "target":
                    target = tSD;
                    break;
                case "hand":
                    // Информация о расположении руки игрока
                    tSD.player = int.Parse(slotsX[i].att("player"));
                    tSD.rot = float.Parse(slotsX[i].att("rot"));
                    slotDefs.Add(tSD);
                    break;
            }
        }
    }
}
