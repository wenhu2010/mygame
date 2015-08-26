require "global"

print "初始化完成"

local test = {
	v1 = 100,
	v2 = 0
}

function test.func1()
	print("v1", test.v1)
end

return test