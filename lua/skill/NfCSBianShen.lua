require 'NfCSBase'

local NfCSBianShen = NfCSBase:New{
}

function NfCSBianShen.onBegin(self)
    local skill = self.skill

    NfCSBase.onBegin(self, 2)

    skill:AddEvent(skill.SingTime, function()
        self.Fire(self)
    end)
end

function NfCSBianShen.Fire( self )
    local skill = self.skill
    local attacker = self.attacker
    local attackers = self.attackers

    local buffs = skill.Buffs
    local tbuffs = skill.Tbuffs

    -- 删除变身buff
    for i=0, tbuffs.Count-1 do
        local b = tbuffs[i]
        attacker:EndBuff(b)
    end

    local buffLv = skill.lv
    local buff1 = buffs[0]
    attacker:AddBuff(attacker, buff1, buffLv)

    if skill.Tskill ~= -1 then
        skill:AddEvent(0.001, function()
            attacker:DoSkill(skill.Tskill, skill.lv, skill)
        end)
    end

    if buffs.Count > 1 then
        local buff2 = buffs[1]
        for i=1, attackers.Count-1 do
            local c = attackers[i]
            c:AddBuff(c, buff2, buffLv)
        end
    end

    skill:End(skill.TotalTime)
end

return NfCSBianShen