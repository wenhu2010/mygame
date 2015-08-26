require 'NfCSBase'

local NfCSCastChar = NfCSBase:New{
}

function NfCSCastChar.onBegin(self)
    print "NfCSCastChar.onBegin(self)"
    local skill = self.skill

    NfCSBase.onBegin(self, 2)

    local effs = {}
    local bones = {"Bip01 R Finger1", "Bip01 L Finger1"}
    local attackers = self.attackers

    skill:AddEvent(skill.SingTime, function()
        for i=1,attackers.Count-1 do
            local c = attackers[i]
            effs[i] = self.CatchChar(self, c, bones[i])
        end
    end)

    skill:AddEvent(skill.HitTime, function()
        self.Fire(self, effs)
    end)
end

function NfCSCastChar.Fire( self, effs )
    local skill = self.skill
    local attacker = self.attacker
    local attackers = self.attackers
    local targets = skill:FindAllAttackTarget(attacker)

    -- local animlen = attacker:GetAnimLength("heji")
    -- skill:AddEvent(animlen, function( )
    --     self.IdleAll(self, "idle")
    -- end)

    for i=1,attackers.Count-1 do
        local c = attackers[i]
        local enemyCamp = RevCamp(c.camp)
        skill:PlayGeZiEffect(c.slot, enemyCamp)

        local destPos = Fight.Inst:GetSlotPos(enemyCamp, c.slot)

        self.CastChar(self, c, destPos, targets, effs[i])
    end

end

function NfCSCastChar.CatchChar(self, castChar, bone)
    local skill = self.skill
    local attacker = self.attacker

    local hand = attacker:FindBone(bone)
    castChar.position = Vector3(hand.transform.position.x, hand.transform.position.y, hand.transform.position.z)
    castChar.transform.parent = hand.transform
    castChar.rotation = Quaternion.Euler(Vector3(90, 0, 0))

    return skill:PlayEffect(castChar, skill.KeepEffect, -1)
end

function NfCSCastChar.CastChar(self, castChar, destPos, targets, keepEffObj)
    local skill = self.skill
    local attacker = self.attacker

    castChar.transform.parent = nil

    local dir = Vector3(destPos.x - castChar.position.x, destPos.y - castChar.position.y, destPos.z - castChar.position.z)
    dir:Normalize()
    castChar:LookAt(destPos)

    -- 子弹碰撞检测
    local bulletSpeed = skill.BulletSpeed

    local dist = Vector3.Distance(castChar.position, destPos)
    local mttime = dist / bulletSpeed
    local mtime = 0
    local xAng = 0

    skill:AddEvent(function()
        local collide = false
        mtime = mtime + Time.deltaTime
        if mtime >= mttime then
            castChar.position = Vector3(destPos.x, destPos.y, destPos.z)
            collide = true
        else
            local md = Time.deltaTime * bulletSpeed
            castChar.position = castChar.position + dir * md
            xAng = xAng + Time.deltaTime * 20000
            castChar.rotation = Quaternion.Euler(Vector3(0, 0, xAng))
        end

        if collide then
            self.OnCollision(self, targets, destPos)

            if keepEffObj ~= nil then
                skill:DestroyEff(keepEffObj)
            end

            if attacker.IsDead == false then
                skill:AddEvent(0.3, function()
                    skill:MoveSrcPos(castChar)
                end)
            else
                skill:End()
            end
            return true
        end

        return false
    end)
end

function NfCSCastChar.OnCollision( self, targets, destPos )
    local skill = self.skill

    -- 目标伤害计算
    skill:CalcCircalDamage(self.attacker, targets, destPos, skill:GetComboDamage(self.attackers), 0)

    local fireEffObj = skill:PlayEffect(skill.FireEffect, 3)
    if fireEffObj ~= null then
        fireEffObj.position = destPos
    end
end

function NfCSCastChar.onEnd(self)
    local attackers = self.attackers
    for i=0,attackers.Count-1 do
        local c = attackers[i]
        c.transform.parent = nil
        c:MoveBack()
    end

    self.skill:DestroyAllEff()
    self.skill:ShowScene()
    self.skill:ShowAllFriend()
end

return NfCSCastChar