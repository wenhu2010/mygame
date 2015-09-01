using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfMoveChain : NfAttack
{
    protected override void OnSkillEnd()
    {
        base.OnSkillEnd();

        DestroyAllEff();
    }

    protected override void OnAttack()
    {
        StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        // 播放动画和特效
        float animLen = attacker.GetAnimLength(FireAnim);
        PlayFireAnimAndEffect();

        // 目标伤害计算
        float hitTime = HitTime;
        string bulletModel = BulletModel;
        string fireBone = FireBone;
        GameObject hand = attacker.FindBone(fireBone);//Helper.FindObject(gameObject, fireBone, false);

        List<Character> targets = FindTargets(true);

        yield return new WaitForSeconds(hitTime);

        float dam = Damage;
        foreach (Character c in targets)
		{
			Vector3 beginPos = hand.transform.position;
			Vector3 endPos = new Vector3(c.position.x, 0.6f, c.position.z);
			PlayChainEff(bulletModel, animLen - hitTime, beginPos, endPos);

            CalcDamage(attacker, c, dam, 0.0001f);
        }
    }
}
