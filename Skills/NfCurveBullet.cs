using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfCurveBullet : NfSkill
{
    protected float keepTime = 0f;
    protected int bulletNum = 0;
    protected float gravity = 45f;
    protected float speed = 15f;
    protected string fireBone;

    protected override void OnSkillBegin()
    {
        keepTime = 0f;
        bulletNum = 0;

		JsonData bulletJson = JsonMapper.ToObject(skilldata.BulletSpeed);
        if (bulletJson.IsArray)
        {
            gravity = (float)bulletJson[0];
            speed = (float)bulletJson[1];
        }

        fireBone = FireBone;

        StartCameraAnim();
        StartCoroutine(FireCurveBullet());
    }

    protected override void OnSkillEnd()
    {
        ShowMesh(true);
        DestroyAll();
    }

    protected virtual IEnumerator FireCurveBullet()
    {
        // 播放动画和特效
        PlaySingAnimAndEffect();

        yield return new WaitForSeconds(SingTime);

        ShowMesh(false);

        // 发射子弹
        string bulletModel = BulletModel;
        keepTime = KeepTime;
        if (keepTime > 0f && rangeType == RangeType.FrontRow)
        {
            Character target = GetFirstAttackTarget();
            if (target != null)
            {
                FireAOE(target, fireBone, bulletModel);
            }
            else
            {
                End();
            }
        }
        else
        {
            List<Character> targets = FindTargets(true);
            if (targets.Count > 0)
            {
                if (targets.Count == 1)
                {
                    attacker.LookAt(targets[0]);
                }

                bulletNum = targets.Count;
                foreach (var t in targets)
                {
                    Fire(t, fireBone, bulletModel);
                }
            }
            else
            {
                End();
            }
        }
    }

    protected void Fire(Character target, string fireBone, string bulletModel)
    {
        Transform bullet = null;
        EffectObject effObj = PlayEffect(bulletModel, -1f);
        GameObject bulletObj = effObj.obj;
        bullet = bulletObj.transform;

        GameObject hand = attacker.FindBone(fireBone);//Helper.FindObject(gameObject, fireBone, false);
        bullet.position = new Vector3(hand.transform.position.x, hand.transform.position.y, hand.transform.position.z);

        // 子弹碰撞检测
        Vector3 destPos;
        destPos = new Vector3(target.position.x, target.position.y + 1f, target.position.z);

        Vector2 v2DestPos = new Vector2(destPos.x, destPos.z);
        Vector2 v2SrcPos = new Vector2(bullet.position.x, bullet.position.z);
        Vector2 dir = new Vector2(v2DestPos.x - v2SrcPos.x, v2DestPos.y - v2SrcPos.y);
        dir.Normalize();

        float dist = Vector2.Distance(v2DestPos, v2SrcPos);
        float total = dist / speed;
        float h = destPos.y - bullet.position.y;
        float v = (h - 0.5f * (-gravity) * total * total) / total;
        Vector3 vo = new Vector3(dir.x * speed, v, dir.y * speed);
        Vector3 a = new Vector3(0f, -gravity, 0f);
        Vector3 srcPos = new Vector3(bullet.position.x, bullet.position.y, bullet.position.z);

        float pictch = Mathf.Atan2(v, speed) * Mathf.Rad2Deg;
        bullet.rotation = Quaternion.FromToRotation(new Vector3(0f, 0f, 1f), new Vector3(dir.x, 0f, dir.y)) * Quaternion.Euler(-pictch, 0, 0);

        float time = 0;
        AddEvent(delegate
        {
            time += Time.deltaTime;
            Vector3 p = vo * time + 0.5f * a * time * time;
            bullet.position = srcPos + p;

            float vt = v + (-gravity) * time;
            pictch = Mathf.Atan2(vt, speed) * Mathf.Rad2Deg;
            bullet.rotation = Quaternion.FromToRotation(new Vector3(0f, 0f, 1f), new Vector3(dir.x, 0f, dir.y)) * Quaternion.Euler(-pictch, 0f, 0f);

            if (time >= total)
            {
                OnCollision(target);

                DestroyEff(effObj);

                if (--bulletNum <= 0)
                {
                    DoTskill();
                    End();
                }
                return true;
            }

            return false;
        });
    }

    protected virtual void OnCollision(Character target)
    {
        // AOE伤害
        if (damageTranJson.IsArray)
        {
            if (damageTranJson[0].IsArray)
            {
                EffectObject hitEffObj = PlayEffect(HitSelfEffect, 3f);
                if (hitEffObj != null)
                {
                    hitEffObj.obj.transform.position = target.position;
                }

                CalcCircalDamage(attacker, Fight.Inst.FindAllAttackTarget(attacker), target.position, Damage, 0f);
                return;
            }
        }
        // 目标伤害计算
        CalcDamage(attacker, target, Damage, 0f);
    }

    void FireAOE(Character target, string fireBone, string bulletModel)
    {
        Transform bullet = null;
        EffectObject effObj = PlayEffect(bulletModel, -1f);
        GameObject bulletObj = effObj.obj;
        bullet = bulletObj.transform;

        GameObject hand = attacker.FindBone(fireBone);//Helper.FindObject(gameObject, fireBone, false);
        bullet.position = new Vector3(hand.transform.position.x, hand.transform.position.y, hand.transform.position.z);

        // 子弹碰撞检测
        Vector3 destPos;
        int slot = target.slot;
        if (slot >= 9)
        {
            slot = 11;
        }
        else if (slot >= 4)
        {
            slot = 6;
        }
        else if (slot >= 1)
        {
            slot = 2;
        }

        destPos = Fight.Inst.GetSlotPos(target.camp, slot);

        Vector2 v2DestPos = new Vector2(destPos.x, destPos.z);
        Vector2 v2SrcPos = new Vector2(bullet.position.x, bullet.position.z);
        Vector2 dir = new Vector2(v2DestPos.x - v2SrcPos.x, v2DestPos.y - v2SrcPos.y);
        dir.Normalize();

        float dist = Vector2.Distance(v2DestPos, v2SrcPos);
        float total = dist / speed;
        float h = destPos.y - bullet.position.y;
        float v = (h - 0.5f * (-gravity) * total * total) / total;
        Vector3 vo = new Vector3(dir.x * speed, v, dir.y * speed);
        Vector3 a = new Vector3(0f, -gravity, 0f);
        Vector3 srcPos = new Vector3(bullet.position.x, bullet.position.y, bullet.position.z);

        float pictch = Mathf.Atan2(v, speed) * Mathf.Rad2Deg;
        bullet.rotation = Quaternion.FromToRotation(new Vector3(0f, 0f, 1f), new Vector3(dir.x, 0f, dir.y)) * Quaternion.Euler(-pictch, 0, 0);

        float time = 0;
        AddEvent(delegate
        {
            time += Time.deltaTime;
            Vector3 p = vo * time + 0.5f * a * time * time;
            bullet.position = srcPos + p;

            float vt = v + (-gravity) * time;
            pictch = Mathf.Atan2(vt, speed) * Mathf.Rad2Deg;
            bullet.rotation = Quaternion.FromToRotation(new Vector3(0f, 0f, 1f), new Vector3(dir.x, 0f, dir.y)) * Quaternion.Euler(-pictch, 0f, 0f);

            if (time >= total)
            {
                DestroyEff(effObj);

                EffectObject eff = PlayEffect(KeepEffect, keepTime);
                if (eff != null)
                {
                    eff.obj.transform.position = new Vector3(destPos.x, 0.01f, destPos.z);
                }

                float dam = Damage;
                List<Character> targets = FindTargets(true);
                float hitTime = HitTime;
                foreach (var t in targets)
                {
                    // 目标伤害计算
                    CalcDamage(attacker, t, dam, hitTime);
                } 
                
                AddEvent(hitTime, delegate
                {
                    DoTskill();
                });

                End(hitTime + 0.5f);
                return true;
            }

            return false;
        });
    }
}
