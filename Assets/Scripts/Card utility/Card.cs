using UnityEngine;

public class Card
{
    public int value;
    public GameObject cardObject;

    public Card(int _value)
    {
        value = _value;
    }

    public override string ToString() 
    {
        return value + "";   
    }
}
