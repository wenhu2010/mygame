require 'global'

local NfComboSkillBullet = {
    skill,
    attacker,
    attackers,
    bulletNum
}

function NfComboSkillBullet.onBegin(self)
    local skill = self.skill

    self.attackers = skill:GetComboCharList()    

    skill:HideScene() 

    NfComboSkillBullet.PlayCombAnimAndEffect(self)

    skill:StartCameraAnim()
    skill:HideAllFriend(self.attackers)

    skill:AddEvent(skill.SingTime, function( )
        NfComboSkillBullet.Attack(self)
    end)
end

function NfComboSkillBullet.PlayCombAnimAndEffect(self)
    local skill = self.skill
    local attackers = self.attackers

    for i=0, attackers.Count-1 do
        local c = attackers[i]
        c.position = Fight.Inst:GetSlotPos(self.attacker.camp, 2)
        c.rotation = c.SrcRotation
        c:PlayAnim("heji")
    end
    skill:PlayEffect(self.attacker, skill.SingEffect, 3)
end

function NfComboSkillBullet.OnStopCameraAnim(self)
    self.skill:ShowScene()
    self.skill:ShowAllFriend()
    NfComboSkillBullet.MoveSrc(self)
end

function NfComboSkillBullet.FireBullet(self, attacker, target)
    local skill = self.skill
    local hand = attacker:FindBone(skill.FireBone)

    local effObj = skill:PlayEffect(skill.BulletModel, -1)
    local bullet = effObj.obj.transform
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
        NfComboSkillBullet.OnCollision(self, attacker, target)

        skill:DestroyEff(effObj)

        bulletNum = bulletNum - 1
        if bulletNum <= 0 then
            skill:End()
        end
    end)
end

function NfComboSkillBullet.OnCollision(self, attacker, target)
    local skill = self.skill
    -- AOE伤害
    -- skill:CalcCircalDamage(attacker, skill:FindAllAttackTarget(attacker), target.position, skill:GetDamage(attacker), 0)
    skill:CalcDamage(attacker, target, skill.Damage, 0)
end

function NfComboSkillBullet.Attack( self )
    local skill = self.skill
    local attackers = self.attackers

    local bulletModel = skill.BulletModel
    local targets = skill:FindTargets(true)
    bulletNum = targets.Count * attackers.Count

    print(bulletNum, bulletModel)

    if targets.Count > 0 then
        local target = targets[0]
        for i=0, attackers.Count-1 do
            local attacker = attackers[i]
            attacker:LookAt(target)

            for i=0, targets.Count-1 do
                NfComboSkillBullet.FireBullet(self, attacker, targets[i])
            end
        end
    else
        End()
    end
end

function NfComboSkillBullet.onEnd(self)
    NfComboSkillBullet.MoveSrc(self)

    self.skill:DestroyAllEff()
end

function NfComboSkillBullet.MoveSrc(self)
    local attackers = self.attackers
    for i=0, attackers.Count-1 do
        local c = attackers[i]
        c.position = c.SrcPos
        c:Idle()
    end
end

return NfComboSkillBullet