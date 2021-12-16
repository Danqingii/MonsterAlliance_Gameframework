/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using UnityEngine;
using System.Collections;
using System;
using XLua;
using System.Collections.Generic;

namespace Tutorial
{
	[LuaCallCSharp]
	public class BaseClass
	{
		public static void BSFunc()
		{
			Debug.Log("Derived Static Func, BSF = " + BSF);
		}

		public static int BSF = 1;

		public void BMFunc()
		{
			Debug.Log("Derived Member Func, BMF = " + BMF);
		}

		public int BMF { get; set; }
	}

	public struct Param1
	{
		public int x;
		public string y;
	}

	[LuaCallCSharp]
	public enum TestEnum
	{
		E1,
		E2
	}


	[LuaCallCSharp]
	public class DerivedClass : BaseClass
	{
		[LuaCallCSharp]
		public enum TestEnumInner
		{
			E3,
			E4
		}

		public void DMFunc()
		{
			Debug.Log("Derived Member Func, DMF = " + DMF);
		}

		public int DMF { get; set; }

		public double ComplexFunc(Param1 p1, ref int p2, out string p3, Action luafunc, out Action csfunc)
		{
			Debug.Log("P1 = {x=" + p1.x + ",y=" + p1.y + "},p2 = " + p2);
			luafunc();
			p2 = p2 * p1.x;
			p3 = "hello " + p1.y;
			csfunc = () =>
			{
				Debug.Log("csharp callback invoked!");
			};
			return 1.23;
		}

		public void TestFunc(int i)
		{
			Debug.Log("TestFunc(int i)");
		}

		public void TestFunc(string i)
		{
			Debug.Log("TestFunc(string i)");
		}

		public static DerivedClass operator +(DerivedClass a, DerivedClass b)
		{
            DerivedClass ret = new DerivedClass();
			ret.DMF = a.DMF + b.DMF;
			return ret;
		}

		public void DefaultValueFunc(int a = 100, string b = "cccc", string c = null)
		{
			UnityEngine.Debug.Log("DefaultValueFunc: a=" + a + ",b=" + b + ",c=" + c);
		}

		public void VariableParamsFunc(int a, params string[] strs)
		{
			UnityEngine.Debug.Log("VariableParamsFunc: a =" + a);
			foreach (var str in strs)
			{
				UnityEngine.Debug.Log("str:" + str);
			}
		}

		public TestEnum EnumTestFunc(TestEnum e)
		{
			Debug.Log("EnumTestFunc: e=" + e);
			return TestEnum.E2;
		}

		public Action<string> TestDelegate = (param) =>
		{
			Debug.Log("TestDelegate in c#:" + param);
		};

		public event Action TestEvent;

		public void CallEvent()
		{
			TestEvent();
		}

		public ulong TestLong(long n)
		{
			return (ulong)(n + 1);
		}

		class InnerCalc : ICalc
		{
			public int add(int a, int b)
			{
				return a + b;
			}

			public int id = 100;
		}

		public ICalc GetCalc()
		{
			return new InnerCalc();
		}

		public void GenericMethod<T>()
		{
			Debug.Log("GenericMethod<" + typeof(T) + ">");
		}
	}

	[LuaCallCSharp]
	public interface ICalc
	{
		int add(int a, int b);
	}

	[LuaCallCSharp]
	public static class DerivedClassExtensions
    {
		public static int GetSomeData(this DerivedClass obj)
		{
			Debug.Log("GetSomeData ret = " + obj.DMF);
			return obj.DMF;
		}

		public static int GetSomeBaseData(this BaseClass obj)
		{
			Debug.Log("GetSomeBaseData ret = " + obj.BMF);
			return obj.BMF;
		}

		public static void GenericMethodOfString(this DerivedClass obj)
		{
			obj.GenericMethod<string>();
		}
	}
}

public class LuaCallCs : MonoBehaviour
{
	LuaEnv luaenv = null;

	// Use this for initialization
	void Start()
	{
		luaenv = new LuaEnv();
		luaenv.DoString("require 'LuaCallCSharp'");
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
