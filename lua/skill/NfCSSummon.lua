require 'NfCSBase'

local NfCSSummon = NfCSBase:New{
    summon,
}

function NfCSSummon.onBegin(self)
    local skill = self.skill

    NfCSBase.onBegin(self, 2)

    skill:AddEvent(skill.SingTime, function()
        self:Fire()
    end)
end

function NfCSSummon.Fire(self)
    local skill = self.skill
    local attackers = self.attackers
    local slots = skill:GetEmptySlots()
    local slot = 0
    if skill.rangeType == 0 then
        slot = slots[slots.Count - 1]
    else
        slot = slots[0]
    end

    local lv = 0
    local jie = 0
    for i=0,attackers.Count-1 do
        local c = attackers[i]
        lv = lv + c.lv
        jie = jie + c.strengthenid
    end
    lv = lv / attackers.Count
    jie = jie / attackers.Count

    local destPos = self.attacker.position    
    local eff = skill:PlayEffect(skill.FireEffect, 3)
    if eff then
        eff.position = destPos
    end

    skill:AddEvent(skill.HitTime, function()
        self.summon = skill:Summon(skill.Tbuff, slot, Mathf.RoundToInt(lv), Mathf.RoundToInt(jie))
        self.summon.position = destPos
        self.summon:PushStartSkill(skill.Tskill, skill.lv)
    end)

    skill:End(skill.TotalTime)
end

function NfCSSummon.OnStopCameraAnim(self)
    NfCSBase.OnStopCameraAnim(self)
    self.summon.position = self.summon.SrcPos
end

return NfCSSummon