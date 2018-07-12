using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CBState включает как положения для игры, так и to____ положения для движения
public enum CBState {
    drawpile,
    toHand,
    hand,
    toTarget,
    target,
    discard,
    to,
    idle
}

public class CardBartok : Card {
    // Статичные поля используются чтобы задавать одинаковые значения для всех карт
    static public float MOVE_DURATION = 0.5f;
    static public string MOVE_EASING = Easing.InOut;
    static public float CARD_HEIGHT = 3.5f;
    static public float CARD_WIDTH = 2f;

    public CBState state = CBState.drawpile;

    // Поля чтобы записывать инфу для перемещения и вращения
    public List<Vector3> bezierPts;
    public List<Quaternion> bezierRots;
    public float timeStart, timeDuration;

    public int eventualSortOrder;
    public string eventualSortLayer;

    // Когда карта закончила движение, она будет вызывать reportFinishTo.SendMessage()
    public GameObject reportFinishTo = null;
    public Player callbackPlayer = null;

    private void Awake() {
        callbackPlayer = null;  // Просто убедимся
    }

    // MoveTo говорит карте переместиться в новую позицию и поворот
    public void MoveTo(Vector3 tPos, Quaternion tRot) {
        // Бизеровые кривые будут иметь только две точки
        // Создаём список
        bezierPts = new List<Vector3>();
        bezierPts.Add(transform.position);  // Добавляем стартовую позицию
        bezierPts.Add(tPos);                // Добавляем конечную позицию
        // То же самое с поворотом
        bezierRots = new List<Quaternion>();
        bezierRots.Add(transform.rotation);
        bezierRots.Add(tRot);

        // Если время старта не заданно, двигаем сразу же
        if(timeStart == 0) {
            timeStart = Time.time;
        }
        // Продолжительность всегда одинаковая но может дополниться позже
        timeDuration = MOVE_DURATION;

        // Установка положения toHand or toTarget будет делать вызывающий метод
        state = CBState.to;
    }

    public void MoveTo(Vector3 tPos) {
        MoveTo(tPos, Quaternion.identity);
    }

    private void Update() {
        switch (state) {
            case CBState.toHand:
            case CBState.toTarget:
            case CBState.to:
                // Вычисляем u из текущего времени и длительности
                float u = (Time.time - timeStart) / timeDuration;

                // Используем смягчающий класс
                float uC = Easing.Ease(u, MOVE_EASING);

                if (u < 0) { // Мы ещё не должны начать двигаться
                    // Остаёмся в базовой позиции
                    transform.localPosition = bezierPts[0];
                    transform.rotation = bezierRots[0];
                    return;
                } else if (u >= 1) { // u >= 1, мы закончили движение
                    uC = 1; // Чтобы не перестараться
                    // Меням состояние
                    if (state == CBState.toHand) state = CBState.hand;
                    if (state == CBState.toTarget) state = CBState.target;
                    if (state == CBState.to) state = CBState.idle;
                    // Двигаем к финальной позиции
                    transform.localPosition = bezierPts[bezierPts.Count - 1];
                    transform.rotation = bezierRots[bezierRots.Count - 1];
                    // Задаём timeStart к 0, так он перезапишется в след. раз
                    timeStart = 0;

                    if (reportFinishTo != null) { // Если есть предмет для отчёта
                        reportFinishTo.SendMessage("CBCallback", this);
                        // Обнуляем чтоб не посылать несколько команд подряд
                        reportFinishTo = null;
                    } else if (callbackPlayer != null){    // Некому отправлять
                        callbackPlayer.CBCallback(this);
                        callbackPlayer = null;  // Чтобы не вызывать несколько раз
                    } else {
                        // Ничего не делаем
                    }
                } else { // 0<=u<1 Значит что мы движемся
                    Vector3 pos = Utils.Bezier(uC, bezierPts);
                    transform.localPosition = pos;
                    Quaternion rot = Utils.Bezier(uC, bezierRots);
                    transform.rotation = rot;

                    // Проверяем правильно ли поставилось сортировка
                    if (u>0.5f && spriteRenderers[0].sortingOrder != eventualSortOrder) {
                        SetSortOrder(eventualSortOrder);
                    }
                    if (u>0.75f && spriteRenderers[0].sortingLayerName != eventualSortLayer) {
                        SetSortingLayerName(eventualSortLayer);
                    }
                }
                break;
        }
    }

    public override void OnMouseUpAsButton() {
        // Вызываем CardClicked в основном скрипте
        Bartok.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }
}
