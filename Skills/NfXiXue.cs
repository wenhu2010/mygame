using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfXiXue : NfSkill
{
    protected override void OnSkillBegin()
    {
        StartCameraAnim();

        StartCoroutine(Attack());
    }

    protected override void OnSkillEnd()
    {
        attacker.Idle();
        DestroyAll();
    }

    IEnumerator Attack()
    {
        float singTime = attacker.GetAnimLength(SingAnim);
        PlaySingAnimAndEffect();

        AddBuff(attacker);

        yield return new WaitForSeconds(singTime - 0.05f);

        string keepAnim = KeepAnim;
        string keepEff = KeepEffect;
        string hitSelfEffect = HitSelfEffect;
        string fireBone = FireBone;
        string bulletModel = BulletModel;
        float hitTime = HitTime;
        float keepAnimLen = attacker.PlayAnim(keepAnim);
        EffectObject effKeepObj = PlayEffect(gameObject, keepEff, keepAnimLen);

        float per = Damage / 100f;
        int atkc = AtkC;
        List<Character> targets = FindTargets(true);
        List<EffectObject> effs = new List<EffectObject>();
        foreach (var t in targets)
        {
            EffectObject effObj = PlayChainEff(attacker, t, bulletModel, fireBone, "Bip01");
            effs.Add(effObj);
        }
        for (int i = 0; i < atkc; ++i)
        {
            yield return new WaitForSeconds(hitTime);

            int num = 0;
            foreach (var t in targets)
            {
                if (t.IsDead == false)
                {
                    float val = t.MaxHp * per;
                    CalcDamage(attacker, t, val, 0f);
                    AddHp(attacker, attacker, (int)val, hitSelfEffect, 0f);
                    num += 1;
                }
            }
            if (num == 0)
                break;
        }

        yield return new WaitForSeconds(hitTime);

        foreach (var e in effs)
        {
            DestroyEff(e);
        }
        DestroyEff(effKeepObj);
        End();
    }
}
