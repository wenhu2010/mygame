using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfFlopChar : NfSkill
{
    protected override void OnSkillBegin()
    {
        StartCameraAnim();

        StartCoroutine(Attack());
    }

    protected override void OnSkillEnd()
    {
        DestroyAll();

        Character target = attacker.target;
        target.transform.parent = null;
        target.position = target.SrcPos;
        target.rotation = target.SrcRotation;
    }

    IEnumerator Attack()
    {
        PlaySingAnimAndEffect();
        float singTime = SingTime;

        yield return new WaitForSeconds(singTime);
        
        Character target = attacker.target;

        Vector3 dest = new Vector3(target.position.x, target.position.y, target.position.z);
        Vector3 dir;
        dir = target.direction;
        dir.Normalize();
        dest = dest + dir * CalcRadius(attacker, target);

        MoveTo(dest);
    }

    IEnumerator OnAttack()
    {
        Character target = attacker.target;

		string fireBone = FireBone;
        GameObject hand = attacker.FindBone(fireBone);//Helper.FindObject(gameObject, fireBone, false);
        GameObject bindBone = target.FindBone("Bip01");//Helper.FindObject(target.gameObject, "Bip01", false);
        target.position = new Vector3(hand.transform.position.x, hand.transform.position.y - bindBone.transform.position.y, hand.transform.position.z);
        target.transform.parent = hand.transform;

        PlayFireAnimAndEffect();
        float animLen = attacker.GetAnimLength(FireAnim);
        float hitTime = HitTime;
        yield return new WaitForSeconds(animLen - hitTime);

        float dam = Damage;
        List<Character> targets = FindTargets(true);
        int n = 0;
        foreach (var t in targets)
        {
            float r = 1f;
            if (n < damageTranJson.Count)
                r = (float)damageTranJson[n++];
            float val = dam * r;
            CalcDamage(attacker, t, val, 0f);
        }

        yield return new WaitForSeconds(hitTime);

        MoveSrcPos(attacker);

        target.transform.parent = null;
        target.position = target.SrcPos;
        target.rotation = target.SrcRotation;
    }

    protected void MoveTo(Vector3 destPos)
    {
        Move(attacker, destPos, attacker.moveSpeed * 3f, "run", attacker.moveAnimSpeed * 4f, delegate
        {
            StartCoroutine(OnAttack());
        });
    }
}
