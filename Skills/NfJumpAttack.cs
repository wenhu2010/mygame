using UnityEngine;
using System.Collections;

public class NfJumpAttack : NfAttack
{
    EffectObject effObj;

    protected override void OnSkillBegin()
    {
        effObj = null;
        isLookAtTarget = false;
        isTurnBack = false;
        targetChar = GetFirstAttackTarget();

        MoveTo(targetChar);
	}

    protected override void OnSkillEnd()
    {
        base.OnSkillEnd();
        DestroyEff(effObj);
    }

    protected override void MoveTo(Character target)
    {
        isLookAtTarget = true;

        StartCoroutine(_MoveTo(target));
    }

    protected IEnumerator _MoveTo(Character target)
    {
        // 播放动画和特效
        float singTime = attacker.GetAnimLength(SingAnim);
        PlaySingAnimAndEffect();

        yield return new WaitForSeconds(singTime);

        effObj = PlayEffect(attacker, KeepEffect, -1);

        //向目标移动
        Vector3 dest = new Vector3(target.position.x, target.position.y, target.position.z);
        Vector3 dir;
        if (rangeType == RangeType.Single)
            dir = attacker.position - dest;
        else
            dir = target.direction;
        dir.Normalize();
        dest = dest + dir * CalcRadius(attacker, target);
        float d = Vector3.Distance(dest, attacker.position);

        runAnim = KeepAnim;
        float animLen = attacker.GetAnimLength(runAnim);
        moveSpeed = BulletSpeed;//d / animLen;
        float animSpeed = animLen / (d / moveSpeed);

        MoveTo(dest, animSpeed, OnAttackBegin);
    }

    protected override void OnAttackBegin()
    {
        DestroyEff(effObj);
        base.OnAttackBegin();
    }

    protected override void MoveBack()
    {
        moveSpeed = attacker.moveSpeed;
        runAnim = "run";
        base.MoveBack();
    }
}
