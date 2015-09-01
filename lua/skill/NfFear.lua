require 'NfSkillBase'

local NfFear = NfSkillBase:New{
    target,
    effKeepObj,
    destPos
}

function NfFear.onBegin(self)
    local skill = self.skill
    local attacker = self.attacker
    local target = attacker.target
    self.target = attacker.target
    
    skill:StartCameraAnim()
    skill:PlayFireAnimAndEffect()

    target.isFear = true

    local hitTime = skill.HitTime

    skill:AddEvent(hitTime, function( )
	    local dam = skill.Damage
		local targets = skill:FindTargets(true)
		for i=0,targets.Count-1 do
		    skill:CalcDamage(attacker, targets[i], dam, 0)
		end

		if target.IsDead == false then
		    self.Fear(self, target)
		else
    		local animLen = attacker:GetAnimLength(skill.FireAnim)
		    skill:End(animLen - hitTime)
		end
    end)
end

function NfFear.Fear(self, target)
    local skill = self.skill
    local attacker = self.attacker
    local slots = skill:GetEmptySlots(RevCamp(attacker.camp))
    self.destPos = target.position
    local fearPos = target.position
    if slots.Count > 0 then
    	target.slot = slots[0]
    	self.destPos = Fight.Inst:GetSlotPos(target.camp, target.slot)
    	target.SrcPos = self.destPos
    	Fight.Inst:SortAllChar()
    end
                
    self.effKeepObj = target:PlayEffect(skill.KeepEffect, -1)
    self.FearMove(self, target, fearPos, 4, 0)
end

function NfFear.RandPos(pos, r)
    local x = Fight.RandF(pos.x - r, pos.x + r)
    local z = Fight.RandF(pos.z - r, pos.z + r);
    z = Mathf.Clamp(z, -4, 3)
    return Vector3(x, pos.y, z)
end

function NfFear.FearMove(self, target, fearPos, num, delay )
    local skill = self.skill
    local attacker = self.attacker

    skill:AddEvent(delay, function()
        local pos
        if num > 1 then        
            pos = self.RandPos(fearPos, 3)
        else
            pos = self.destPos
        end

        target:LookAt(pos)
        target:PlayAnim("run", target.moveAnimSpeed)

        skill:Move(target, pos, target.moveSpeed, "run", target.moveAnimSpeed, function()
        	num = num - 1
            if num < 1 then
                target.position = pos
                skill:End()
            else
                target.position = pos
                self.FearMove(self, target, fearPos, num, 0.4)
            end
        end)
    end)
end

function NfFear.onEnd(self)
	local target = self.target
    if target ~= nil then
        target.isFear = false
        target:DestroyEff(self.effKeepObj)
        if target.IsDead == false then
        	target:MoveBack()
        end
    end
end

return NfFear
