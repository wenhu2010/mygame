using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfZuanDi : NfSkill {

    protected override void OnSkillBegin()
    {
        StartCoroutine(StartSkill());
    }

    protected override void OnSkillEnd()
    {
        DestroyAll();
    }

    IEnumerator StartSkill()
    {
        Character target = GetFirstAttackTarget();

		float downtime = KeepTime;
		string fireAnim = FireAnim;

        // 钻下去
        float downLen = attacker.PlayAnim("down");

        yield return new WaitForSeconds(downLen);
        attacker.PlayAnim("downidle");

        // 钻上来
        yield return new WaitForSeconds(downtime);
        Vector3 dest = new Vector3(target.position.x, target.position.y, target.position.z);
        Vector3 dir = target.direction;//attacker.position - dest;
        dir.z = 0f;
        dir.Normalize();
        dest = dest + dir * CalcRadius(attacker, target);
        attacker.position = dest;
        attacker.LookAt(target);
        float upLen = attacker.PlayAnim("up");

        // 攻击
        yield return new WaitForSeconds(upLen);
        // 播放攻击动画
        float fireLen = attacker.PlayAnim(fireAnim);
        PlayEffect(gameObject, FireEffect, fireLen);
        // 目标伤害计算
        float hitTime = HitTime;
        float dam = Damage;
        List<Character> targets = FindTargets(true);
        int n = 0;
        foreach (var t in targets)
		{
			float r = 1f;
            if (n < damageTranJson.Count)
                r = (float)damageTranJson[n++];
			float val = dam * r;
            CalcDamage(attacker, t, val, hitTime);
        }
        // 回来
        yield return new WaitForSeconds(fireLen);
        attacker.PlayAnim("down");

        yield return new WaitForSeconds(downLen);
        attacker.position = attacker.SrcPos;
        attacker.PlayAnim("up");

        yield return new WaitForSeconds(upLen);
        attacker.Idle();
        End();
    }
}
