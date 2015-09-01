using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfBullet : NfSkill
{
    protected int bulletNum = 0;
    protected string fireBone;

    protected override void OnSkillBegin()
    {
        StartCameraAnim();

        bulletNum = 0;

        hideMesh.Clear();
        fireBone = FireBone;

        StartCoroutine(FireBullet());
    }

    protected override void OnSkillEnd()
    {
        ShowMesh(true);
        DestroyAll();
    }

    protected void Fire(Character target, string fireBone, string bulletModel)
    {
        Transform bullet = null;
        GameObject hand = attacker.FindBone(fireBone);
        EffectObject effObj = PlayEffect(bulletModel, -1f);
        GameObject bulletObj = effObj.obj;
        bullet = bulletObj.transform;
        bullet.position = hand.transform.position;

        Vector3 destPos = target.position;
        GameObject bone = target.FindBone("Bip01");
        if (bone != null)
        {
            destPos.y = bone.transform.position.y;
        }

        Vector3 dir = new Vector3(destPos.x - bullet.position.x, destPos.y - bullet.position.y, destPos.z - bullet.position.z);
        dir.Normalize();
        bullet.rotation = Quaternion.FromToRotation(new Vector3(0f, 0f, 1f), new Vector3(dir.x, dir.y, dir.z));

        // 子弹碰撞检测
        float bulletSpeed = BulletSpeed;

        Move(bullet, destPos, bulletSpeed, delegate
        {
            OnCollision(target);

            DestroyEff(effObj);
            if (--bulletNum <= 0)
            {
                DoTskill();
                End();
            }
        });
    }

    protected virtual void OnCollision(Character target)
    {
        // AOE伤害
        if (damageTranJson.IsArray)
        {
            if (damageTranJson[0].IsArray)
            {
                CalcCircalDamage(attacker, Fight.Inst.FindAllAttackTarget(attacker), target.position, Damage, 0f);
                return;
            }
        }
        // 目标伤害计算
        CalcDamage(attacker, target, Damage, 0f);
    }

    protected virtual IEnumerator FireBullet()
    {
        // 播放动画和特效
        PlaySingAnimAndEffect();

        yield return new WaitForSeconds(SingTime);

        ShowMesh(false);

        // 发射子弹
		string bulletModel = BulletModel;
        List<Character> targets = FindTargets(true);
        bulletNum = targets.Count;

        if (targets.Count > 0)
        {
            attacker.LookAt(targets[0]);

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
