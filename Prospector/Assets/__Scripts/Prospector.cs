using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prospector : MonoBehaviour {
    static public Prospector S;

    public Deck deck;
    public TextAsset deckXML;

    void Awake() {
        S = this;    
    }

    void Start() {
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
    }

}
