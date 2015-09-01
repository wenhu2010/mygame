using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfShiHuaBuff : NfDizzyBuff, IWuDiBuff {

	Dictionary<Material, Texture> backupTexs = new Dictionary<Material, Texture>();

	protected override bool Init ()
	{
        if (caster == owner && Fight.Inst.FindAllFriend(owner).Count == 1)
            return false;

		foreach (var b in owner.Buffs)
		{
			if (b.enabled && b is NfShiHuaBuff)
			{
				return false;
			}
		}

        if (owner.IsDead)
			return false;
		if (caster != owner && owner.IsAntiDizzy)
		{
			owner.ShowAnti();
			return false;
		}
        float per = Percent;
        if (Fight.Rand(0, 101) > per)
            return false;

        num = Num;

        owner.EnableDizzy();

		ChangeTexture();
		ChangeAttri(owner);

        owner.PlayEffect(Effect, 3f);
		owner.StopAnim ();
		return true;
	}

	protected override void OnEnd ()
	{
        PushTskill();

		RestoreTexture();
		RestoreChangeAttri(owner);

        owner.PlayAnim(Anim);
        owner.PlayEffect(Effect, 3f);
		base.OnEnd ();
	}
	
	private void ChangeTexture()
	{
		backupTexs.Clear();

		string textPath = HitEffect;
		
		Texture image = Resources.Load(textPath) as Texture;

        SkinnedMeshRenderer[] smr = owner.GetSkinnedMeshRenderer();
		foreach (var r in smr)
		{
			foreach (var m in r.materials)
			{
				backupTexs.Add(m, m.mainTexture);
				m.mainTexture = image;
			}
		}
	}
	
	private void RestoreTexture()
	{
        SkinnedMeshRenderer[] smr = owner.GetSkinnedMeshRenderer();
		foreach (var r in smr)
		{
			foreach (var m in r.materials)
			{
				if (backupTexs.ContainsKey(m))
				{
					m.mainTexture = backupTexs[m];
				}
			}
		}
		backupTexs.Clear();
	}
}
