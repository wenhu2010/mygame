//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class NfAddHp : NfSkill {

//    protected override void OnSkillBegin()
//    {
//        StartCameraAnim();

//        List<Character> targets = FindTargets(false);

//        PlayFireAnimAndEffect();

//        if (targets.Count == 1)
//        {
//            if (attacker != targets[0])
//            {
//                attacker.LookAt(targets[0].position);
//            }
//        }

//        string hitEff = HitEffect;
//        AddEvent(HitTime, delegate
//        {
//            targets = FindTargets(false);
//            foreach (var target in targets)
//            {
//                AddHp(attacker, target, (int)Damage, hitEff, 0f);
//                AddBuff(target);
//            }
//        });

//        // 结束技能
//        float animLen = attacker.GetAnimLength(FireAnim);
//        AddEvent(animLen, delegate
//        {
//            attacker.rotateBack();

//            DoTskill();
//            End();
//        });
//    }

//    protected override void OnSkillEnd()
//    {
//    }
//}
