using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prospector : MonoBehaviour {
    static public Prospector S;

    public Deck deck;
    public TextAsset deckXML;

    public Layout layout;
    public TextAsset layoutXML;

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
    }
}
