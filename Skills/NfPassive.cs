using UnityEngine;
using System.Collections;
using LitJson;

public class NfPassive : NfSkill
{
    public bool start = false;

    protected override void OnSkillBegin()
    {
        start = true;
        AddBuff(attacker, true);

        if (skilldata.Tskill != -1)
			attacker.PushStartSkill(skilldata.Tskill, lv);

        End();
    }

    protected override void OnSkillEnd()
    {

    }
}
