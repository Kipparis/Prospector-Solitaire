﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prospector : MonoBehaviour {
    static public Prospector S;

    public Deck deck;
    public TextAsset deckXML;

    public Layout layout;
    public TextAsset layoutXML;

    public Vector3 layoutCenter;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Transform layoutAnchor;  // Якорь для стола

    public CardProspector target;   // Ключевая карта, которая выдвигается перед нами
    public List<CardProspector> tableau;    // Игровой стол
    public List<CardProspector> discardPile;    // Бита

    public List<CardProspector> drawPile;

    void Awake() {
        S = this;    
    }

    void Start() {
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);
        // ref означает что исходный список тоже меняется

        layout = GetComponent<Layout>();    // Извлекаем скрипт
        layout.ReadLayout(layoutXML.text);

        drawPile = ConvertListCardsToListCardProspectors(deck.cards);
        LayoutGame();
    }

    // Draw() функция будет выбирать единственную карту из колоды и возвращать её
    CardProspector Draw() {
        CardProspector cd = drawPile[0];    // Выбираем первую карту
        drawPile.RemoveAt(0);
        return (cd);
    }

    CardProspector FindCardByLayoutID(int layoutID) {
        foreach (CardProspector cp in tableau) {
            if(cp.layoutID == layoutID) {
                // Если у карты такой же ID возвращаем её
                return (cp);
            }
        }
        // Если ничего не найденно, возвращаем нуль
        return (null);
    }

    // LayoutGame() расставляет начальный стол из карт, типо "пещера"
    void LayoutGame() {
        // Создаём пустой ио, чтобы быть якорём для всего стола (бедный ио :[ )
        if(layoutAnchor == null) {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter; // Ставим его правильно
        }
        // Есть якорь для всех карт
        CardProspector cp;
        // Следуем расскладке
        foreach (SlotDef tSD in layout.slotDefs) {
            // Повторяем для каждого слота в раскладке
            cp = Draw();    // Выбираем карту из колоды
            cp.faceUp = tSD.faceUp; // Задаём лицевую часть
            cp.transform.parent = layoutAnchor; // Заменит предыдущего родителя
            cp.transform.localPosition = new Vector3(
                layout.multiplier.x * tSD.x,
                layout.multiplier.y * tSD.y,
                -tSD.layerID);
            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            cp.state = CardState.tableau;

            cp.SetSortingLayerName(tSD.layerName);

            tableau.Add(cp);    // Добавляем карту на стол
        }

        // Находим какие карты скрывают другие
        foreach (CardProspector tCP in tableau) {
            foreach (int hid in tCP.slotDef.hiddenBy) {
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }

        // Задаём первую цель
        MoveToTarget(Draw());

        // Располагаем колоду
        UpdateDrawPile();
    }

    List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD) {
        List<CardProspector> lCP = new List<CardProspector>();
        CardProspector tCP;
        foreach (Card tCD in lCD) {
            tCP = tCD as CardProspector;
            lCP.Add(tCP);
        }
        return (lCP);
    }

    public void CardClicked(CardProspector cd) {
        // Ответ обозначается в зависимости от состояния кликнутой карты
        switch (cd.state) {
            case CardState.drawpile:
                // Берём карту из колоды
                MoveToDiscard(target);  // Убираем текущую активную карту в биту
                MoveToTarget(Draw());   // Берём карту из колоды
                UpdateDrawPile();   // Заново распологаем колоду
                break;
            case CardState.tableau:
                // Кликание на карту на столе будет проверять готовность
                bool validMatch = true;
                if (!cd.faceUp) {
                    validMatch = false;
                }
                if(!AdjacentRank(cd, target)) {
                    // Если это не смежный ранг не подходит
                    validMatch = false;
                }
                if (!validMatch) return;    // Возвращаем если не подходит
                // Дальше подходит
                tableau.Remove(cd); // Удаляем со списка стола
                MoveToTarget(cd);   // Делаем целью
                SetTableauFaces();  // Обновляем стол
                break;
            case CardState.target:
                break;
            case CardState.discard:
                break;
            default:
                break;
        }
        // Проверяем закончилась ли игра
        CheckForGameOver();
    }

    // Убираем текущий таргет в биту
    void MoveToDiscard(CardProspector cd) {
        // Задаём статус карты
        cd.state = CardState.discard;
        discardPile.Add(cd);    // Добавляем карту в список биты
        cd.transform.parent = layoutAnchor; // Обновляем родителя
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID + 0.5f );
        cd.faceUp = true;
        // Распологаем на вершине биты
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }

    // Задаём новый таргет
    void MoveToTarget(CardProspector cd) {
        // Если уже есть таргет, убираем последний в биту
        if (target != null) MoveToDiscard(target);  
        target = cd;    // Новый таргет
        cd.state = CardState.target;
        cd.transform.parent = layoutAnchor;
        // Перемещаем в позицию таргета
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID );
        cd.faceUp = true;   // Видим таргет
        // Задаём глубину прорисовки
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }

    // Распологает все карты в колоде, чтобы показать как много осталось
    void UpdateDrawPile() {
        CardProspector cd;
        // Повторяем для всех карт в колоде
        for (int i = 0; i < drawPile.Count; i++) {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;
            // Распологаем его правильно
            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(
                layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
                layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
                -layout.drawPile.layerID + 0.1f * i);
            cd.faceUp = false;  // Делаем их закрытыми
            cd.state = CardState.drawpile;
            // Задаём правильную отрисовку
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    public bool AdjacentRank(CardProspector c0, CardProspector c1) {
        // Если хотя бы одна из карт перевёрнута, не смежная
        if(!c0.faceUp || !c1.faceUp) { return (false); }

        // Если они на 1 единице разницы, они смежные
        if(Mathf.Abs(c0.rank - c1.rank) == 1) { return (true); }
        
        // Если один из них туз, другой король => они смежные
        if(c0.rank == 1 && c1.rank == 13) { return (true); }
        if(c1.rank == 1 && c0.rank == 13) { return (true); }

        // Во всех других случаях возвращаем ложь
        return (false);
    }

    void SetTableauFaces() {
        foreach (CardProspector cp in tableau) {
            bool fup = true;    // Предположим карта лицом вверх
            foreach (CardProspector cover in cp.hiddenBy) {
                // Если хотя одна из карт на столу, тогда карта должна быть перевёрнута
                if(cover.state == CardState.tableau) { fup = false; }
            }
            cp.faceUp = fup;
        }
    }

    // Проверяем закончилась ли игра
    void CheckForGameOver() {
        // Если стол пуст, игра закончена
        if(tableau.Count == 0) {
            // Вызываем конец игры с победой
            GameOver(true);
            return;
        }
        // Если всё ещё есть карты в колоде, игра не законченна
        if(drawPile.Count > 0) {
            return;
        }
        // Проверяем доступные ходы
        foreach (CardProspector cp in tableau) {
            if(AdjacentRank(cp, target)) {
                // Если всё ещё есть подходящий ход, игра не законченна
                return;
            }
        }
        // Так как нет подходящих ходов, игра законченна
        // Вызываем GameOver с порожением
        GameOver(false);
    }

    // Вызывается когда игра законченна
    void GameOver(bool won) {
        if (won) {
            print("Game Over. You won! :)");
        } else {
            print("Game Over. You Lost. :(");
        }
        // Перезагружаем сцену
        Application.LoadLevel("__Prospector_Scene_0");
    }
}
