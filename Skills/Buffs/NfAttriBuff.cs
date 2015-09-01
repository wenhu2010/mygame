using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfAttriBuff : NfBuff
{
    protected bool dodge;
    protected float dodgeTime;
    protected int num = 0;
    protected int dodgeNum = 0;

    protected override bool Init()
    {
        if (Tskill < 100)
            dodgeNum = Tskill;
        num = Num;
        dodge = false;
        PlaySelfEffect();
        ChangeAttri(owner);
        return true;
    }

    protected override void OnEnd()
    {
        RestoreChangeAttri(owner);
        base.OnEnd();
    }

    public override bool HandleDodge(Character attacker)
    {
        if (owner.IsDizzy || owner.IsBianYang)
            return false;

        if (dodge)
        {
            owner.PlayAnim("dodge");
            return true;
        }

        if (owner.AVD > 0)
        {
            dodge = Fight.Rand(0, 1001) < owner.AVD;
            if (dodge || dodgeNum > 0)
            {
                float len = owner.PlayAnim("dodge");
                dodgeTime = Time.time + len * 0.75f;
                dodgeNum--;
                return true;
            }
        }

        return false;
    }

    public override void Update()
    {
        if (dodge && Time.time >= dodgeTime)
        {
            dodge = false;
        }
    }

    public override void HandleHuiHe()
    {
        if (--num < 0)
        {
            End();
        }
    }

    public override void HandleDead()
    {
        List<Character> targets = FindTargets();
        foreach (var t in targets)
        {
            AddTbuff(t);
        }
    }
}
