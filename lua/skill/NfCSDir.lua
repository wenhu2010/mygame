require 'NfCSBase'

local NfCSDir = NfCSBase:New{
}

function NfCSDir.onBegin(self)
    local skill = self.skill

    NfCSBase.onBegin(self, 2)

    skill:AddEvent(skill.SingTime, function()
        self.Fire(self)
    end)
end

function NfCSDir.Fire( self )
    local skill = self.skill
    local attacker = self.attacker
    local attackers = self.attackers
    local target = skill:GetFirstAttackTarget()
    local targets = skill:FindAllAttackTarget(attacker)
    local dam = skill:GetComboDamage(attackers)

    for i=0, attackers.Count-1 do
        local c = attackers[i]
        c:LookAt(target)
    end

    skill:FireDirBullet(attacker.position, target.position, skill.BulletModel, skill.BulletSpeed, dam, skill.AtkC, skill.HitTime)

    if skill.MaxNum > 0 then
        target = skill:GetSecondAttackTarget()
        skill:FireDirBullet(attacker.position, target.position, skill.BulletModel, skill.BulletSpeed, dam, skill.AtkC, skill.HitTime)
    end

    skill:End(skill.TotalTime)
end

return NfCSDir