using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfIgniteBuff : NfBuff
{
    int count = 0;

    protected override bool Init()
    {
        count = Num;

        PlaySelfEffect();

        return true;
    }

    protected override void OnEnd()
    {
        base.OnEnd();
    }

    public override void HandleWaveAttack()
    {
        int dam = (int)(TotalValue);
		owner.BeRevHit (null, dam, HitEffect);

        if (--count <= 0)
        {
            _enabled = false;
            AddEvent(0.5f, delegate
            {
                OnEnd();
            });
        }
    }
}

public class NfIgnite2Buff : NfBuff
{
    int count = 0;

    protected override bool Init()
    {
        count = Num;

        PlaySelfEffect();

        return true;
    }

    protected override void OnEnd()
    {
        base.OnEnd();
    }

    public override void HandleWaveAttack()
    {
        int dam = (int)(Percent * owner.MaxHp);
        owner.BeRevHit(null, dam, HitEffect);

        if (--count <= 0)
        {
            _enabled = false;
            AddEvent(0.5f, delegate
            {
                OnEnd();
            });
        }
    }
}
