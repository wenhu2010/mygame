using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfRevAttri : NfSkill {

    protected override void OnSkillBegin()
    {
        PlayFireAnimAndEffect();

        AddEvent(HitTime, delegate
        {
            attacker.SkillHit();

            List<Character> targets = FindTargets(false);
            foreach (var t in targets)
            {
                RevAttri(t);
            }
        });

        End(attacker.GetAnimLength(FireAnim));
    }

    protected override void OnSkillEnd()
    {
        DestroyAll();
    }

    void RevAttri(Character c)
    {
        c.ATTRIBUTE = (c.ATTRIBUTE + 2) % 4;

        PlayEffect(c.gameObject, HitEffect, 3f);
    }
}
