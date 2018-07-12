using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Enum содержит разные фазы игры
public enum TurnPhase {
    idle,
    pre,
    waiting,
    post,
    gameOver
}

public class Bartok : MonoBehaviour {
    static public Bartok S;
    // Поле статичное чтобы убедиться что только 1 текущий игрок
    static public Player CURRENT_PLAYER;

    public TextAsset deckXML;
    public TextAsset layoutXML;
    public Vector3 layoutCenter = Vector3.zero;

    // Градусы на которые отклоняются карты
    public float handFanDegrees = 10f;
    public int numStartingCards = 7;
    public float drawTimeStagger = 0.1f;
    public bool _________________;

    public Deck deck;
    public List<CardBartok> drawPile;
    public List<CardBartok> discardPile;

    public BartokLayout layout;
    public Transform layoutAnchor;

    public List<Player> players;
    public CardBartok targetCard;

    public TurnPhase phase = TurnPhase.idle;
    public GameObject turnLight;

    public GameObject GTGameOver;
    public GameObject GTRoundResult;

    private void Awake() {
        S = this;

        // Находим turnLight по имени
        turnLight = GameObject.Find("TurnLight");
        GTGameOver = GameObject.Find("GTGameOver");
        GTGameOver.SetActive(false);
        GTRoundResult = GameObject.Find("GTRoundResult");
        GTRoundResult.SetActive(false);
    }

    private void Start() {
        deck = GetComponent<Deck>();    // Извлекаем скрипт
        deck.InitDeck(deckXML.text);    // Передаём т
        Deck.Shuffle(ref deck.cards);

        layout = GetComponent<BartokLayout>();
        layout.ReadLayout(layoutXML.text);  // Передаём файл в функшн

        drawPile = UpgradeCardsList(deck.cards);
        LayoutGame();
    }

    // Обновляем чтобы были CardBartoks
    // Конечно они всё время были таковыми, но мы просто даём Unity знать
    List<CardBartok> UpgradeCardsList(List<Card> lCD) {
        List<CardBartok> lCB = new List<CardBartok>();
        foreach (Card tCD in lCD) {
            lCB.Add(tCD as CardBartok);
        }
        return (lCB);
    }

    // Распологаем карты в колоде правильно
    public void ArrangeDrawPile() {
        CardBartok tCB;

        for (int i = 0; i < drawPile.Count; i++) {
            tCB = drawPile[i];
            tCB.transform.parent = layoutAnchor;
            tCB.transform.localPosition = layout.drawPile.pos;
            // Поворот должен быть 0
            tCB.faceUp = false;
            tCB.SetSortingLayerName(layout.drawPile.layerName);
            tCB.SetSortOrder(-i * 4);
            tCB.state = CBState.drawpile;
        }
    }

    void LayoutGame() {
        if (layoutAnchor == null) {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;   // Извлекаем трансформ компонент
            layoutAnchor.transform.position = layoutCenter; // Распологаем его
        }

        // Ставим колоду
        ArrangeDrawPile();

        // Иницииализируем игроков
        Player pl;
        players = new List<Player>();
        foreach (SlotDef tSD in layout.slotDefs) {
            pl = new Player();
            pl.handSlotDef = tSD;
            pl.playerNum = players.Count;
            players.Add(pl);
        }
        players[0].type = PlayerType.human; // Делаем человеком первого игрока

        // Раздаём карты по игрокам
        CardBartok tCB;
        for (int i = 0; i < numStartingCards; i++) {
            for (int j = 0; j < players.Count; j++) {
                tCB = Draw();   // Ролим карту
                // Ставим задержку
                tCB.timeStart = Time.time + drawTimeStagger * (i * 4 + j);
                //  ^ задаём время перед вызовом MoveTo()

                // Добавляем карту в руку
                // %4 делаем ренж цифр 0-3
                players[(j + 1) % 4].AddCard(tCB);  // Можно просто j
            }
        }
        // Вызываем Bartok.DrawFirstTarget() Когда карты разданны
        Invoke("DrawFirstTarget", drawTimeStagger * (numStartingCards * players.Count + players.Count));
    }

    public void DrawFirstTarget() {
        CardBartok tCB = MoveToTarget(Draw());
        // Говорим CardBartok сделать обратный вызов когда он закончит
        tCB.reportFinishTo = this.gameObject;
    }

    // Этот вызов используется единажды, когда последняя карта закончила раздаваться
    public void CBCallback(CardBartok cb) {
        // Иногда нужно делать такие отчёты
        Utils.tr(Utils.RoundToPlaces(Time.time), "Bartok.CBCallback()", cb.name);

        StartGame();
    }

    public void StartGame() {
        // Выбираем челика слева чтобы начал игру (мы собсна игрок 0 индекс)
        PassTurn(1);
    }

