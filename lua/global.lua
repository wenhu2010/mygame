
import "UnityEngine"
local msgpack = require 'MessagePack'

function newObject(self, o)
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

function send(fn, param, call)
    param.func = fn
    Http.Send(msgpack.pack(param), function(buf)
        call(msgpack.unpack(buf))
    end)
end