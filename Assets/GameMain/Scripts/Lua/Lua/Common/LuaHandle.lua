local loadedLua = CS.LPCFramework.ResourceManager.LoadedLua

-- lua白名单（此名单下脚本不卸载，比如一些公共数据存储）
local blackLuaList = {
    ["Launcher.Launcher"] = 1,
    ["Launcher.ABUpdate"] = 1,
    ["Common.Common"] = 1,
    ["Common.LuaHandle"] = 1,
    ["Manager.VideoManager"] = 1,
    ["bit"] = 1,
    ["pb"] = 1
}

LuaHandle = {}

-- 加载lua
function LuaHandle.load(path)
    if nil == path then
        return
    end
    -- print("+++++++++++++++", path)
    -- 加载
    return require(path)
end

-- 卸载lua
function LuaHandle.unload(path)
    if nil == path then
        return
    end

    package.loaded[path] = nil
    if loadedLua:Contains(path) then
        loadedLua:Remove(path)
    end
end

-- 卸载所有lua
function LuaHandle.unloadAll()
    local luaPath = ""
    for i = loadedLua.Count - 1, 0, -1 do
        luaPath = loadedLua[i]
        if nil == blackLuaList[luaPath] then
            -- print("-------------", luaPath)
            LuaHandle.unload(luaPath)
        end
    end
end