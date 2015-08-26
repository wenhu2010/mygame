
require 'global'

local test = {
	v1 = 0,
	v2 = 0
}

function test.new(o)
	o = o or {}
	setmetatable(o, test)
	test.__index = test
	return o
end

function test.func1(self, v)
	self.v1 = v
end

function test.print(self)
	print("v1", self.v1)
end

return test