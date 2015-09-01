using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfGuWuBuff : NfBuff
{
    int num;
    protected override bool Init()
    {
        PlaySelfEffect();
        num = Num;

        ChangeAttri(owner);
        return true;
    }

    protected override void OnEnd()
    {
        RestoreChangeAttri(owner);
        base.OnEnd();
    }

    public override void HandleSkillEnd(NfSkill skill)
    {
        if (--num <= 0)
        {
            OnEnd();
        }
    }
}
