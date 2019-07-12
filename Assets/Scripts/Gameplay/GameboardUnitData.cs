using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum characterClassTypes
{
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

    public characterClassTypes charType;

    public Sprite icon;
}
