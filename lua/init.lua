require "global"
-- require "utility"
-- local json = require('json')
-- local start = os.clock()
-- local s = json.encode({ 1, 2, 'fred', {first='mars',second='venus',third='earth'} })
-- print("json.encode " .. (os.clock() - start));
-- print(s)

-- local start = Time.realtimeSinceStartup
-- for i=1,1 do
-- 	json.decode(s)
-- end
-- local t = json.decode(s)
-- print("json.decode " .. (Time.realtimeSinceStartup - start))

-- print("first", t[4].first)

-- local msgpack = require 'MessagePack'

-- utility.print(msgpack.unpack(msgpack.pack({param="ffffe"})))

print "初始化完成"

return test