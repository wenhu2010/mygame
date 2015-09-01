using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfDartle : NfSkill
{
    int fireNum = 0;

    protected override void OnSkillBegin()
    {
        fireNum = AtkC;

        StartCameraAnim();
        StartCoroutine(Attack());
    }

    protected override void OnSkillEnd()
    {
        DestroyAll();

        attacker.rotateBack();
    }


    protected IEnumerator Attack()
    {
        // 播放动画和特效
        float singTime = attacker.GetAnimLength(SingAnim);
        PlaySingAnimAndEffect();
        yield return new WaitForSeconds(singTime);

        Character target = attacker.target;
        FindTarget(target, FireBone, BulletModel);
    }

    protected IEnumerator Fire(Character target, string fireBone, string bulletModel)
    {
        attacker.LookAt(target);

        animation.Stop(FireAnim);
        PlayFireAnimAndEffect();
        yield return new WaitForSeconds(SingTime);

        Transform bullet = null;
        GameObject hand = attacker.FindBone(fireBone);//Helper.FindObject(gameObject, fireBone, false);
        EffectObject effObj = PlayEffect(bulletModel, -1f);
        GameObject bulletObj = effObj.obj;
        bullet = bulletObj.transform;
        bullet.position = new Vector3(hand.transform.position.x, hand.transform.position.y, hand.transform.position.z);

        Vector3 destPos = target.position;
        GameObject bone = target.FindBone("Bip01");//Helper.FindObject(gameObject, "Bip01", false);
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
            // 目标伤害计算
            CalcDamage(attacker, target, Damage, 0f);

            DestroyEff(effObj);

            if (--fireNum > 0)
            {
                FindTarget(target, fireBone, bulletModel);
            }
            else
            {
                End();
            }
        });
    }

    private void FindTarget(Character target, string fireBone, string bulletModel)
    {
        Character atkTarget = target;
        if (target.IsDead)
        {
            atkTarget = Fight.Inst.FindExistBackTarget(target);
        }
        if (atkTarget == null)
        {
            atkTarget = GetFirstAttackTarget();
        }
        if (atkTarget != null && atkTarget.IsDead == false)
        {
            StartCoroutine(Fire(atkTarget, fireBone, bulletModel));
        }
        else
        {
            End();
        }
    }
}
