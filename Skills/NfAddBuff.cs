using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfAddBuff : NfSkill
{
    protected override void OnSkillBegin()
    {
        StartCameraAnim();

        List<Character> targets = FindTargets(false);

        PlayFireAnimAndEffect();

        if (targets.Count == 1)
        {
            if (attacker != targets[0])
            {
                attacker.LookAt(targets[0].position);
            }
        }
        
        AddEvent(HitTime, delegate
        {
            attacker.SkillHit();

            foreach (var target in targets)
            {
                target.PlayEffect(HitEffect, 2f);
                AddBuff(target);
            }
        });

        // 结束技能
        float animLen = attacker.GetAnimLength(FireAnim);
        AddEvent(animLen, delegate
        {
            attacker.rotateBack();
            DoTskill();
            End();
        });
    }

    protected override void OnSkillEnd()
    {
        DestroyAll();
    }
}