    public void PassTurn(int num = -1) {
        // Если не было переданно числа, выбираем следующего игрока
        if (num == -1) {
            int ndx = players.IndexOf(CURRENT_PLAYER);
            num = (ndx + 1) % 4;
        }
        int lastPlayerNum = -1;
        if(CURRENT_PLAYER != null) {
            lastPlayerNum = CURRENT_PLAYER.playerNum;
            if (CheckGameOver()) {
                return;
            }
        }
        CURRENT_PLAYER = players[num];
        phase = TurnPhase.pre;

        CURRENT_PLAYER.TakeTurn();

        // Двигаем лампочку чтобы подсветить текущего игрока
        Vector3 lPos = CURRENT_PLAYER.handSlotDef.pos + Vector3.back * 5;
        lPos.z = 0.95f;
        turnLight.transform.position = lPos;

        // Сообщаем о том что ход переданн
        Utils.tr(Utils.RoundToPlaces(Time.time), "Bartok.PassTurn()", "Old: " + lastPlayerNum,
            "New: " + CURRENT_PLAYER.playerNum);
    }

    // ValidPlay Проверяет что выбранная карта может быть сыгрына
    public bool ValidPlay(CardBartok cb) {
        // Допустимый ход если ранг одинаковый
        if (cb.rank == targetCard.rank) return (true);

        // Допустимый ход если масть одинаковая
        if (cb.suit == targetCard.suit) return (true);

        // В других случаях возвращаем 
        return (false);
    }

    public CardBartok MoveToTarget(CardBartok tCB) {
        tCB.timeStart = 0;  // Оно потом обновится
        tCB.MoveTo(layout.discardPile.pos + Vector3.back);
        tCB.state = CBState.toTarget;
        tCB.faceUp = true;
        tCB.SetSortingLayerName("10");
        tCB.eventualSortLayer = layout.target.layerName;
        if(targetCard != null) {
            MoveToDiscard(targetCard);
        }

        targetCard = tCB;
        return (tCB);
    }

    public CardBartok MoveToDiscard(CardBartok tCB) {
        tCB.state = CBState.discard;
        discardPile.Add(tCB);

        // Для корректной отрисовочки
        tCB.SetSortingLayerName(layout.discardPile.layerName);
        tCB.SetSortOrder(discardPile.Count * 4);
        // Чтобы коллайдеры не перекрывались
        tCB.transform.localPosition = layout.discardPile.pos + Vector3.back / 2;

        return (tCB);
    }

    public CardBartok Draw() {
        CardBartok cd = drawPile[0];
        drawPile.RemoveAt(0);
        return (cd);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            players[0].AddCard(Draw());
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            players[1].AddCard(Draw());
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            players[2].AddCard(Draw());
        }
        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            players[3].AddCard(Draw());
        }
    }

    public void CardClicked(CardBartok tCB) {
        // Если это не ход игрока, ничего не делаем
        if (CURRENT_PLAYER.type != PlayerType.human) return;
        // Если игра ждёт завершение движения, не отвечаем
        if (phase == TurnPhase.waiting) return;

        // Действуем по разному в зависимости он того, где кликнутая карта
        switch (tCB.state) {
            case CBState.drawpile:
                // Берём карту из колоды
                CardBartok cb = CURRENT_PLAYER.AddCard(Draw());
                cb.callbackPlayer = CURRENT_PLAYER;
                Utils.tr(Utils.RoundToPlaces(Time.time), "Bartok.CardClicked()", "Draw", cb.name);
                phase = TurnPhase.waiting;
                break;
            case CBState.hand:
                // Проверяем доступность хода
                if (ValidPlay(tCB)) {
                    CURRENT_PLAYER.RemoveCard(tCB);
                    MoveToTarget(tCB);
                    tCB.callbackPlayer = CURRENT_PLAYER;
                    Utils.tr(Utils.RoundToPlaces(Time.time), "Batrok.CardClicked()", 
                        "Play", tCB.name, targetCard.name + " is Target");
                    phase = TurnPhase.waiting;
                } else {
                    // Просто игнорируем
                    Utils.tr(Utils.RoundToPlaces(Time.time), "Batrok.CardClicked()",
                        "Attempt to Play", tCB.name, targetCard.name + " is Target");
                }
                break;
        }
    }

    public bool CheckGameOver() {
        // Смотрим нужно ли перетасовать биту в колоду
        if(drawPile.Count == 0) {
            List<Card> cards = new List<Card>();
            foreach (CardBartok cb in discardPile) {
                cards.Add(cb);
                discardPile.Clear();
                Deck.Shuffle(ref cards);
                drawPile = UpgradeCardsList(cards);
                ArrangeDrawPile();
            }
        }

        // Проверяем выйграл ли игрок
        if (CURRENT_PLAYER.hand.Count == 0) {
            // Игрок победил
            if (CURRENT_PLAYER.type == PlayerType.human) {
                GTGameOver.GetComponent<Text>().text = "You won!";
                GTRoundResult.GetComponent<Text>().text = "";
            } else { // Победил ИИ
                GTGameOver.GetComponent<Text>().text = "Game Over";
                GTRoundResult.GetComponent<Text>().text = "Player " + CURRENT_PLAYER.playerNum + " won";
            }
            GTGameOver.SetActive(true);
            GTRoundResult.SetActive(true);
            phase = TurnPhase.gameOver;
            Invoke("RestartGame", 1);
            return (true);
        }
        return (false);
    }

    public void RestartGame() {
        CURRENT_PLAYER = null;
        Application.LoadLevel("__Bartok_Scene_0");
    }
}
