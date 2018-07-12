using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum PlayerType {
    human,
    ai
}

// Отдельно взятый игрок в игре

// Делаем игрока видимым классом в проспекторе
[System.Serializable]
public class Player {

    public PlayerType type = PlayerType.ai;
    public int playerNum;

    public List<CardBartok> hand;   // Карты в руке игрока
    public SlotDef handSlotDef;

    // Добавляем карты в руку
    public CardBartok AddCard(CardBartok tCB) {
        if (hand == null) hand = new List<CardBartok>();

        // Добавляем карту в руку
        hand.Add(tCB);

        // Сортируем по рангу используя LINQ если это человек
        if(type == PlayerType.human) {
            CardBartok[] cards = hand.ToArray();

            // Затем LINQ запрос, похож на то что делать
            // foreach (CardBartok cd in cards) затем сортировать по рангу
            cards = cards.OrderBy(cd => cd.rank).ToArray();
            // Конвернитируем обратно в список
            hand = new List<CardBartok>(cards);
            // Note: LINQ операторы могут быть немного медленными, 
            // но т.к. делаем это редко, то можно
        }

        tCB.SetSortingLayerName("10");  // Ставит двигающуюся карту на верх
        tCB.eventualSortLayer = handSlotDef.layerName;

        FanHand();
        return (tCB);
    }

    // Убирает карту из руки
    public CardBartok RemoveCard(CardBartok cb) {
        hand.Remove(cb);
        FanHand();
        return (cb);
    }

    public void FanHand() {
        // startRot это родация вокруг Z оси первой карты
        float startRot = 0;
        startRot = handSlotDef.rot;
        if (hand.Count > 1) {
            startRot += Bartok.S.handFanDegrees * (hand.Count - 1) / 2;
        }
        // Затем каждая карта поворачиваемся на handFanDegrees чтобы разложиться веером

        // Двигаем карты к их новой позиции
        Vector3 pos;
        float rot;
        Quaternion rotQ;
        for (int i = 0; i < hand.Count; i++) {
            rot = startRot - Bartok.S.handFanDegrees * i;   // Вращаем вокруг Z
            rotQ = Quaternion.Euler(0, 0, rot);

            // Позиция карты наполовину выше самой высоты карты
            pos = Vector3.up * CardBartok.CARD_HEIGHT / 2f;
            // Умножения Quaternion на Vector3 даёт вектор который был на вращением и который был воёрнут
            pos = rotQ * pos;
            // Добавляем основную позицию для каждой руки
            pos += handSlotDef.pos;
            // Стакает карты в Z направлении, это не заметно, но защищает коллайдеры от перекрытия
            pos.z = -0.5f * i;

            // Убеждаемся что карты начнут двигаться сразу, если это не начало игры
            if(Bartok.S.phase != TurnPhase.idle) {
                hand[i].timeStart = 0;
            }

            // Задаёт позицию и поворот карты в руке
            hand[i].MoveTo(pos, rotQ);  // Говорим перемещаться
            hand[i].state = CBState.toHand; // Потом станет hand
            //hand[i].transform.localPosition = pos;
            //hand[i].transform.rotation = rotQ;
            //hand[i].state = CBState.hand;

            // Если type == PlayerType.human, карта будет лицом вверх
            hand[i].faceUp = (type == PlayerType.human);
            hand[i].eventualSortOrder = i * 4;
            //hand[i].SetSortOrder(4 * i);
        }
    }

    // TakeTurn() позволяет ИИ играть
    public void TakeTurn() {
        Utils.tr(Utils.RoundToPlaces(Time.time), "Player.TakeTurn");

        // Ничего не делаем если это человек
        if (type == PlayerType.human) return;

        Bartok.S.phase = TurnPhase.waiting;

        CardBartok cb;

        // Если это ии, находим допустимые ходы
        List<CardBartok> validCards = new List<CardBartok>();
        foreach (CardBartok tCB in hand) {
            if (Bartok.S.ValidPlay(tCB)) {
                validCards.Add(tCB);
            }
        }
        // Если нет валидных карт
        if (validCards.Count == 0) {
            cb = AddCard(Bartok.S.Draw());
            cb.callbackPlayer = this;
            return;
        }

        // Если есть какие то карты, выбираем одну
        cb = hand[Random.Range(0, hand.Count)];
        RemoveCard(cb);
        Bartok.S.MoveToTarget(cb);
        cb.callbackPlayer = this;
    }

    public void CBCallback(CardBartok tCB) {
        Utils.tr(Utils.RoundToPlaces(Time.time), "Player.CBCallback()", tCB.name, "Player " + playerNum);
        // Карта закончила движение, передаём ход
        Bartok.S.PassTurn();
    }
}
