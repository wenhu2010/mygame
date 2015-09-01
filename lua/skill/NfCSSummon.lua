require 'NfCSBase'

local NfCSSummon = NfCSBase:New{
}

function NfCSSummon.onBegin(self)
    local skill = self.skill

    NfCSBase.onBegin(self, 2)

    skill:AddEvent(skill.SingTime, function()
        self.Fire(self)
    end)
end

function NfCSSummon.Fire( self )
    local skill = self.skill
    local attackers = self.attackers
    local slots = skill:GetEmptySlots()
    local slot = 0
    if skill.Range == 0 then
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

    local c = skill:Summon(skill.Tbuff, slot, Mathf.RoundToInt(lv), Mathf.RoundToInt(jie))
    skill:PlayEffect(c, skill.FireEffect, 3)

    skill:End(skill.TotalTime)
end

return NfCSSummon