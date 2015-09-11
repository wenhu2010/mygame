require 'global'
local json = require 'json'

local function GetDir(tran, destPos)    
    local dir = destPos - tran.position
    dir:Normalize()
    return dir
end

local function SetDir(tran, dir)
    tran.rotation = Quaternion.LookRotation(dir)
end

local function CalcDamage(attacker, skill, dam, data, target, bullet)
    local targets = Fight.Inst:FindAllAttackTarget(attacker)
    local s = string.sub(data.Range,1,2)
    local pos = bullet.position
    local rot = bullet.rotation
    if s == '[[' then
        local range = json.decode(data.Range)
        for j=0,targets.Count-1 do
            local t = targets[j]
            local dist = Vector3.Distance(t.position, pos)
            for i=1,#range do
                local sr = range[i]
                local r = sr[1]
                local n = sr[2]
                if dist <= r then
                    skill:CalcDamage(attacker, t, dam*n, 0)
                    break
                end
            end
        end

    else
        skill:CalcDamage(attacker, target, dam, 0)
    end
    
    local hitEff = skill:PlayEffectSync(data.Eff, 3)
    if hitEff ~= nil then
        hitEff.position = pos
        hitEff.rotation = rot
    end
end

local function MoveToPos(attacker, bullet, destPos, speed, gravity, endFn, collideFn, param)
    local v2DestPos = Vector2(destPos.x, destPos.z)
    local v2SrcPos = Vector2(bullet.position.x, bullet.position.z)
    local dir = v2DestPos-v2SrcPos
    dir = dir.normalized

    local dist = Vector2.Distance(v2DestPos, v2SrcPos)
    local total = dist / speed
    local h = destPos.y - bullet.position.y
    local v = (h - 0.5 * (-gravity) * total * total) / total
    local vo = Vector3(dir.x * speed, v, dir.y * speed)
    local acc = Vector3(0, -gravity, 0)
    local srcPos = bullet.position

    local pictch = Mathf.Atan2(v, speed) * Mathf.Rad2Deg
    local axis = Vector3(0, 0, 1)
    bullet.rotation = Quaternion.FromToRotation(axis, Vector3(dir.x, 0, dir.y)) * Quaternion.Euler(-pictch, 0, 0)

    local time = 0
    attacker:AddEvent(function()
        time = time + Time.deltaTime
        local at2 = 0.5 * time * time
        local addpos = vo * time + acc * at2
        bullet.position = srcPos + addpos

        local vt = v + (-gravity) * time
        pictch = Mathf.Atan2(vt, speed) * Mathf.Rad2Deg
        bullet.rotation = Quaternion.FromToRotation(axis, Vector3(dir.x, 0, dir.y)) * Quaternion.Euler(-pictch, 0, 0)

        if collideFn and collideFn(bullet, param) then
            endFn()
            return true
        end

        if time >= total then
            endFn()
            return true
        end
        return false
    end)
end

local function MoveToTarget(attacker, tran, destTran, speed, endFn)
    local destPos = destTran.position
    local dir = GetDir(tran, destPos)
    SetDir(tran, dir)

    local dist = Vector3.Distance(tran.position, destPos)
    if dist > 0 then
        local ttime = dist / speed
        local time = 0
        attacker:AddEvent(function()
            time = time + Time.deltaTime
            destPos = destTran.position
            dir = GetDir(tran, destPos)
            SetDir(tran, destPos)

            if time >= ttime then
                tran.position = destPos
                endFn()
                return true
            else
                dist = Vector3.Distance(destPos, tran.position)
                speed = dist / (ttime - time)
                local md = Time.deltaTime * speed
                tran.position = tran.position + dir * md
            end
            return false
        end)
    else
        endFn()
    end
end

local function FireToPos(attacker, skill, dam, destPos, data, bullet, callback)
    MoveToPos(attacker, bullet, destPos, data.Speed, data.G, function()
        skill:DestroyEff(bullet)
        CalcDamage(attacker, skill, dam, data, nil, bullet)
        callback()
    end, nil, nil)
