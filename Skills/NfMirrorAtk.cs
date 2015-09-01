using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfMirrorAtk : NfSkill {

    protected float gravity = 45f;
    protected float speed = 15f;

    protected override void OnSkillBegin()
    {
        if (BulletModel != "null")
        {
            JsonData bulletJson = JsonMapper.ToObject(skilldata.BulletSpeed);
            if (bulletJson.IsArray)
            {
                gravity = (float)bulletJson[0];
                speed = (float)bulletJson[1];
            }
        }

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
        int slot = attacker.slot;

        CampType enemyCamp = attacker.camp == CampType.Friend ? CampType.Enemy : CampType.Friend;
        PlayGeZiEffect(slot, enemyCamp);

        var destPos = Fight.Inst.GetSlotPos(enemyCamp, slot);

        if (BulletModel != "null")
        {
            Fire(destPos, FireBone, BulletModel);
        }
        else
        {
            EffectObject fireEffObj = PlayEffect(KeepEffect, 3f);
            if (fireEffObj != null)
            {
                fireEffObj.obj.transform.position = destPos;
            }

            yield return new WaitForSeconds(HitTime);

            OnCollision(destPos);
            DoTskill();
            End(0.5f);
        }
    }

    protected void Fire(Vector3 destPos, string fireBone, string bulletModel)
    {
        Transform bullet = null;
        EffectObject effObj = PlayEffect(bulletModel, -1f);
        GameObject bulletObj = effObj.obj;
        bullet = bulletObj.transform;

        GameObject hand = attacker.FindBone(fireBone);//Helper.FindObject(gameObject, fireBone, false);
        bullet.position = hand.transform.position;//new Vector3(hand.transform.position.x, hand.transform.position.y, hand.transform.position.z);

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
                OnCollision(destPos);

                DestroyEff(effObj);

                DoTskill();
                End(0.5f);

                return true;
            }

            return false;
        });
    }

    private void OnCollision(Vector3 destPos)
    {
        EffectObject fireEffObj = PlayEffect(FireEffect, 3f);
        if (fireEffObj != null)
        {
            fireEffObj.obj.transform.position = destPos;
        }

        // 目标伤害计算
        List<Character> targets = FindTargets(true);
        if (targets.Count > 0)
        {
            CalcCircalDamage(attacker, targets, destPos, Damage, 0f);
        }
        else
        {
            attacker.PlaySound(HitSound);
        }
    }
}
