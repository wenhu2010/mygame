using UnityEngine;
using System.Collections;

public class NfStunBuff : NfBuff
{
    protected override bool Init()
    {
        return true;
    }

    public override int HandleHitPre(Character target, int damage)
    {
        float per = Percent;
        if (Fight.Rand(0, 101) <= per)
        {
            AddTbuff(target);
        }
        return damage;
    }
}
