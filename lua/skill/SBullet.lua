require 'NfSkillBase'
require 'BulletUtil'

local SBullet = NfSkillBase:New{
}

function SBullet:onBegin()
    local skill = self.skill
    local attacker = self.attacker
    
    skill:StartCameraAnim()
    skill:PlaySingAnimAndEffect()

    skill:AddEvent(skill.SingTime, function()
        local dam = skill.Damage
        if skill.rangeType == 8 then
            local destPos = GetSlotPos(RevCamp(attacker.camp), attacker.slot)
            FireBulletByPos(attacker, skill, dam, destPos, function()
                self:Finish()
            end)
        else
            local targets = skill:FindTargets(true)
            if targets.Count == 1 then
                if attacker ~= targets[0] then
                    attacker:LookAt(targets[0].position)
                end
            end

            local bulletNum = targets.Count
            if bulletNum > 0 then
                for i=0,targets.Count-1 do
                    local t = targets[i]
                    FireBulletByTarget(attacker, skill, dam, t, function()
                        bulletNum = bulletNum - 1
                        if bulletNum <= 0 then
                            self:Finish()
                        end
                    end)
                end
            else
                skill:End()
            end
        end
    end)
end

function SBullet:Finish()
    local skill = self.skill
    skill:DoTskill()
    skill:End()
end

return SBullet