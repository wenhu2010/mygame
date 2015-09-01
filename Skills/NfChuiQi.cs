using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfChuiQi : NfSkill
{
    protected override void OnSkillBegin()
    {
        StartCameraAnim();
        StartCoroutine(Attack());
    }

    protected override void OnSkillEnd()
    {
        DestroyAll();
    }

    IEnumerator Attack()
    {
        PlaySingAnimAndEffect();

        yield return new WaitForSeconds(HitTime);

        List<Character> allEnemys = Fight.Inst.FindAllAttackTarget(attacker);
        if (allEnemys.Count > 1)
        {
            attacker.SkillHit();

            List<Character> targets = FindTargets(true);
            foreach (var target in targets)
            {
				AddTbuff(target);

				yield return new WaitForEndOfFrame();
            }

            attacker.target = Fight.Inst.FindAttackTarget(attacker);
        }

        float animLen = attacker.GetAnimLength(SingAnim);
        if (animLen > HitTime)
            yield return new WaitForSeconds(animLen - HitTime);

        DoTskill();

        End();
    }
}
