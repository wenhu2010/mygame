
import "UnityEngine"

function startCoroutine(fn)
    local c=coroutine.create(fn)
    coroutine.resume(c)
end

function waitTime(sec)
    Yield(WaitForSeconds(sec))
end

function waitEnd()
    Yield(WaitForEndOfFrame())
end

function waitFrameUpdate()
    Yield(WaitForFixedUpdate())
end

function newObject( self, o )
    o = o or {}
    setmetatable(o, self)
    self.__index = self
    return o
end

function inherite(class, o)
    o = o or {}
    setmetatable(o, class)
    class.__index = class
    return o
end