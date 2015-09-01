using UnityEngine;
using System.Collections;

public class NfAntiBuff : NfBuff
{
    public enum AntiType
    {
        AntiDizzy = 1,
        AntiFanJi = 2,
        AntiDizzyFanJi = 3,
        AntiCrit = 4,
        AntiCharm= 5,
    }

    public AntiType type;
    int num;

    protected override bool Init()
    {
        num = Num;

        PlaySelfEffect();

        type = (AntiType)((int)BaseValue);
        switch (type)
        {
            case AntiType.AntiDizzy:
                owner.EnableAntiDizzy();
                break;
            case AntiType.AntiFanJi:
                owner.EnableAntiFanJi();
                break;
            case AntiType.AntiDizzyFanJi:
                owner.EnableAntiDizzy();
                owner.EnableAntiFanJi();
                break;
        }
        
        return true;
    }

    protected override void OnEnd()
    {
        base.OnEnd();

        switch (type)
        {
            case AntiType.AntiDizzy:
                owner.DisableAntiDizzy();
                break;
            case AntiType.AntiFanJi:
                owner.DisableAntiFanJi();
                break;
            case AntiType.AntiDizzyFanJi:
                owner.DisableAntiDizzy();
                owner.DisableAntiFanJi();
                break;
        }
    }

    public override void HandleHuiHe()
    {
        if (--num < 0)
        {
            OnEnd();
        }
    }
}
