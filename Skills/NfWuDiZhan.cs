using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfWuDiZhan : NfSkill {

    protected override void OnSkillBegin()
    {
        StartCameraAnim();

        StartCoroutine(Attack());
    }

    protected override void OnSkillEnd()
    {
        DestroyAll();
        DestroyAllEff();
    }

    IEnumerator Attack()
    {
        // 播放动画和特效
        float singTime = attacker.GetAnimLength(SingAnim);
        PlaySingAnimAndEffect();

        yield return new WaitForSeconds(singTime);

        Vector3 srcPos = new Vector3(attacker.position.x, attacker.position.y, attacker.position.z);

        string fireAnim = FireAnim;
        float hitTime = HitTime;
        float srcAnimSpeed = animation[fireAnim].speed;
        float fireAnimTime = attacker.GetAnimLength(fireAnim) / 2f;
        hitTime = hitTime / 2f;

        EffectObject ribbon = PlayEffect(gameObject, KeepEffect, -1f);
        if (ribbon != null)
        {
            ribbon.obj.transform.position = attacker.position;
            ribbon.obj.transform.rotation = attacker.rotation;
            ribbon.obj.transform.parent = transform;
        }

        float dam = Damage;
        int atkc = AtkC;
        int count = 0;
        while (count < atkc)
        {
            if (attacker.target.IsDead)
                attacker.target = Fight.Inst.FindAttackTarget(attacker);
            List<Character> targets = FindTargets(true);//fight.FindNearTargets(target);
            if (targets.Count == 0)
                break;

            Character target;
            if (count == 0)
            {
                target = GetFirstAttackTarget();
            }
            else
            {
                int idx = Fight.Rand(0, targets.Count);
                target = targets[idx];
            }

            Vector3[] aktPos = new Vector3[4];

            float radius = CalcRadius(attacker, target);
            Vector3 dest = new Vector3(target.position.x, target.position.y, target.position.z);
            Vector3 dir = new Vector3(1, 0, 0);
            aktPos[0] = dest + dir * radius;
            dir = new Vector3(0, 0, 1);
            aktPos[1] = dest + dir * radius;
            dir = new Vector3(0, 0, -1);
            aktPos[2] = dest + dir * radius;
            dir = new Vector3(-1, 0, 0);
            aktPos[3] = dest + dir * radius;

            attacker.position = aktPos[Fight.Rand(0, aktPos.Length)];
            attacker.LookAt(dest);

            // 播放动画和特效
            attacker.PlayAnim(FireAnim, 2f);
            PlayEffect(gameObject, FireEffect, 3f);

            // 目标伤害计算
            CalcDamage(attacker, target, dam, hitTime);

            yield return new WaitForSeconds(fireAnimTime + 0.1f);
            ++count;
        }

        if (ribbon != null)
            DestroyEff(ribbon);
        animation[fireAnim].speed = srcAnimSpeed;
        attacker.position = srcPos;
        attacker.rotation = attacker.SrcRotation;
        End();
    }
}
