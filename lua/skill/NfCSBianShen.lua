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
    local buffLv = skill.lv
    local i = 0
    local num = math.min(skill.MaxNum, buffs.Count, attackers.Count)
    while i < num do
        local c = attackers[i]
        -- 删除变身buff
        for i=0, tbuffs.Count-1 do
            local b = tbuffs[i]
            c:EndBuff(b)
        end

        local b = buffs[i]
        c:AddBuff(c, b, buffLv)
        i = i + 1
    end

    attacker:PushStartSkill(skill.Tskill, skill.lv)

    skill:End(skill.TotalTime)
end

return NfCSBianShen