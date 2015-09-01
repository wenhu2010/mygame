using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

//public class NfComboBeginAttack : NfSkill
//{
//    protected Character target;
//    public EffectObject keepEff;

//    protected override void OnSkillBegin()
//    {
//        keepEff = null;

//        target = GetFirstAttackTarget();
//        attacker.target = target;

//        StartCameraAnim();
//        StartCoroutine(Attack());
//    }

//    private IEnumerator Attack()
//    {
//        // 播放动画和特效
//        string singAnim = SingAnim;
//        if (singAnim != "null")
//        {
//            PlaySingAnimAndEffect();
//            float singTime = attacker.GetAnimLength(singAnim);
//            yield return new WaitForSeconds(singTime);
//        }

//        List<Character> targets = FindTargets(true);
//        Vector3 dest = new Vector3(target.position.x, target.position.y, target.position.z);
//        Vector3 dir;
//        dir = target.direction;
//        dir.Normalize();
//        dest = dest + dir * CalcRadius(attacker, target);

//        MoveTo(dest, target, targets);
//    }

//    private IEnumerator OnAttack(Vector3 dest, Character target, List<Character> targets)
//    {
//        attacker.position = dest;
//        attacker.LookAt(target.position);

//        // 第一下攻击
//        float hitTime = HitTime;
//        float animLen = attacker.GetAnimLength(FireAnim);
//        PlayFireAnimAndEffect();

//        bool breakAtk = false;
//        foreach (var buff in target.Buffs)
//        {
//            if (buff.enabled && buff.HandleBeHitPre(attacker))
//            {
//                breakAtk = true;
//                break;
//            }
//        }

//        if (breakAtk)
//        {
//            yield return new WaitForSeconds(animLen);
//            End();
//        }
//        else
//        {
//            // 目标伤害计算
//            float dam = Damage;
//            int n = 0;
//            foreach (var t in targets)
//            {
//                float r = 1f;
//                if (n < damageTranJson.Count)
//                    r = (float)damageTranJson[n++];
//                float val = dam * r;
//                CalcDamage(attacker, t, val, hitTime);
//            }

//            AddEvent(hitTime, delegate
//            {
//                AddBuff(attacker);
//            });

//            AddEvent(animLen, delegate
//            {          
//                // 第二下攻击
//                NfSkill skill = DoTskill();
//                if (skill != null)
//                {
//                    End();
//                }
//                else
//                {
//                    MoveSrcPos(attacker);
//                }
//            });
//        }
//    }

//    protected void MoveTo(Vector3 destPos, Character target, List<Character> targets)
//    {
//        keepEff = PlayEffect(attacker, KeepEffect, -1);

//        Move(attacker, destPos, BulletSpeed, KeepAnim, attacker.moveAnimSpeed, delegate
//        {
//            DestroyEff(keepEff);
//            StartCoroutine(OnAttack(destPos, target, targets));
//        });
//    }

//    protected override void OnSkillEnd()
//    {
//        DestroyEff(keepEff);
//        DestroyAll();
//    }
//}

//public class NfComboAttack : NfSkill
//{
//    protected override void OnSkillBegin()
//    {
//        StartCameraAnim();
//        StartCoroutine(Attack());
//    }

//    private IEnumerator Attack()
//    {
//        string singAnim = SingAnim;
//        if (singAnim != "null")
//        {
//            PlaySingAnimAndEffect();
//            float singTime = attacker.GetAnimLength(singAnim);
//            yield return new WaitForSeconds(singTime);
//        }

//        PlayFireAnimAndEffect();

//        string bullet = BulletModel;
//        if (bullet != "null")
//        {
//            PlayEffect(attacker.target.gameObject, bullet, 3f);
//        }

//        // 目标伤害计算
//        float dam = Damage;
//        int n = 0;
//        List<Character> targets = FindTargets(true);
//        float hitTime = HitTime;
//        foreach (var t in targets)
//        {
//            float r = 1f;
//            if (n < damageTranJson.Count)
//                r = (float)damageTranJson[n++];
//            float val = dam * r;
//            CalcDamage(attacker, t, val, hitTime);
//        }

//        float animLen = attacker.GetAnimLength(FireAnim);
//        yield return new WaitForSeconds(animLen);

//        DoTskill();

//        End();
//    }

//    protected override void OnSkillEnd()
//    {
//        DestroyAll();
//    }
//}

//public class NfComboEndAttack : NfSkill
//{
//    protected override void OnSkillBegin()
//    {
//        StartCoroutine(Attack());
//    }

//    private IEnumerator Attack()
//    {
//        PlayFireAnimAndEffect();

//        float hitTime = HitTime;
//        yield return new WaitForSeconds(hitTime);

//        // 目标伤害计算
//        float dam = Damage;
//        int n = 0;
//        List<Character> targets = FindTargets(true);
//        foreach (var t in targets)
//        {
//            float r = 1f;
//            if (n < damageTranJson.Count)
//                r = (float)damageTranJson[n++];
//            float val = dam * r;
//            CalcDamage(attacker, t, val, 0f);
//        }

//        float animLen = attacker.GetAnimLength(FireAnim);
//        yield return new WaitForSeconds(animLen - hitTime);

//        MoveSrcPos(attacker);
//    }

//    protected override void OnSkillEnd()
//    {
//        DestroyAll();
//    }
//}

public class NfComboAOE : NfSkill
{
    protected override void OnSkillBegin()
    {
        StartCameraAnim();
        StartCoroutine(Attack());
    }

    private IEnumerator Attack()
    {
        string singAnim = SingAnim;
        if (singAnim != "null")
        {
            PlaySingAnimAndEffect();
            float singTime = attacker.GetAnimLength(singAnim);
            yield return new WaitForSeconds(singTime);
        }

        PlayFireAnimAndEffect();

        string bullet = BulletModel;
        if (bullet != "null")
        {
			AddEvent(HitTime, delegate {
				PlayEffect(attacker.target.gameObject, bullet, 3f);
			});
        }

        // 目标伤害计算
        List<Character> targets = FindTargets(true);
        if (targets.Count > 0)
        {
            CalcCircalDamage(attacker, targets, attacker.target.position, Damage, HitTime);
        }

        float animLen = attacker.GetAnimLength(FireAnim);
        yield return new WaitForSeconds(animLen);

        DoTskill();

        End();
    }

    protected override void OnSkillEnd()
    {
        DestroyAll();
    }
}
