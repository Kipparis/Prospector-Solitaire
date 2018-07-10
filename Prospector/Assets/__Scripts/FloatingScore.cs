using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum FSState {
    idle,
    pre,
    active,
    post
}

// FloatingScore может двигаться по экрану сам, следуя Bezier кривой
public class FloatingScore : MonoBehaviour {
    public FSState state = FSState.idle;
    [SerializeField]
    private int _score = 0; // Счёт
    public string scoreString;

    // Свойство счёта которое также задаёт scoreString когда задан
    public int score {
        get { return (_score); }
        set {
            _score = value;
            scoreString = Utils.AddCommasToNumber(_score);
            GetComponent<Text>().text = scoreString;
        }
    }

    public List<Vector3> bezierPts; // Bezier points для движения
    public List<float> fontSizes;   // Бизеровые точки для увеличения шрифта
    public float timeStart = -1;
    public float timeDuration = 1;
    public string easingCurve = Easing.InOut;   // Используем смягчение в Utils

    // Игровой объект который будет получать SendMessage когда это закончит движение
    public GameObject reportFinishTo = null;

    // Задаём FloatingScore и движение
    // Заметим использование базового параметра для eTimeS и eTimeD
    public void Init(List<Vector3> ePts, float eTimeS = 0, float eTimeD = 1) {
        bezierPts = new List<Vector3>(ePts);

        if (ePts.Count == 1) { // Там только одна точка
            // просто идём туда
            transform.position = ePts[0];
            return;
        }

        // Если eTimeS им. базовое значение, начинаем сразу же
        if (eTimeS == 0) eTimeS = Time.time;
        timeStart = eTimeS;
        timeDuration = eTimeD;

        state = FSState.pre;    // Готов к началу движения
    }

    public void FSCallback(FloatingScore fs) {
        score += fs.score;
    }

    private void Update() {
        // Если он не двигается, просто возвращаем
        if (state == FSState.idle) return;

        // Вычисляем u из текущего времени и длинны
        float u = (Time.time - timeStart) / timeDuration;
        // Используем Easing класс из Utils чтобы изменить u
        float uC = Easing.Ease(u, easingCurve);
        if (u < 0) {  // Мы ещё не должны начать передвигаться
            state = FSState.pre;
            // Двигаем к стартовой точке
            transform.position = bezierPts[0];
        } else {
            if (u >= 1) { // Если у больше единицы значит мы закончили движение
                uC = 1; // Задаём уЦе = 1 чтобы мы не перестарались
                state = FSState.post;
                if (reportFinishTo != null) {   // Если есть ио для фидбека
                    // Используем SendMessage чтобы вызвать FSCallback метод
                    reportFinishTo.SendMessage("FSCallback", this);
                    // Когда сообщение отправлено уничтожаем ио
                    Destroy(gameObject);
                } else {
                    // Если некому вернуть
                    // Просто оставляем в покое
                    state = FSState.idle;
                }
            } else {    // 0<=u<1, что означает что счёт всё ещё движется
                state = FSState.active;
            }
            // Используем бизеровую кривую чтобы двигаться к правильно точке
            Vector3 pos = Utils.Bezier(uC, bezierPts);
            transform.position = pos;
            if (fontSizes != null && fontSizes.Count > 0) {
                // Если есть различные размеры шрифта, то приравниваем так же
                int size = Mathf.RoundToInt(Utils.Bezier(uC, fontSizes));
                GetComponent<Text>().fontSize = size;
            }
        }
    }
}
