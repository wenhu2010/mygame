﻿require 'NfCSBase'

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
    local slots = skill:GetEmptySlots()
    local slot = 0
    if skill.Range == 0 then
        slot = slots[slots.Count - 1]
    else
        slot = slots[0]
    end
    local c = skill:Summon(skill.Tbuff, slot)
    skill:PlayEffect(c, skill.FireEffect, 3)

    skill:End(skill.TotalTime)
end

return NfCSSummon