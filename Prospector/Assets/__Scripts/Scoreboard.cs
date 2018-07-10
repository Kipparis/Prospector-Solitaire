using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Отвечает за показ счёта игроку
public class Scoreboard : MonoBehaviour {
    public static Scoreboard S;

    public GameObject prefabFloatingScore;

    public bool ________________;
    public GameObject canvas;
    [SerializeField]
    private int _score = 0;
    public string _scoreString;

    // score свойство также задаёт scoreString
    public int score {
        get { return (_score); }
        set {
            _score = value;
            _scoreString = Utils.AddCommasToNumber(_score);
        }
    }

    // Свойство scoreString также задаёт Text.text;
    public string scoreString {
        get { return (_scoreString); }
        set {
            _scoreString = value;
            GetComponent<Text>().text = _scoreString;
        }
    }

    private void Awake() {
        S = this;
    }

    private void Start() {
        canvas = GameObject.Find("Canvas");
    }

    // Когда вызывается с SendMessage, оно добавляет fs.score к этому счёту
    public void FSCallback(FloatingScore fs) {
        score += fs.score;
    }

    // Одно будет создавать новый плавающий счёт и определять его
    public FloatingScore CreateFloatingScore(int amt, List<Vector3> pts) {
        GameObject go = Instantiate(prefabFloatingScore) as GameObject;
        //go.transform.parent = transform.Find("Canvas");
        go.transform.SetParent(canvas.transform, false);
        FloatingScore fs = go.GetComponent<FloatingScore>();
        fs.score = amt;
        fs.reportFinishTo = this.gameObject;    // Сообщение этому игровому объекту
        fs.Init(pts);
        return (fs);
    }
}
