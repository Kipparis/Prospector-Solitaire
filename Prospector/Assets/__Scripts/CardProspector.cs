using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 4 возможных положения карты
public enum CardState {
    drawpile,
    tableau,
    target,
    discard
}

public class CardProspector : Card {

    public CardState state = CardState.drawpile;
    // hiddenBy список содержит какие карты удерживают эту карту закрытой
    public List<CardProspector> hiddenBy = new List<CardProspector>();
    // LayoutID такой же как у карты
    public int layoutID;
    // SlotDef содержит информацию о позиции
    public SlotDef slotDef;

    public override void OnMouseUpAsButton() {
        // Вызываем метод из проспектора
        Prospector.S.CardClicked(this);

        base.OnMouseUpAsButton();
    }
}
