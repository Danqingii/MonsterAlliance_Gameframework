
Launcher = {}
print("Launcher")

local this = Launcher

function Launcher.Start()
    print("初始化")
end

local numr = 0

function Launcher.Update()
   print(this.numr)
    this.numr =this.numr + 1
end