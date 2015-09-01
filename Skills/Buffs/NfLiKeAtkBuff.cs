using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfLiKeAtkBuff : NfBuff {

    protected override bool Init()
    {
        if (owner.IsDizzy)
            return false;

        if (owner == caster)
        {
			owner.target = Fight.Inst.FindAttackTarget(owner);
            DoTskill();
        }
        else
        {
            owner.PlayEffect(Effect, 3f);
            owner.target = Fight.Inst.FindAttackTarget(owner);
            owner.CastSkill();
        }
        return false;
    }
}

public class NfNullSkill : NfSkill
{
    protected override void OnSkillBegin()
    {
        End();
    }

    protected override void OnSkillEnd()
    {
    }
}

public class NfBiShaBuff : NfBuff
{
    Character GetTarget()
    {
        float percent = Percent / 100f;
        List<Character> enemys = Fight.Inst.FindAllAttackTarget(owner);
        foreach (var e in enemys)
        {
            if (CanAtk(percent, e))
            {
                return e;
            }
        }
        return null;
    }

    protected override bool Init()
    {
        return true;
    }

    public override void HandleEnemyHpChange(Character target)
    {
        if (owner.IsDizzy)
            return;

        float percent = Percent / 100f;

        if (isAdd == false)
        {
            if (CanAtk(percent, target))
            {
                ChangeAttri(owner);
                PlaySelfEffect();
                AddTbuff(owner);
                isAdd = true;
                PushTskill();
            }
        }
        else
        {
            if (GetTarget() == null)
            {
                EndAttri();
            }
        }
    }

    private void EndAttri()
    {
        RestoreChangeAttri(owner);
        StopSelfEffect();
        RemoveTbuff(owner);
        isAdd = false;
    }

    public override NfSkill HandleSkillBegin()
    {
        if (owner.ReqSkillId == Tskill)
        {
            if (GetTarget() == null)
            {
                EndAttri();
                return new NfNullSkill();
            }
        }
        return null;
    }

    public override void HandleSkillEnd(NfSkill skill)
    {
        if (skill.id == Tskill)
        {
            isAdd = false;

            float percent = Percent / 100f;
            List<Character> enemys = Fight.Inst.FindAllAttackTarget(owner);
            foreach (var e in enemys)
            {
                if (CanAtk(percent, e))
                {
                    DoTskill(owner, e);
                    return;
                }
            }

            owner.MoveBack();
        }
    }

    private static bool CanAtk(float percent, Character e)
    {
        return e.CanBeAtk && e.MaxHp * percent >= e.HP;
    }

    public override void Update()
    {
        if (isAdd == false)
        {
            float percent = Percent / 100f;
            List<Character> enemys = Fight.Inst.FindAllAttackTarget(owner);
            foreach (var e in enemys)
            {
                if (CanAtk(percent, e))
                {
                    ChangeAttri(owner);
                    PlaySelfEffect();
                    AddTbuff(owner);
                    isAdd = true;
                    PushTskill();
                }
            }
        }
    }

    public bool isAdd { get; set; }
}

public class NfChiSkill : NfSkill
{
    protected Character target;
    public EffectObject keepEff;

    protected override void OnSkillBegin()
    {
        keepEff = null;

        target = GetFirstAttackTarget();
        attacker.target = target;

        StartCameraAnim();
        StartCoroutine(Attack());
    }

    private IEnumerator Attack()
    {
        // 播放动画和特效
        string singAnim = SingAnim;
        if (singAnim != "null")
        {
            PlaySingAnimAndEffect();
            float singTime = attacker.GetAnimLength(singAnim);
            yield return new WaitForSeconds(singTime);
        }

        List<Character> targets = FindTargets(true);
        Vector3 dest = new Vector3(target.position.x, target.position.y, target.position.z);
        Vector3 dir;
        dir = target.direction;
        dir.Normalize();
        dest = dest + dir * CalcRadius(attacker, target);

        MoveTo(dest, target, targets);
    }

    private IEnumerator OnAttack(Vector3 dest, Character target, List<Character> targets)
    {
        attacker.position = dest;
        attacker.LookAt(target.position);

        // 第一下攻击
        float hitTime = HitTime;
        float animLen = attacker.GetAnimLength(FireAnim);
        PlayFireAnimAndEffect();

        bool breakAtk = false;
        foreach (var buff in target.Buffs)
        {
            if (buff.enabled && buff.HandleBeHitPre(attacker))
            {
                breakAtk = true;
                break;
            }
        }

        if (breakAtk)
        {
            yield return new WaitForSeconds(animLen);
            End();
        }
        else
        {
            // 目标伤害计算
            float dam = Damage;
            int n = 0;
            foreach (var t in targets)
            {
                float r = 1f;
                if (n < damageTranJson.Count)
                    r = (float)damageTranJson[n++];
                float val = dam * r;
                CalcDamage(attacker, t, val, hitTime);
            }

            AddEvent(hitTime, delegate
            {
                AddBuff(attacker);
            });

            AddEvent(animLen, delegate
            {
                End();
            });
        }
    }

    protected void MoveTo(Vector3 destPos, Character target, List<Character> targets)
    {
        keepEff = PlayEffect(attacker, KeepEffect, -1);

        Move(attacker, destPos, BulletSpeed, KeepAnim, attacker.moveAnimSpeed, delegate
        {
            DestroyEff(keepEff);
            StartCoroutine(OnAttack(destPos, target, targets));
        });
    }

    protected override void OnSkillEnd()
    {
        DestroyEff(keepEff);
        DestroyAll();
    }
}