using UnityEngine;
using System.Collections;
using LitJson;
using System.Collections.Generic;

public class NfShield : NfSkill {

    protected override void OnSkillBegin()
    {
        StartCameraAnim();

        List<Character> targets = FindTargets(false);

		int buffId = Buff;
        int buffLv = lv;
        float hitTime = HitTime;

        AddEvent(hitTime, delegate
        {
            foreach (var target in targets)
            {
                target.AddBuff(attacker, buffId, buffLv);
            }
        });

        float animLen = attacker.GetAnimLength(FireAnim);
        PlayFireAnimAndEffect();

        // 结束技能
        End(animLen);
    }

    protected override void OnSkillEnd()
    {
    }
}
