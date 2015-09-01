using UnityEngine;
using System.Collections;
using LitJson;

public class NfIgniteShieldBuff : NfBuff
{
    int num;
    protected override bool Init()
    {
        PlaySelfEffect();
        num = Num;
        return true;
    }

    public override int HandleBeHit(Character attacker, int damage, int addAnger)
    {
        AddTbuff(attacker);

        if (--num < 1)
        {
            End();
        }

        return damage;
    }
}
