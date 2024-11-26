public class Card
{
    public Suit Suit { get; set; }
    public int Value { get; set; }

    public Card(Suit suit, int value)
    {
        Suit = suit;
        Value = value;
    }
}

public enum Suit
{
    Hearts,
    Diamonds,
    Clubs,
    Spades
}
