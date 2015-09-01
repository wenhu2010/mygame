using UnityEngine;
using System.Collections;

public class NfXiXueBuff : NfBuff
{

    protected int num = 0;
    protected override bool Init()
    {
        num = Num;
        PlaySelfEffect();
        return true;
    }

    public override int HandleHitHp(Character target, int damage, int addAnger)
    {
        float per = Percent;

        int addHp = (int)(damage * per);
        owner.ShowHp();
        owner.AddHp(owner, addHp, HitEffect);
        return damage;
    }

    public override void HandleHuiHe()
    {
        if (--num < 0)
        {
            OnEnd();
        }
    }
}
