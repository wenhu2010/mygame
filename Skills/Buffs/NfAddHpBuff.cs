using UnityEngine;
using System.Collections;

public class NfAddHpBuff : NfBuff {

	protected override bool Init ()
	{
        AddEvent(Percent, delegate
        {
			if (owner.IsDead == false)
			{
	            int addHp = (int)(TotalValue);
	            owner.AddHp(owner, addHp, Effect);
			}
        });
		return false;
	}
}
