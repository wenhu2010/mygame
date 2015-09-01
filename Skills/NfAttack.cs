using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfAttack : NfSkill {

    protected Character targetChar;

    protected Vector3 destPos;
    protected bool isLookAtTarget = false;
    protected bool isTurnBack = false;
    protected string runAnim = "run";
    protected float moveSpeed;

    protected override void OnSkillBegin()
    {
        isLookAtTarget = false;
        isTurnBack = false;
		targetChar = GetFirstAttackTarget();
        moveSpeed = attacker.moveSpeed;
		
		MoveTo(targetChar);
	}

    protected override void OnSkillEnd()
    {
        DestroyAll();
        turnBack();
    }

    protected override bool CanFanJi()
    {
        return true;
    }

    protected override void turnTarget()
    {
        if (isLookAtTarget && targetChar != null)
        {
			List<Character> targets = Fight.Inst.FindAllFriend(targetChar);
            foreach (var t in targets)
            {
                if (t.IsDizzy)
                    continue;
                if (t.isXuLi)
                    continue;
                if (t.isMonster)
                {
                    if (t.monsterType == Monster.MonsterType.Boss)
                        continue;
                }

                t.LookAt(attacker);
            }
        }
    }

    protected void turnBack()
    {
        if (isTurnBack)
            return;
		List<Character> targets = Fight.Inst.FindAllFriend(targetChar);
        foreach (var t in targets)
        {
            t.rotateBack();
        }
        isTurnBack = true;
    }

    protected virtual void MoveTo(Vector3 pos, float animSpeed, MoveCallback callback)
    {
        Move(attacker, pos, moveSpeed, runAnim, animSpeed, callback);
	}

    protected virtual void MoveTo(Character target)
    {
        isLookAtTarget = true;

        //向目标移动
        Vector3 dest = new Vector3(target.position.x, target.position.y, target.position.z);
        Vector3 dir;
        if (rangeType == RangeType.Single)
            dir = attacker.position - dest;
        else
            dir = target.direction;
        dir.Normalize();
        dest = dest + dir * CalcRadius(attacker, target);
        float animSpeed = (moveSpeed / attacker.moveSpeed) * attacker.moveAnimSpeed;
        MoveTo(dest, animSpeed, OnAttackBegin);
    }

    protected virtual void OnAttackBegin()
    {
        bool breakAtk = false;
        foreach (var buff in targetChar.Buffs)
        {
            if (buff.enabled && buff.HandleBeHitPre(attacker))
            {
                breakAtk = true;
                break;
            }
        }

        if (breakAtk == false)
        {
            StartAttack();
        }
        else
        {
            PlayFireAnimAndEffect();

            float animLen = attacker.GetAnimLength(FireAnim);

            AddEvent(animLen, delegate
            {
                DestroyAllEff();
                End();
            });
        }
    }

    protected virtual void StartAttack()
    {
        attacker.LookAt(targetChar.position);

        // 播放动画和特效        
        float animLen = attacker.GetAnimLength(FireAnim);

        // 攻击
        OnAttack();

        // 回到原来位置
        AddEvent(animLen, delegate
        {
            if (DoTskill() == null)
            {
                MoveBack();
            }
            else
            {
                End();
            }
        });
    }

    protected virtual void MoveBack()
    {
        //attacker.position = attacker.SrcPos;
        //attacker.transform.rotation = attacker.SrcRotation;
        //attacker.Idle();
        //End();

        isLookAtTarget = false;
        moveSpeed = 50f;
        float animSpeed = (moveSpeed / attacker.moveSpeed) * attacker.moveAnimSpeed;
        MoveTo(attacker.SrcPos, animSpeed, OnMoveBackFinish);

        turnBack();
    }

    protected virtual void OnAttack()
    {
        PlayFireAnimAndEffect();

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
    }

    protected virtual void OnMoveBackFinish()
    {
        attacker.rotateBack();
        attacker.Idle();
		End();
	}
}