end

local function FireToTarget(attacker, skill, dam, target, data, bullet, callback)
    local hitBone = target:FindBone("Bip01")
    local destTran = target.transform
    if hitBone ~= nil then
        destTran = hitBone.transform
    end

    MoveToTarget(attacker, bullet, destTran, data.Speed, function()
        skill:DestroyEff(bullet)
        CalcDamage(attacker, skill, dam, data, target, bullet)
        callback()
    end)
end

local function OnceBulletCollide(bullet, param)
    local rm = {}
    local idx = 1
    local hit = false
    local targets = param.targets
    local callback = param.callback
    local r = param.radius
    for i=0,targets.Count-1 do
        local t = targets[i]
        if t.CanBeAtk then
            local dist = Vector3.Distance(t.position, bullet.position)
            if dist <= r then
                callback(t)
                return true
            end
        end
    end
    return false
end

local function PenetrateBulletCollide(bullet, param)
    local rm = {}
    local idx = 1
    local targets = param.targets
    local callback = param.callback
    local r = param.radius
    for i=0,targets.Count-1 do
        local t = targets[i]
        if t.CanBeAtk then
            local dist = Vector3.Distance(t.position, bullet.position)
            if dist <= r then
                callback(t)
                rm[idx] = t
            end
        end
    end
    for n=1,#rm do
        targets:Remove(rm[n])
    end
    return false
end

local function FireByDir(attacker, skill, dam, destPos, data, bullet, callback)
    local bone = Helper.FindObject(attacker.gameObject, data.Bone)
    local srcPos = attacker.position
    if bone ~= nil then        
        srcPos = bone.transform.position
    end

    local dir = destPos - bullet.position
    dir = Vector3(dir.x, 0, dir.z)
    dir:Normalize()
    destPos = bullet.position + dir * data.Dist
    local r = data.Radius
    local targets = Fight.Inst:FindAllAttackTarget(attacker)
    local t = data.Type

    local targets = Fight.Inst:FindAllAttackTarget(attacker)
    local collideFn
    local param = {}
    param.targets = targets
    param.radius = data.Radius
    param.callback = function(target)
        CalcDamage(attacker, skill, dam, data, target, bullet)
    end

    if t == 4 then
        collideFn = PenetrateBulletCollide
    else
        collideFn = OnceBulletCollide
    end

    MoveToPos(attacker, bullet, destPos, data.Speed, data.G, function()
        skill:DestroyEff(bullet)
        callback()
    end, collideFn, param)
end

local function CreateBullet(attacker, skill, data)
    local bone = Helper.FindObject(attacker.gameObject, data.Bone)
    local srcPos = attacker.position
    if bone ~= nil then        
        srcPos = bone.transform.position
    end

    local bullet = skill:PlayEffectSync(data.Model, -1)
    bullet.position = srcPos
    return bullet
end

function FireBulletByTarget(attacker, skill, dam, target, callback)
    local data = DataMgr.GetBullet(skill.Bullet)
    if data then
        local bullet = CreateBullet(attacker, skill, data)
        if data.Type == 1 then
            FireToPos(attacker, skill, dam, target.position, data, bullet, callback)
        elseif data.Type == 2 then
            if target then
                FireToTarget(attacker, skill, dam, target, data, bullet, callback)
            end
        else
            FireByDir(attacker, skill, dam, target.position, data, bullet, callback)
        end
    end
end

function FireBulletByPos(attacker, skill, dam, destPos, callback)
    local data = DataMgr.GetBullet(skill.Bullet)
    if data then
        local bullet = CreateBullet(attacker, skill, data)
        if data.Type == 1 then
            FireToPos(attacker, skill, dam, destPos, data, bullet, callback)
        else
            FireByDir(attacker, skill, dam, destPos, data, bullet, callback)
        end
    end
end