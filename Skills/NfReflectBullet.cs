using UnityEngine;
using System.Collections;
using LitJson;
using System.Collections.Generic;

public class NfReflectBullet : NfSkill {

    int bulletNum = 0;

    protected override void OnSkillBegin()
    {
        StartCoroutine(FireBullet());
    }

    protected override void OnSkillEnd()
    {
        if (preSkill != null)
            preSkill.End();
        DestroyAll();
    }

    protected void Fire(Vector3 firePos, Character target, string bulletModel)
    {
        Transform bullet = null;
        EffectObject effObj = PlayEffect(bulletModel, -1f);
        GameObject bulletObj = effObj.obj;//Instantiate(Resources.Load(bulletModel)) as GameObject;
        bullet = bulletObj.transform;
        bullet.position = firePos;

        Vector3 destPos = new Vector3(target.position.x, 0f, target.position.z);
        Vector3 dir = new Vector3(destPos.x - bullet.position.x, 0f, destPos.z - bullet.position.z);
        dir.Normalize();
        bullet.rotation = Quaternion.FromToRotation(new Vector3(0f, 0f, 1f), new Vector3(dir.x, 0f, dir.z));

        // 子弹碰撞检测
		float bulletSpeed = BulletSpeed;

        Move(bullet, destPos, bulletSpeed, delegate
        {
            // 目标伤害计算
            CalcDamage(attacker, target, Damage, 0f);
            DestroyEff(effObj);
            if (--bulletNum <= 0)
            {
                if (skilldata.Tskill != -1)
                {
                    attacker.target = target;
                    DoTskill();
                }
                End();
            }
        });
    }

    protected IEnumerator FireBullet()
    {
        Character target = attacker.target;

        List<Character> targets = null;
        if (rangeType == RangeType.RandSingle)
        {
            List<Character> chars = Fight.Inst.FindAllAttackTarget(attacker);
            chars.Remove(target);

            targets = new List<Character>();
            int count = MaxNum;
            while (chars.Count > 0 && targets.Count < count)
            {
                int idx = Fight.Rand(0, chars.Count);
                Character c = chars[idx];
                chars.Remove(c);
                targets.Add(c);
            }
        }
        else
        {
            targets = FindTargets(true);
			targets.Remove(target);

			ClearGeZiEffects();
        }

        if (targets.Count > 0)
		{
			for (int i = 0; i < targets.Count; ++i)
			{
				PlayGeZiEffect(targets[i].slot, targets[i].camp);
			}

            bulletNum = targets.Count;

            yield return new WaitForSeconds(SingTime);

            string bulletModel = BulletModel;

            Vector3 firePos = new Vector3(target.position.x, target.position.y + 0.6f, target.position.z);

            foreach (var t in targets)
            {
                if (t != target)
                {
                    Fire(firePos, t, bulletModel);
                }
            }
        }
        else
        {
            yield return new WaitForEndOfFrame();
            End();
        }
    }
}
