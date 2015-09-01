using UnityEngine;
using System.Collections;
using LitJson;
using System.Collections.Generic;

public class NfXiSheng : NfAttack
{
    EffectObject ribbon;

    protected override void OnSkillBegin()
    {
        isLookAtTarget = false;
        isTurnBack = false;
        targetChar = GetFirstAttackTarget();

        moveSpeed = BulletSpeed;
        runAnim = KeepAnim;

        PlaySingAnimAndEffect();
        float st = attacker.GetAnimLength(SingAnim);
        if (st > 0f)
        {
            StartCoroutine(Sing(st));
        }
        else
        {
            ribbon = PlayEffect(attacker, KeepEffect, -1);
            MoveTo(targetChar);
        }
    }

    IEnumerator Sing(float singTime)
    {
        yield return new WaitForSeconds(singTime);

        ribbon = PlayEffect(attacker, KeepEffect, -1);
        MoveTo(targetChar);
    }

    protected override void OnSkillEnd()
    {
        DestroyRibbon();

        base.OnSkillEnd();
    }

    protected override void OnAttack()
    {
        DestroyRibbon();
        base.OnAttack();

        moveSpeed = attacker.moveSpeed;
        runAnim = "run";
    }

    protected void DestroyRibbon()
    {
        if (ribbon != null)
        {
            DestroyEff(ribbon);
            ribbon = null;
        }
    }
}
