using UnityEngine;
using System.Collections;
using LitJson;

public class NfFanZhiBuff : NfBuff
{
    int num;
    protected override bool Init()
    {
        PlaySelfEffect();
        num = Num;
        return true;
    }

    public override bool HandleBeHitPre(Character attacker)
    {
        if (owner.IsDizzy || owner.IsBianYang || NfSkill.CanFanJi(attacker) == false)
            return false;
        if (owner.CurrSkill != null)
        {
            if (owner.CurrSkill is NfThrowBack == false)
                return false;
            if (owner.CurrSkill is NfJiTui == false)
                return false;
        }

        if (num <= 0)
        {
            float per = Percent;
            if (Fight.Rand(1, 101) > per)
            {
                return false;
            }
        }

        --num;
        DoTskill(owner, attacker);
        return true;
    }
}
