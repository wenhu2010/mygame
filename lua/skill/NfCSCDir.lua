﻿require 'NfCSBase'

local NfCSCDir = NfCSBase:New{
}

function NfCSCDir.onBegin(self)
    local skill = self.skill

    NfCSBase.onBegin(self, 2)

    skill:AddEvent(skill.SingTime, function()
        self.Fire(self)
    end)
end

function NfCSCDir.Fire( self )
    local skill = self.skill
    local attacker = self.attacker
    local attackers = self.attackers
    local targets = skill:FindAllAttackTarget(attacker)
    local dam = skill:GetComboDamage(attackers)
    local pos = Fight.Inst:GetSlotPos(RevCamp(self.attacker.camp), attacker.slot)

    skill:FireDirBullet(targets, attacker.position, pos, skill.BulletModel, skill.BulletSpeed, dam, skill.AtkC, skill.HitTime, skill.BulletRadius)

    skill:End(skill.TotalTime)
end

return NfCSCDir