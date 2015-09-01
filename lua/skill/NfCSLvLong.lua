require 'NfCSBase'

local NfCSLvLong = NfCSBase:New{
}

function NfCSLvLong.onBegin(self)
    local skill = self.skill

    NfCSBase.onBegin(self, 2)

    skill:AddEvent(skill.SingTime, function()
        self.Fire(self)
    end)
end

function NfCSLvLong.Fire( self )
    local skill = self.skill
    local attacker = self.attacker
    local attackers = self.attackers
    local targets = skill:FindAllAttackTarget(attacker)
    local dam = skill:GetComboDamage(attackers)
    local pos = Fight.Inst:GetSlotPos(RevCamp(self.attacker.camp), 2)

    skill:FireDirBullet(targets, attacker.position, pos, skill.BulletModel, skill.BulletSpeed, dam, skill.AtkC, skill.HitTime, skill.BulletRadius)

    skill:End(skill.TotalTime)
end

function NfCSLvLong.OnStopCameraAnim(self)
    NfCSBase.OnStopCameraAnim(self)
    self.skill:DestroyAllEff()
end

return NfCSLvLong