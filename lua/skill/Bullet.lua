require 'NfSkillBase'
require 'BulletUtil'

local Bullet = NfSkillBase:New{
}

function Bullet.onBegin(self)
    local skill = self.skill
    local attacker = self.attacker
    
    skill:StartCameraAnim()
    skill:PlaySingAnimAndEffect()

    -- skill:AddEvent(skill.SingTime, function()
    --     local dam = skill.Damage
    --     if skill.rangeType == 8 then
    --         local destPos = GetSlotPos(RevCamp(attacker.camp), attacker.slot)
    --         skill:FireBullet(attacker, destPos, dam, function()
    --             self.Finish(self)
    --         end)
    --     else
    --         local targets = skill:FindTargets(true)
    --         if targets.Count == 1 then
    --             if attacker ~= targets[0] then
    --                 attacker:LookAt(targets[0].position)
    --             end
    --         end

    --         local bulletNum = targets.Count
    --         if bulletNum > 0 then
    --             for i=0,targets.Count-1 do
    --                 local t = targets[i]
    --                 skill:FireBullet(attacker, t, dam, function()
    --                     bulletNum = bulletNum - 1
    --                     if bulletNum <= 0 then
    --                         self.Finish(self)
    --                     end
    --                 end)
    --             end
    --         else
    --             skill:End()
    --         end
    --     end
    -- end)

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

function Bullet.Finish(self)
    local skill = self.skill
    skill:DoTskill()
    skill:End()
end

return Bullet