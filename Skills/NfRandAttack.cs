using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfRandAttack : NfSkill {

    protected override void OnSkillBegin()
    {
        StartCameraAnim();

        StartCoroutine(OnAttack());
    }

    protected override void OnSkillEnd()
    {
        DestroyAll();
    }

    IEnumerator OnAttack()
    {
        PlaySingAnimAndEffect();

        yield return new WaitForSeconds(SingTime);

        float dam = Damage;
        int atkc = AtkC;
        float hitTime = HitTime;

        PlayFireAnimAndEffect();

        int count = 0;
        while (count < atkc)
        {
            if (attacker.target.IsDead)
                attacker.target = Fight.Inst.FindAttackTarget(attacker);
            List<Character> targets = FindTargets(true);
            if (targets.Count == 0)
                break;

            yield return new WaitForSeconds(hitTime);

            Character target;
            if (count == 0)
            {
                target = GetFirstAttackTarget();
            }
            else
            {
                int idx = Fight.Rand(0, targets.Count);
                target = targets[idx];
            }
            CalcDamage(attacker, target, dam, 0f);

            ++count;
        }

        End(hitTime);
    }
}
