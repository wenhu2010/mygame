using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfDirBullet : NfSkill
{
    protected override void OnSkillBegin()
    {
        List<Character> enemys = FindTargets(true);
        
        Character target = GetFirstAttackTarget();
        Vector3 atkPos = target.position;
        if (rangeType == RangeType.ForwardColumn)
        {
            atkPos = Fight.Inst.GetSlotPos(attacker.camp == CampType.Friend ? CampType.Enemy : CampType.Friend,
                attacker.slot);
        }
        else
        {
            ClearGeZiEffects();
        }

        StartCameraAnim();

        // 设置朝向
        Vector3 dir = atkPos - attacker.position;
        dir.Normalize();
        attacker.LookAt(atkPos);

        // 播放动画和特效
        EffectObject effObj = PlayFireAnimAndEffect(-1);
        GameObject eff = effObj.obj;
        eff.transform.rotation = attacker.rotation;

        // 计算技能OBB
        float dist = Vector3.Distance(atkPos, attacker.position) + 10f;
        Vector3 center = attacker.position + dir.normalized * dist / 2f;
        OBB obb = new OBB(new Vector2(center.x, center.z), new Vector2(dist / 2f, 1f), attacker.rotation.eulerAngles.y);

        // 检测碰撞
        float singTime = SingTime;
        float bulletSpeed = BulletSpeed;
        if (bulletSpeed < 0.01f)
        {
            singTime = HitTime;
        }
        float dam = Damage;
        int n = 0;
        foreach (Character e in enemys)
        {
			if (e.CanBeAtk == false)
				continue;
            //e.UpdateOBB();
            OBB obbe = new OBB(new Vector2(e.position.x, e.position.z), new Vector2(1f, 1f), e.rotation.eulerAngles.y);
            if (obb.Intersects(obbe))
            {
                float t = singTime;
                if (bulletSpeed > 0f)
                {
                    float s = Vector2.Distance(attacker.pos2v, e.pos2v);
                    t += s / bulletSpeed;
                }
                float r = 1f;
                if (n < damageTranJson.Count)
                    r = (float)damageTranJson[n++];
                float val = dam * r;
                CalcDamage(attacker, e, val, t);

                if (rangeType != RangeType.ForwardColumn)
                    PlayGeZiEffect(e.slot, e.camp);
            }
        }

        // 结束技能
        AddEvent(TotalTime, delegate
        {
            DoTskill();
            End();
        });
    }

    protected override void OnSkillEnd()
    {
        DestroyAll();
        DestroyAllEff();

        attacker.rotateBack();
    }
}
