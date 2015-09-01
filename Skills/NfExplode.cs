using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfExplode : NfAttack
{
    protected override void OnSkillEnd()
    {
        base.OnSkillEnd();

        iTween.ScaleTo(gameObject, attacker.SrcScale, 0.3f);
        if (attacker.IsDead)
        {
            attacker.Dead(delegate
            {
                DestroyAllEff();
            });
        }
    }

    protected override void StartAttack()
    {
        attacker.LookAt(targetChar);

        PlaySingAnimAndEffect();

        float hitTime = HitTime;
        float scale = gameObject.transform.localScale.y * 1.5f;
        iTween.ScaleTo(gameObject, new Vector3(scale, scale, scale), hitTime);

        StartCoroutine(Explode(hitTime));
    }

    IEnumerator Explode(float hitTime)
    {
        yield return new WaitForSeconds(hitTime);

        PlayEffect(gameObject, FireEffect, 3f);
        attacker.visible = false;
        attacker.HP = 0;

        float dam = Damage;
        List<Character> targets = FindTargets(true);
        foreach (var t in targets)
        {
            CalcDamage(attacker, t, dam, 0f);
        }

        yield return new WaitForSeconds(targetChar.GetAnimLength("hit"));
        End();
    }
}
