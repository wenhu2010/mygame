using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfDeathCoil : NfSkill {

    protected override void OnSkillBegin()
    {
        List<Character> targets = Fight.Inst.FindAllAttackTarget(attacker);
        List<Character> friends = Fight.Inst.FindAllFriend(attacker);
        targets.AddRange(friends);

        Character target = GetFirstAttackTarget();
        float hpPer = 1f;
        foreach (var t in targets)
        {
            float per = (float)t.HP / (float)t.MaxHp;
            if (per < hpPer)
            {
                target = t;
                hpPer = per;
            }
        }

        attacker.LookAt(target);

        if (target.camp == attacker.camp)
        {
            StartCameraAnim();
            PlaySingAnimAndEffect();

            AddEvent(HitTime, delegate
            {
                AddHp(attacker, target, (int)Damage, HitEffect, 0f);
                AddTbuff(target);
            });
            End(attacker.GetAnimLength(SingAnim));
        }
        else
        {
            DoTskill();
            End();
        }
    }

    protected override void OnSkillEnd()
    {
        DestroyAll();
    }
}
