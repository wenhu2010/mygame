using UnityEngine;
using System.Collections;

public class NfChuiQiBuff : NfDizzyBuff, IWuDiBuff
{
    float xAng;
	Vector3 pos;

    protected override bool Init()
    {
        if (owner.IsAntiDizzy)
        {
            owner.ShowAnti();
            return false;
        }

        num = Num;
        xAng = 0;

        PlaySelfEffect();
        if (effobj != null)
        {
            effobj.obj.transform.parent = null;
            effobj.obj.transform.position = owner.position;
        }

		pos = new Vector3(owner.position.x, owner.position.y + 1f, owner.position.z);
		owner.position = pos;
        owner.shadow.obj.transform.position = new Vector3(owner.position.x, 0f, owner.position.z);
        owner.EnableWuDi();
        owner.EnableDizzy();
        return true;
    }

    protected override void OnEnd()
    {
        base.OnEnd();
        owner.position = owner.SrcPos;
        owner.rotation = owner.SrcRotation;
        owner.shadow.obj.transform.position = new Vector3(owner.position.x, 0f, owner.position.z);
        owner.DisableWuDi();
        owner.DisableDizzy();
    }

    public override void HandleHuiHe()
    {
        if (--num < 0)
        {
            OnEnd();
        }
    }

    public override void Update()
    {
        if (enabled)
        {
			xAng += Time.deltaTime * 1000;
			owner.position = pos;
            owner.rotation = Quaternion.Euler(new Vector3(0, xAng, 0));
        }
    }
}

public class NfDizzyWudiBuff : NfDizzyBuff, IWuDiBuff
{
    protected override bool Init()
    {
        if (owner.IsAntiDizzy)
        {
            owner.ShowAnti();
            return false;
        }

        num = Num;

        PlaySelfEffect();

        owner.EnableWuDi();
        owner.EnableDizzy();
        return true;
    }

    protected override void OnEnd()
    {
        base.OnEnd();

        owner.DisableWuDi();
        owner.DisableDizzy();
    }

    public override void HandleHuiHe()
    {
        if (--num < 0)
        {
            OnEnd();
        }
    }
}
