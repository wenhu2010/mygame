using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfCrossAttack : NfSkill
{
    protected override void OnSkillBegin()
    {
        StartCameraAnim();
        StartCoroutine(Attack());
    }

    protected override void OnSkillEnd()
    {
        DestroyAll();
    }

    IEnumerator Attack()
    {
        EffectObject bulletObj = PlayEffect(BulletModel, 5f);
        Transform bullet = bulletObj.obj.transform;
        Character target = GetFirstAttackTarget();
        Vector3 center = target.position;
        attacker.LookAt(target.position);

        bullet.rotation = attacker.rotation;
        bullet.position = target.position;

        PlaySingAnimAndEffect();
        yield return new WaitForSeconds(SingTime);


        // 计算技能OBB
        OBB verObb = new OBB(new Vector2(center.x, center.z), new Vector2(6f, 0.1f), bullet.rotation.eulerAngles.y);
        float rowAngle = bullet.rotation.eulerAngles.y + 90f;
        if (rowAngle >= 360f)
            rowAngle = rowAngle - 360f;
        OBB horObb = new OBB(new Vector2(center.x, center.z), new Vector2(6f, 0.1f), rowAngle);
        // 检测碰撞
        List<Character> enemys = Fight.Inst.FindAllAttackTarget(attacker);
        float dam = Damage;

        ClearGeZiEffects();

        float[] hitTimes = HitTimes;
        yield return new WaitForSeconds(hitTimes[0]);
        foreach (Character e in enemys)
		{
			if (e.CanBeAtk == false)
				continue;
            OBB obb = new OBB(new Vector2(e.position.x, e.position.z), new Vector2(1f, 1f), e.rotation.eulerAngles.y);
            if (horObb.Intersects(obb))
            {
                CalcDamage(attacker, e, dam, 0f);

                PlayGeZiEffect(e.slot, e.camp);
            }
        }

        yield return new WaitForSeconds(hitTimes[1]);
        foreach (Character e in enemys)
		{
			if (e.CanBeAtk == false)
				continue;
            OBB obb = new OBB(new Vector2(e.position.x, e.position.z), new Vector2(1f, 1f), e.rotation.eulerAngles.y);
            if (verObb.Intersects(obb))
            {
                CalcDamage(attacker, e, dam, 0f);

                PlayGeZiEffect(e.slot, e.camp);
            }
        }

        End();
    }
}
