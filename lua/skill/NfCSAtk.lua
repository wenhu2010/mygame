require 'NfCSBase'

local NfCSAtk = NfCSBase:New{
}

function NfCSAtk.onBegin(self)
    local skill = self.skill

    NfCSBase.onBegin(self, 2)

    skill:AddEvent(skill.SingTime, function()
        self.Fire(self)
    end)
end

function NfCSAtk.Fire( self )
    local skill = self.skill
    local attacker = self.attacker
    local attackers = self.attackers
    local targets = skill:FindTargets(true)

    for i=0, targets.Count-1 do
        local t = targets[i]
        skill:CalcDamage(attacker, target, skill:GetComboDamage(attackers), 0)
    end
    
    skill:End(skill.TotalTime)
end

return NfCSAtk