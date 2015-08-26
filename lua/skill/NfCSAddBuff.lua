require 'NfCSBase'

local NfCSAddBuff = NfCSBase:New{
}

function NfCSAddBuff.onBegin(self)
    local skill = self.skill

    NfCSBase.onBegin(self, 2)

    skill:AddEvent(skill.SingTime, function()
        self.Fire(self)
    end)
end

function NfCSAddBuff.Fire( self )
    local skill = self.skill
    local attackers = self.attackers

    for i=0, attackers.Count-1 do
        local attacker = attackers[i]
        skill:AddBuff(attacker)
    end

    skill:End(skill.TotalTime)
end

return NfCSAddBuff