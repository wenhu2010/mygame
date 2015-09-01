using UnityEngine;
using System.Collections;

public class NfChongFeng : NfAttack
{
    EffectObject ribbon;

    protected override void OnSkillBegin()
    {
        StartCameraAnim();

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

    protected override void MoveTo(Vector3 pos, float animSpeed, NfSkill.MoveCallback callback)
    {
        float dist = Vector3.Distance(pos, attacker.position);
        float totalTime = dist / moveSpeed;
        float animLen = attacker.GetAnimLength(runAnim);
        animSpeed = animLen / totalTime;
        Move(attacker, pos, moveSpeed, runAnim, animSpeed, callback);
    }

    IEnumerator Sing(float singTime)
    {
        yield return new WaitForSeconds(singTime);

        ribbon = attacker.PlayEffect(KeepEffect, -1f);
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
            attacker.DestroyEff(ribbon);
            ribbon = null;
        }
    }
}
