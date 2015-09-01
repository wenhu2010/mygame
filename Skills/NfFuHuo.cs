using UnityEngine;
using System.Collections;

public class NfFuHuo : NfSkill {

    protected override void OnSkillBegin()
    {
        if (Fight.Inst.GetFuHuoCharacterCount(attacker.camp) > 0)
        {
            StartCameraAnim();
            PlaySingAnimAndEffect();

            Fight.DeadCharInfo dci = Fight.Inst.GetFuHuoCharacter(attacker.camp);
            attacker.LookAt(dci.chara);

            AddEvent(HitTime, delegate
            {
                dci.chara.FuHuo(Damage, dci.anger);
                PlayEffect(dci.chara.gameObject, HitEffect, 3f);

                Fight.Inst.RemoveFuHuoCharacter(dci);
            });

            End(attacker.GetAnimLength(SingAnim));
        }
        else
        {
            DoTskill();
            End();
        }
    }

    protected override void OnSkillEnd()
    {
        attacker.rotateBack();
        DestroyAll();
    }
}
