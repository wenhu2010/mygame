require 'NfSkillBase'

local NfAddHp = NfSkillBase:New{
}

function NfAddHp.onBegin(self)
    local skill = self.skill
    local attacker = self.attacker
    
    skill:StartCameraAnim()
    skill:PlayFireAnimAndEffect()
    
    local targets = skill:FindTargets(false)
    if targets.Count == 1 then
        if attacker ~= targets[0] then
            attacker:LookAt(targets[0].position)
        end
    end

    local hitEff = skill.HitEffect
    local dam = skill.Damage

    skill:AddEvent(skill.HitTime, function()
	    for i=0,targets.Count-1 do
	        local target = targets[i]
	        skill:AddHp(attacker, target, dam, hitEff, 0)
	        skill:AddBuff(target)
	    end
    end)

    -- 结束技能
    local animLen = attacker:GetAnimLength(skill.FireAnim)
    skill:AddEvent(animLen, function()
        attacker:rotateBack()
        skill:DoTskill()
        skill:End()
    end)
end

function NfAddHp.onEnd(self)

end

return NfAddHp