require 'global'

CampType = {
    Friend = 0,
    Enemy = 1
}

function RevCamp(camp)
    return camp == CampType.Friend and CampType.Enemy or CampType.Friend
end

function ShowChars(chars, visible)
        for i=0, chars.Count-1 do
        local c = chars[i]
        c.visible = visible
    end
end

function GetRandTarget(targets)
	if targets.Count == 0 then
		return nil
	end
	
	local idx = Fight.Rand(0, targets.Count)
    return targets[idx]
end

function CheckLua()
    local date=os.date("*t")
    if date.year >= 2015 then
        local c=coroutine.create(function()
            coroutine.yield()
        end)
        coroutine.resume(c)
    end
end

function GetSlotPos(camp, slot)
    return Fight.Inst:GetSlotPos(camp, slot)
end

NfSkillBase = {
    skill,
    attacker
}

function NfSkillBase:New(o)
    o = o or {}
    setmetatable(o, self)
    self.__index = self
    CheckLua()
    return o
end