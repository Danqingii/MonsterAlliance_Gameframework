/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System;
using UnityEngine;
using System.Collections;
using System.IO;
using Game;
using XLua;

namespace Tutorial
{
    public class CustomLoader : MonoBehaviour
    {
        LuaEnv luaenv = null;
        // Use this for initialization
        void Start()
        {
            luaenv = new LuaEnv();

            luaenv.AddLoader(CustomLuaLoader);
            luaenv.DoString("require 'LuaMain'"); //Dostring 内部已经处理 require了 只会返回 luamain

            luaenv.Global.Get<Action>("OnInit").Invoke();
        }

        private byte[] CustomLuaLoader(ref string fileName)
        {
            //TODO
            string script = $"Assets/GameMain/Scripts/LuaScripts/{fileName}.lua";

            byte[] bytes = File.ReadAllBytes(script);
            
            if (bytes[0] == 239 && bytes[1] == 187 && bytes[2] == 191)
            {
                // 处理UFT-8 BOM头
                bytes[0] = bytes[1] = bytes[2] = 32;
            }
            return bytes;
        }
        
        // Update is called once per frame
        void Update()
        {
            if (luaenv != null)
            {
                luaenv.Tick();
            }
        }

        void OnDestroy()
        {
            luaenv.Dispose();
        }
    }
}
