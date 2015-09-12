require 'NfSkillBase'

local SWuShuang = NfSkillBase:New{
}

function SWuShuang:onBegin()
    local skill = self.skill
    local attacker = self.attacker
    
    skill:StartCameraAnim()
    skill:PlaySingAnimAndEffect()

    local hitEff = skill.HitEffect
    local dam = skill.Damage

    -- 结束技能
    skill:AddEvent(skill.SingTime, function()
        local destPos = GetSlotPos(RevCamp(attacker.camp), 11)
        local dir = Vector3.zero - destPos
        dir:Normalize()
        destPos = destPos + dir * attacker.radius
        skill:Move(attacker, destPos, attacker.moveSpeed, function()
            skill:PlayFireAnimAndEffect()
            local targets = skill:FindTargets(true)
            local animLen = attacker:GetAnimLength(skill.FireAnim)
            for i=0,targets.Count-1 do
                local t = targets[i]
                skill:CalcDamage(attacker, t, dam, 0.3)
                skill:CalcDamage(attacker, t, dam, 0.6)
                skill:CalcDamage(attacker, t, dam, 1.3)
                skill:CalcDamage(attacker, t, dam, 1.6)
                skill:CalcDamage(attacker, t, dam, 2)
                skill:CalcDamage(attacker, t, dam, 2.3)
            end
            -- skill:AddEvent(skill.TotalTime, function()
            --     local hitTime = skill.HitTime
            --     for i=0,targets.Count-1 do
            --         local t = targets[i]
            --         for n=1,skill.AtkC do
            --             skill:CalcDamage(attacker, t, dam, 0.2*(n-1))
            --         end
            --     end
            -- end)
            skill:AddEvent(animLen, function()
                skill:Move(attacker, attacker.SrcPos, attacker.moveSpeed, function()
                    skill:End()
                end)
            end)
        end)
    end)
end

return SWuShuang