require 'NfCSBase'

local NfCSBullet = NfCSBase:New{
    bulletNum
}

function NfCSBullet.onBegin(self)
    local skill = self.skill

    NfCSBase.onBegin(self, 2)

    bulletNum = skill.AtkC * self.attackers.Count

    skill:AddEvent(skill.SingTime, function()
        self.Fire(self)

        for i=2,skill.AtkC do
            skill:AddEvent(skill.HitTime * (i - 1), function()
                self.Fire(self)
            end)
        end
    end)
end

function NfCSBullet.FireBullet(self, attacker, target)
    local skill = self.skill
    local hand = attacker:FindBone(skill.FireBone)

    local effObj = skill:PlayEffect(skill.BulletModel, -1)
    local bullet = effObj.transform
    bullet.position = hand.transform.position

    local destPos = target.position
    local bone = target:FindBone("Bip01")
    if bone ~= nil then
        destPos.y = bone.transform.position.y
    end

    local dir = Vector3(destPos.x - bullet.position.x, destPos.y - bullet.position.y, destPos.z - bullet.position.z)
    dir:Normalize()
    bullet.rotation = Quaternion.FromToRotation(Vector3(0, 0, 1), Vector3(dir.x, dir.y, dir.z))

    -- 子弹碰撞检测
    skill:Move(bullet, destPos, skill.BulletSpeed, function()
        NfCSBullet.OnCollision(self, attacker, target)

        skill:DestroyEff(effObj)

        bulletNum = bulletNum - 1
        if bulletNum <= 0 then
        	skill:End()
        end
    end)
end

function NfCSBullet.OnCollision(self, attacker, target)
    local skill = self.skill
    -- AOE伤害
    skill:CalcCircalDamage(attacker, skill:FindAllAttackTarget(attacker), target.position, skill:GetDamage(attacker), 0)
    -- skill:CalcDamage(attacker, target, skill.Damage, 0)
end

function NfCSBullet.Fire( self )
    local skill = self.skill
    local attackers = self.attackers
    local target = skill:GetFirstAttackTarget()

    for i=0, attackers.Count-1 do
        local attacker = attackers[i]
        attacker:LookAt(target)
        NfCSBullet.FireBullet(self, attacker, target)
    end
end

return NfCSBullet