using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfComboSkill : NfSkill
{
    protected List<Character> attackers;

    protected override void OnSkillBegin()
    {
        attackers = GetComboCharList();

        HideScene();

        PlayCombAnimAndEffect();
        StartCameraAnim();

        StartCoroutine(ComboAtk());
    }

    protected override void OnSkillEnd()
    {
        for (int i = 0; i < attackers.Count; ++i)
        {
            Character c = attackers[i];
            c.position = c.SrcPos;
            c.Idle();
        }

        DestroyAll();
        DestroyAllEff();
    }

    protected override void StopCameraAnim()
    {
        base.StopCameraAnim();

        for (int i = 0; i < attackers.Count; ++i)
        {
            Character c = attackers[i];
            c.position = c.SrcPos;
            c.Idle();
        }

        ShowScene();
    }

    protected virtual IEnumerator ComboAtk()
    {
        float time = TotalTime;
        if (time < 0.01f)
        {
            time = animation["heji"].length;
        }

        int dam = GetComboDamage(attackers);

        string bulletModel = BulletModel;
        float hitTime = HitTime;

        List<Character> targets = FindTargets(true);

        yield return new WaitForSeconds(hitTime);

        for (int i = 0; i < targets.Count; ++i)
        {
            Character c = targets[i];

            Vector3 beginPos = attacker.position + new Vector3(0f, 0.6f, 0f);
            Vector3 endPos = new Vector3(c.position.x, 0.6f, c.position.z);
            PlayChainEff(bulletModel, time - hitTime, beginPos, endPos);
            CalcDamage(attacker, c, dam, 0f);
        }

        yield return new WaitForSeconds(time - hitTime);

        End();
    }

    protected virtual void PlayCombAnimAndEffect()
    {
        for (int i = 0; i < attackers.Count; ++i)
        {
            Character c = attackers[i];
            //if (attacker.camp == CampType.Friend)
            //    c.position = Fight.Inst.GetSlotPos(attacker.camp, 11);
            //else
            c.position = Vector3.zero;
            c.rotation = c.SrcRotation;
            c.PlayAnim("heji");
        }
        PlayEffect(attacker.gameObject, FireEffect, 3f);
    }
}
