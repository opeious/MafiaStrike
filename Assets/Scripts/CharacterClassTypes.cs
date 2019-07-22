using UnityEngine;

public enum CharacterClassTypes {
    STREET,
    CORPORATE,
    MERCENARY
}

public class GameboardUnitData
{
    public int maxHealth = 100;

    public int UnitSpeed;

    public float UnitVelocity;

    public int teamId;

    public int debugId;

    public int damage = 30;

    public CharacterClassTypes charType;

    public CharacterClassTypes strongTo;
    public CharacterClassTypes weakTo;

    public Sprite icon;
}
