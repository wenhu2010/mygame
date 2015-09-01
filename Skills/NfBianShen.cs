using UnityEngine;
using System.Collections;

public class NfBianShen : NfSkill
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

    private void AddBuff()
    {
        attacker.SkillHit();

        if (attacker.HasBuff(Buff) == false)
        {
            AddBuff(attacker);
        }
    }

    IEnumerator Attack()
    {
        // 播放动画和特效
        PlaySingAnimAndEffect();

        yield return new WaitForSeconds(SingTime);

        AddBuff();

        //float animLen = attacker.GetAnimLength(SingAnim);
        //yield return new WaitForSeconds(animLen - SingTime);

        yield return new WaitForFixedUpdate();

        DoTskill();
        End();
    }
}
