using UnityEngine;
using System.Collections;
using LitJson;

public class NfAddDamBuff : NfBuff
{
    protected override bool Init()
    {
        return true;
    }

    public override int HandleBeHit(Character attacker, int damage, int addAnger)
    {
        End();
        return AddDamage(attacker, damage);
    }
}
