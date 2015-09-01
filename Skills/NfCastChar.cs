using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfCastChar : NfSkill {

    Character castChar;
    protected override void OnSkillBegin()
    {
        castChar = null;
        StartCameraAnim();

        StartCoroutine(Cast());
    }

    protected override void OnSkillEnd()
    {
        DestroyAll();

        if (castChar != null)
        {
            castChar.transform.parent = null;
            castChar.MoveBack();
            //castChar.position = castChar.SrcPos;
            //castChar.rotateBack();
            //castChar.Idle();
        }
    }

    IEnumerator Cast()
    {
        // 播放动画和特效
        PlaySingAnimAndEffect();

        float animLen = attacker.GetAnimLength(SingAnim);

        AddEvent(animLen, delegate
        {
            attacker.Idle();
        });

		string fireBone = FireBone;
        int slot = attacker.slot;
        CampType enemyCamp = attacker.camp == CampType.Friend ? CampType.Enemy : CampType.Friend;
        PlayGeZiEffect(slot, enemyCamp);

        var destPos = Fight.Inst.GetSlotPos(enemyCamp, slot);
        castChar = Fight.Inst.FindBackTarget(attacker);
        EffectObject keepEffObj = null;
        if (castChar != null)
        {
            GameObject hand = attacker.FindBone(fireBone);//Helper.FindObject(gameObject, FireBone, false);
            castChar.position = new Vector3(hand.transform.position.x, hand.transform.position.y, hand.transform.position.z);
            castChar.transform.parent = hand.transform;
            castChar.rotation = Quaternion.Euler(new Vector3(90, 0, 0));

            keepEffObj = PlayEffect(castChar, KeepEffect, -1f);
        }

        yield return new WaitForSeconds(SingTime);

        if (castChar == null)
        {
			FireBullet(destPos, fireBone, BulletModel);
        }
        else
        {
            castChar.transform.parent = null;
            CastChar(destPos, castChar, fireBone, keepEffObj);
        }
    }

    protected void FireBullet(Vector3 destPos, string fireBone, string bulletModel)
    {
        Transform bullet = null;
        GameObject hand = attacker.FindBone(fireBone);//Helper.FindObject(gameObject, fireBone, false);
        EffectObject effObj = PlayEffect(bulletModel, -1f);
        GameObject bulletObj = effObj.obj;//Instantiate(Resources.Load(bulletModel)) as GameObject;
        bullet = bulletObj.transform;
        bullet.position = new Vector3(hand.transform.position.x, hand.transform.position.y, hand.transform.position.z);

        Vector3 dir = new Vector3(destPos.x - bullet.position.x, destPos.y - bullet.position.y, destPos.z - bullet.position.z);
        dir.Normalize();
        bullet.rotation = Quaternion.FromToRotation(new Vector3(0f, 0f, 1f), new Vector3(dir.x, dir.y, dir.z));

        // 子弹碰撞检测
		float bulletSpeed = BulletSpeed;

        Move(bullet, destPos, bulletSpeed, delegate
        {
            OnCollision(destPos);

            DestroyEff(effObj);
            float time = attacker.GetAnimLength("hit");
            End(time);
        });
    }

    private void OnCollision(Vector3 destPos)
    {
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

        EffectObject fireEffObj = PlayEffect(FireEffect, 3f);
        if (fireEffObj != null)
        {
            fireEffObj.obj.transform.position = destPos;
        }
    }

    protected void CastChar(Vector3 destPos, Character castChar, string fireBone, EffectObject keepEffObj)
    {
        GameObject hand = attacker.FindBone(fireBone);//Helper.FindObject(gameObject, fireBone, false);
        castChar.position = new Vector3(hand.transform.position.x, hand.transform.position.y + 2f, hand.transform.position.z);

        Vector3 dir = new Vector3(destPos.x - castChar.position.x, destPos.y - castChar.position.y, destPos.z - castChar.position.z);
        dir.Normalize();
        castChar.LookAt(destPos);

        // 子弹碰撞检测
		float bulletSpeed = BulletSpeed;

        float dist = Vector3.Distance(castChar.position, destPos);
        float mttime = dist / bulletSpeed;
        float mtime = 0f;

        float xAng = 0;
        AddEvent(delegate
        {
            bool collide = false;
            mtime += Time.deltaTime;
            if (mtime >= mttime)
            {
                castChar.position = new Vector3(destPos.x, destPos.y, destPos.z);
                collide = true;
            }
            else
            {
                float md = Time.deltaTime * bulletSpeed;
                castChar.position += dir * md;
                xAng += Time.deltaTime * 20000;
                castChar.rotation = Quaternion.Euler(new Vector3(0, 0, xAng));
            }

            if (collide)
            {
                OnCollision(destPos);

                if (keepEffObj != null)
                {
                    DestroyEff(keepEffObj);
                    keepEffObj = null;
                }

                if (attacker.IsDead == false)
                    StartCoroutine(GoBack(castChar));
                else
                    End();
                return true;
            }

            return false;
        });
    }

    IEnumerator GoBack(Character castChar)
    {
        yield return new WaitForSeconds(0.3f);

        MoveSrcPos(castChar);
    }
}
