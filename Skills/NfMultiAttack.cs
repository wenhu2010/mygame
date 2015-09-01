using UnityEngine;
using System.Collections;
using LitJson;

public class NfMultiAttack : NfAttack
{
    protected override void StartAttack()
    {
        StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        attacker.LookAt(targetChar);

        // 目标伤害计算
        float dam = Damage;
        int atkcount = AtkC;
        float hitTime = HitTime;
		float animLen = attacker.GetAnimLength(FireAnim);
        for (int i = 0; i < atkcount; ++i)
        {
            PlayFireAnimAndEffect();
            CalcDamage(attacker, targetChar, dam, hitTime);

            yield return new WaitForSeconds(animLen + 0.1f);

            if (attacker.IsDead)
                break;
            if (targetChar.IsDead)
                break;
        }

        if (attacker.IsDead == false)
        {
            MoveBack();
        }
    }
}
