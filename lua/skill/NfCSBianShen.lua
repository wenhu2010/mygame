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

function NfCSBianShen.Fire(self)
    local skill = self.skill
    local attacker = self.attacker
    local attackers = self.attackers
    local buffs = skill.Buffs
    local tbuffs = skill.Tbuffs

    local i = 0
    while i < skill.MaxNum and i < attackers.Count do
        local c = attackers[i]
        -- 删除变身buff
        for i=0, tbuffs.Count-1 do
            local b = tbuffs[i]
            c:EndBuff(b)
        end

        local buffLv = skill.lv
        for i=0, buffs.Count-1 do
            local b = buffs[i]
            c:AddBuff(attacker, b, buffLv)
        end

        i = i + 1
    end

    attacker:PushStartSkill(skill.Tskill, skill.lv)

    skill:End(skill.TotalTime)
end

return NfCSBianShen