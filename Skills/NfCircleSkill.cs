using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfCircleSkill : NfSkill
{

    protected override void OnSkillBegin()
    {
        StartCameraAnim();

        StartCoroutine(Attack());
        AddBuff(attacker);
    }

    protected override void OnSkillEnd()
    {
        DestroyAll();
    }

    IEnumerator Attack()
    {
        PlaySingAnimAndEffect();

        yield return new WaitForSeconds(SingTime + HitTime);

        List<Character> targets = FindTargets(true);
        CalcCircalDamage(attacker, targets, attacker.position, Damage, 0f);

        yield return new WaitForSeconds(TotalTime);
        DoTskill();
        End();
    }
}
