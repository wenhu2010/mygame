using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfAOE : NfSkill 
{
    Character target;

    protected override void OnSkillBegin()
    {
        StartCameraAnim();
        StartCoroutine(Fire());
    }

    protected override void OnSkillEnd()
    {
        DestroyAll();
        //DestroyAllEff();
        //attacker.Idle();
    }

    IEnumerator Fire()
    {
        string singAnim = SingAnim;
        if (singAnim != "null")
        {
            float animLen = attacker.PlayAnim(SingAnim);
            PlayEffect(gameObject, SingEffect, animLen);
			if (SingTime > 0f) animLen = SingTime;
            yield return new WaitForSeconds(animLen);
        }

        int atkc = AtkC;
        float hitTime = HitTime;
        float keepTime = KeepTime;
        attacker.PlayAnim(FireAnim);
        Character target = attacker.target;
        PlayEffect(gameObject, KeepEffect, (keepTime + hitTime) * atkc);
        string fireEffect = FireEffect;

        List<Character> targets = FindTargets(true);
        float dam = Damage;
        for (int i = 0; i < atkc; ++i)
        {
            PlayEffect(target.gameObject, fireEffect, 3f);

            yield return new WaitForSeconds(hitTime);
            foreach (var t in targets)
            {
                CalcDamage(attacker, t, dam, 0f);
            }

            yield return new WaitForSeconds(keepTime);
        }

        End();
    }
}
