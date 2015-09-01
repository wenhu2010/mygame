using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfGou : NfSkill
{
    Character target;

    protected override void OnSkillBegin()
    {
        target = null;

        StartCameraAnim();
        StartCoroutine(Attack());
    }

    protected override void OnSkillEnd()
    {
        DestroyAll();

        if (target != null)
        {
            target.position = target.SrcPos;
            target.rotation = target.SrcRotation;
        }

        if (attacker.IsDead)
        {
            attacker.Dead(delegate
            {
                DestroyAllEff();
            });
        }
    }

    IEnumerator Attack()
    {
        // 播放动画和特效
        float animLen = attacker.GetAnimLength(SingAnim);
        PlaySingAnimAndEffect();

        yield return new WaitForSeconds(animLen);
        
        target = attacker.target;
        GameObject bone = attacker.FindBone(FireBone);//Helper.FindObject(gameObject, FireBone, false);
        GameObject destBone = target.FindBone("Bip01");//Helper.FindObject(target.gameObject, "Bip01", false);

        EffectObject effObj = PlayChainEff(BulletModel, -1f, bone.transform.position, bone.transform.position);
        GameObject begin = Helper.FindObject(effObj.obj, "Begin_01");
        begin.transform.parent = bone.transform;
        begin.transform.position = bone.transform.position;
        GameObject end = Helper.FindObject(effObj.obj, "End_01");

        string str = KeepAnim;
        JsonData jsd = JsonMapper.ToObject(str);

        attacker.PlayAnim((string)jsd[0]);

        float bulletSpeed = BulletSpeed;
        Move(end.transform, destBone.transform.position, bulletSpeed, delegate
        {
            float len = attacker.PlayAnim((string)jsd[1]);

            AddEvent(len, delegate
            {
                attacker.PlayAnim((string)jsd[2]);
            });

            Move(end.transform, begin.transform.position, bulletSpeed, delegate
            {
                DestroyEff(effObj);
            });

            GameObject bipBone = attacker.FindBone("Bip01");//Helper.FindObject(gameObject, "Bip01", false);
            Move(target, bipBone.transform.position, bulletSpeed, delegate
            {
                StartCoroutine(Explode(target));
            });
        });
    }

    IEnumerator Explode(Character target)
    {
        float animLen = attacker.PlayAnim(FireAnim);
        yield return new WaitForSeconds(animLen);

        PlayEffect(gameObject, FireEffect, 3f);
        attacker.visible = false;
        attacker.HP = 0;

        CalcDamage(attacker, target, Damage, 0f);

        Vector3 destPos = target.SrcPos;
        Vector3 dir = new Vector3(destPos.x - target.position.x, destPos.y - target.position.y, destPos.z - target.position.z);
        dir.Normalize();

        // 子弹碰撞检测
        float bulletSpeed = BulletSpeed;

        float dist = Vector3.Distance(target.position, destPos);
        float mttime = dist / bulletSpeed;
        float mtime = 0f;

        float xAng = 0;
        AddEvent(delegate
        {
            mtime += Time.deltaTime;
            if (mtime >= mttime)
            {
                target.position = new Vector3(destPos.x, destPos.y, destPos.z);
                End();
                return true;
            }
            else
            {
                float md = Time.deltaTime * bulletSpeed;
                target.position += dir * md;
                xAng += Time.deltaTime * 20000;
                target.rotation = Quaternion.Euler(new Vector3(0, xAng, 0));
            }

            return false;
        });
    }
}
