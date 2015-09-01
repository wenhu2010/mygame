using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfFear : NfSkill
{
    Character target;
    EffectObject effKeepObj;
    Vector3 destPos;
    int destSlot;

    protected override void OnSkillBegin()
    {
        target = attacker.target;
        effKeepObj = null;
        destPos = target.position;
        destSlot = target.slot;

        StartCameraAnim();
        StartCoroutine(Attack());
    }

    protected override void OnSkillEnd()
    {
        if (target != null)
        {
            target.isFear = false;
            target.Idle();
            target.rotateBack();
            if (hit && target.IsDead == false)
            {
                target.position = destPos;
                target.SrcPos = destPos;
                target.slot = destSlot;
                Fight.Inst.SortAllChar();
            }
            DestroyEff(effKeepObj);
            target.DestroyEff(effKeepObj);
        }
        DestroyAll();
    }

    IEnumerator Attack()
    {
        target.isFear = true;

        PlayFireAnimAndEffect();
        float animLen = attacker.GetAnimLength(FireAnim);
        float hitTime = HitTime;
        yield return new WaitForSeconds(hitTime);

        float dam = Damage;
        List<Character> targets = FindTargets(true);
        foreach (var t in targets)
        {
            CalcDamage(attacker, t, dam, 0f);
        }

        if (target.IsDead == false)
            Fear(target);
        else
            End(animLen - hitTime);
    }

    void Fear(Character target)
    {
        effKeepObj = target.PlayEffect(KeepEffect, -1f);

        destPos = target.position;
        List<Character> allEnemy = Fight.Inst.GetAllCharacter(attacker.camp == CampType.Friend ? CampType.Enemy : CampType.Friend);

        for (int i = 0; i < GameMgr.GamePlayer.MAXOPENSLOTNUM; ++i)
        {
            bool empty = true;
            for (int n = allEnemy.Count - 1; n >= 0; --n)
            {
                Character e = allEnemy[n];
                if (e.IsDead == false && e.slot == i)
                {
                    empty = false;
                    break;
                }               
            }
            int slot = i;
            if (empty && Fight.Inst.HasSlotPos(target.camp, slot))
            {
                destPos = Fight.Inst.GetSlotPos(target.camp, slot);
                destSlot = i;
                break;
            }
        }

        StartCoroutine(FearMove(target, target.position, 4, 0f));
    }

    Vector3 RandPos(Vector3 pos, float r)
    {
        float x = Fight.RandF(pos.x - r, pos.x + r);
        float z = Fight.RandF(pos.z - r, pos.z + r);
        z = Mathf.Clamp(z, -4f, 3f);
        return new Vector3(x, pos.y, z);
    }

    IEnumerator FearMove(Character target, Vector3 fearPos, int num, float delay)
    {
        yield return new WaitForSeconds(delay);

        const float r = 3f;
        Vector3 pos;
        if (num > 1)
        {
            pos = RandPos(fearPos, r);
        }
        else
        {
            pos = destPos;
        }

        target.LookAt(pos);
        target.PlayAnim("run", target.moveAnimSpeed);

        Move(target, pos, target.moveSpeed, "run", target.moveAnimSpeed, delegate
        {
            --num;
            if (num < 1)
            {
                target.position = pos;
                End();
            }
            else
            {
                target.position = pos;
                StartCoroutine(FearMove(target, fearPos, num, 0.4f));
            }
        });
    }
}

