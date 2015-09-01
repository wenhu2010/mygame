using UnityEngine;
using System.Collections;

public class NfWuDiBuff : NfBuff, IWuDiBuff
{
	protected int num = 0;
	
	protected override bool Init()
	{
		num = Num;
		PlaySelfEffect();
		return true;
	}
	
	public override void HandleHuiHe()
	{
		if (--num < 0)
		{
			End();
		}
	}
}