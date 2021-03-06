namespace LuaInterface {

    using System;
    using System.Runtime.InteropServices;
    using System.Reflection;
    using System.Collections;
    using System.Text;
    using System.Security;
    using UnityEngine;

#pragma warning disable 414
    public class MonoPInvokeCallbackAttribute : System.Attribute {
        private Type type;
        public MonoPInvokeCallbackAttribute(Type t) { type = t; }
    }
#pragma warning restore 414

    public enum LuaTypes {
        LUA_TNONE = -1,
        LUA_TNIL = 0,
        LUA_TNUMBER = 3,
        LUA_TSTRING = 4,
        LUA_TBOOLEAN = 1,
        LUA_TTABLE = 5,
        LUA_TFUNCTION = 6,
        LUA_TUSERDATA = 7,
        LUA_TTHREAD = 8,
        LUA_TLIGHTUSERDATA = 2
    }

    public enum LuaGCOptions {
        LUA_GCSTOP = 0,
        LUA_GCRESTART = 1,
        LUA_GCCOLLECT = 2,
        LUA_GCCOUNT = 3,
        LUA_GCCOUNTB = 4,
        LUA_GCSTEP = 5,
        LUA_GCSETPAUSE = 6,
        LUA_GCSETSTEPMUL = 7,
    }

    public enum LuaThreadStatus {
        LUA_YIELD = 1,
        LUA_ERRRUN = 2,
        LUA_ERRSYNTAX = 3,
        LUA_ERRMEM = 4,
        LUA_ERRERR = 5,
    }

    sealed class LuaIndexes {
        public static int LUA_REGISTRYINDEX = -10000;
        public static int LUA_ENVIRONINDEX = -10001;
        public static int LUA_GLOBALSINDEX = -10002;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ReaderInfo {
        public String chunkData;
        public bool finished;
    }

    public delegate int LuaCSFunction(IntPtr luaState);
    public delegate string LuaChunkReader(IntPtr luaState, ref ReaderInfo data, ref uint size);

    public delegate int LuaFunctionCallback(IntPtr luaState);
    public class LuaDLL {
        public static int LUA_MULTRET = -1;
#if UNITY_IPHONE
        const string LUADLL = "__Internal";
#else
        const string LUADLL = "ulua";
#endif

        // Thread Funcs
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_tothread(IntPtr L, int index);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_xmove(IntPtr from, IntPtr to, int n);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_yield(IntPtr L, int nresults);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_newthread(IntPtr L);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_resume(IntPtr L, int narg);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_status(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_pushthread(IntPtr L);
        public static int luaL_getn(IntPtr luaState, int i) {
            return (int)LuaDLL.lua_objlen(luaState, i);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_gc(IntPtr luaState, LuaGCOptions what, int data);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern string lua_typename(IntPtr luaState, LuaTypes type);
        public static string luaL_typename(IntPtr luaState, int stackPos) {
            return LuaDLL.lua_typename(luaState, LuaDLL.lua_type(luaState, stackPos));
        }

        public static bool lua_isfunction(IntPtr luaState, int stackPos) {
            return lua_type(luaState, stackPos) == LuaTypes.LUA_TFUNCTION;
        }

        public static int lua_islightuserdata(IntPtr luaState, int stackPos) {
            return Convert.ToInt32(lua_type(luaState, stackPos) == LuaTypes.LUA_TLIGHTUSERDATA);
        }

        public static bool lua_istable(IntPtr luaState, int stackPos) {
            return lua_type(luaState, stackPos) == LuaTypes.LUA_TTABLE;
        }

        public static int lua_isthread(IntPtr luaState, int stackPos) {
            return Convert.ToInt32(lua_type(luaState, stackPos) == LuaTypes.LUA_TTHREAD);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaL_error(IntPtr luaState, string message);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern string luaL_gsub(IntPtr luaState, string str, string pattern, string replacement);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_getfenv(IntPtr luaState, int stackPos);


        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_isuserdata(IntPtr luaState, int stackPos);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_lessthan(IntPtr luaState, int stackPos1, int stackPos2);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_rawequal(IntPtr luaState, int stackPos1, int stackPos2);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_setfenv(IntPtr luaState, int stackPos);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_setfield(IntPtr luaState, int stackPos, string name);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_callmeta(IntPtr luaState, int stackPos, string name);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr luaL_newstate();
        /// <summary>DEPRECATED - use luaL_newstate() instead!</summary>
        public static IntPtr lua_open() {
            return LuaDLL.luaL_newstate();
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_close(IntPtr luaState);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaL_openlibs(IntPtr luaState);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_objlen(IntPtr luaState, int stackPos);
        /// <summary>DEPRECATED - use lua_objlen(IntPtr luaState, int stackPos) instead!</summary>
        public static int lua_strlen(IntPtr luaState, int stackPos) {
            return lua_objlen(luaState, stackPos);
        }
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_loadstring(IntPtr luaState, string chunk);
        public static int luaL_dostring(IntPtr luaState, string chunk) {
            int result = LuaDLL.luaL_loadstring(luaState, chunk);
            if (result != 0)
                return result;

            return LuaDLL.lua_pcall(luaState, 0, -1, 0);
        }
        /// <summary>DEPRECATED - use luaL_dostring(IntPtr luaState, string chunk) instead!</summary>
        public static int lua_dostring(IntPtr luaState, string chunk) {
            return LuaDLL.luaL_dostring(luaState, chunk);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_createtable(IntPtr luaState, int narr, int nrec);
        public static void lua_newtable(IntPtr luaState) {
            LuaDLL.lua_createtable(luaState, 0, 0);
        }
        public static int luaL_dofile(IntPtr luaState, string fileName) {
            int result = LuaDLL.luaL_loadfile(luaState, fileName);
            if (result != 0)
                return result;

            return LuaDLL.lua_pcall(luaState, 0, -1, 0);
        }
        public static float lua_tofloat(IntPtr luaState, int index) {
            return (float)lua_tonumber(luaState, index);
        }
        public static void lua_getglobal(IntPtr luaState, string name) {
            LuaDLL.lua_pushstring(luaState, name);
            LuaDLL.lua_gettable(luaState, LuaIndexes.LUA_GLOBALSINDEX);
        }
        public static void lua_setglobal(IntPtr luaState, string name) {
            LuaDLL.lua_pushstring(luaState, name);
            LuaDLL.lua_insert(luaState, -2);
            LuaDLL.lua_settable(luaState, LuaIndexes.LUA_GLOBALSINDEX);
        }
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_settop(IntPtr luaState, int newTop);
        public static void lua_pop(IntPtr luaState, int amount) {
            LuaDLL.lua_settop(luaState, -(amount) - 1);
        }
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_insert(IntPtr luaState, int newTop);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_remove(IntPtr luaState, int index);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_gettable(IntPtr luaState, int index);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_rawget(IntPtr luaState, int index);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_settable(IntPtr luaState, int index);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_rawset(IntPtr luaState, int index);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_setmetatable(IntPtr luaState, int objIndex);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_getmetatable(IntPtr luaState, int objIndex);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_equal(IntPtr luaState, int index1, int index2);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushvalue(IntPtr luaState, int index);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_replace(IntPtr luaState, int index);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_gettop(IntPtr luaState);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaTypes lua_type(IntPtr luaState, int index);
        public static bool lua_isnil(IntPtr luaState, int index) {
            return (LuaDLL.lua_type(luaState, index) == LuaTypes.LUA_TNIL);
        }
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_isnumber(IntPtr luaState, int index);
        public static bool lua_isboolean(IntPtr luaState, int index) {
            return LuaDLL.lua_type(luaState, index) == LuaTypes.LUA_TBOOLEAN;
        }
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_ref(IntPtr luaState, int registryIndex);
        public static int lua_ref(IntPtr luaState, int lockRef) {
            if (lockRef != 0) {
                return LuaDLL.luaL_ref(luaState, LuaIndexes.LUA_REGISTRYINDEX);
            }
            else return 0;
        }
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_rawgeti(IntPtr luaState, int tableIndex, int index);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_rawseti(IntPtr luaState, int tableIndex, int index);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_newuserdata(IntPtr luaState, int size);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_touserdata(IntPtr luaState, int index);
        public static void lua_getref(IntPtr luaState, int reference) {
            LuaDLL.lua_rawgeti(luaState, LuaIndexes.LUA_REGISTRYINDEX, reference);
        }
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaL_unref(IntPtr luaState, int registryIndex, int reference);
        public static void lua_unref(IntPtr luaState, int reference) {
            LuaDLL.luaL_unref(luaState, LuaIndexes.LUA_REGISTRYINDEX, reference);
        }
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_isstring(IntPtr luaState, int index);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_iscfunction(IntPtr luaState, int index);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushnil(IntPtr luaState);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushstdcallcfunction(IntPtr luaState, IntPtr wrapper);

        public static void lua_pushstdcallcfunction(IntPtr luaState, LuaCSFunction function) {
            IntPtr fn = Marshal.GetFunctionPointerForDelegate(function);
            lua_pushstdcallcfunction(luaState, fn);
        }
        public static void lua_pushstdcallcfunction(IntPtr luaState, LuaCSFunction function, string functionName) {
            lua_pushstdcallcfunction(luaState, function);
            lua_setfield(luaState, -2, functionName);
        }
        public static void lua_pushcsharpproperty(IntPtr luaState, string propertyName, LuaCSFunction get, LuaCSFunction set) {
            LuaDLL.lua_newtable(luaState);
            LuaDLL.lua_pushstdcallcfunction(luaState, get);
            LuaDLL.lua_setfield(luaState, -2, "get");

            if (set != null) {
                LuaDLL.lua_pushstdcallcfunction(luaState, set);
                LuaDLL.lua_setfield(luaState, -2, "set");
            }

            LuaDLL.lua_setfield(luaState, -2, propertyName);
        }
        public static void lua_setmetatable(IntPtr luaState, string metatable) {
            LuaDLL.lua_getglobal(luaState, metatable);
            if (LuaDLL.lua_isnil(luaState, -1)) {
                LuaDLL.lua_pop(luaState, 1);
            }
            else {
                LuaDLL.lua_setmetatable(luaState, -2);
            }
        }
        public static void lua_pushrect(IntPtr luaState, Rect rect) {
            LuaDLL.lua_newtable(luaState);
            LuaDLL.lua_pushnumber(luaState, (double)rect.x);
            LuaDLL.lua_setfield(luaState, -2, "x");
            LuaDLL.lua_pushnumber(luaState, (double)rect.y);
            LuaDLL.lua_setfield(luaState, -2, "y");
            LuaDLL.lua_pushnumber(luaState, (double)rect.width);
            LuaDLL.lua_setfield(luaState, -2, "width");
            LuaDLL.lua_pushnumber(luaState, (double)rect.height);
            LuaDLL.lua_setfield(luaState, -2, "height");

            lua_setmetatable(luaState, "Rect");

            LuaDLL.lua_pushvalue(luaState, -1);
        }
        public static void lua_pushvector2(IntPtr luaState, Vector2 vector2) {
            LuaDLL.lua_newtable(luaState);
            LuaDLL.lua_pushnumber(luaState, (double)vector2.x);
            LuaDLL.lua_setfield(luaState, -2, "x");
            LuaDLL.lua_pushnumber(luaState, (double)vector2.y);
            LuaDLL.lua_setfield(luaState, -2, "y");

            lua_setmetatable(luaState, "Vector2");

            LuaDLL.lua_pushvalue(luaState, -1);
        }
        public static void lua_pushvector3(IntPtr luaState, Vector3 v) {
            LuaDLL.lua_newtable(luaState);
            LuaDLL.lua_pushnumber(luaState, (double)v.x);
            LuaDLL.lua_setfield(luaState, -2, "x");
            LuaDLL.lua_pushnumber(luaState, (double)v.y);
            LuaDLL.lua_setfield(luaState, -2, "y");
            LuaDLL.lua_pushnumber(luaState, (double)v.z);
            LuaDLL.lua_setfield(luaState, -2, "z");

            lua_setmetatable(luaState, "Vector3");

            LuaDLL.lua_pushvalue(luaState, -1);
        }
        public static void lua_pushvector4(IntPtr luaState, Vector4 v) {
            LuaDLL.lua_newtable(luaState);
            LuaDLL.lua_pushnumber(luaState, (double)v.x);
            LuaDLL.lua_setfield(luaState, -2, "x");
            LuaDLL.lua_pushnumber(luaState, (double)v.y);
            LuaDLL.lua_setfield(luaState, -2, "y");
            LuaDLL.lua_pushnumber(luaState, (double)v.z);
            LuaDLL.lua_setfield(luaState, -2, "z");
            LuaDLL.lua_pushnumber(luaState, (double)v.w);
            LuaDLL.lua_setfield(luaState, -2, "w");

            lua_setmetatable(luaState, "Vector4");

            LuaDLL.lua_pushvalue(luaState, -1);
        }
        //public static LuaTable lua_toluatable(IntPtr luaState, int index) {
        //    LuaDLL.lua_pushvalue(luaState, index);
        //    //return new LuaTable(LuaDLL.luaL_ref(luaState, LuaIndexes.LUA_REGISTRYINDEX), luaState);
        //}
        public static Rect lua_torect(IntPtr L, int index) {
            LuaDLL.lua_getfield(L, index, "x");
            float num = LuaDLL.lua_tofloat(L, -1);
            LuaDLL.lua_pop(L, 1);
            LuaDLL.lua_getfield(L, index, "y");
            float num2 = LuaDLL.lua_tofloat(L, -1);
            LuaDLL.lua_pop(L, 1);
            LuaDLL.lua_getfield(L, index, "width");
            float num3 = LuaDLL.lua_tofloat(L, -1);
            LuaDLL.lua_pop(L, 1);
            LuaDLL.lua_getfield(L, index, "height");
            float num4 = LuaDLL.lua_tofloat(L, -1);
            LuaDLL.lua_pop(L, 1);
            return new Rect(num, num2, num3, num4);
        }
        public static Vector2 lua_tovector2(IntPtr L, int index) {
            LuaDLL.lua_getfield(L, index, "x");
            float num = LuaDLL.lua_tofloat(L, -1);
            LuaDLL.lua_pop(L, 1);
            LuaDLL.lua_getfield(L, index, "y");
            float num2 = LuaDLL.lua_tofloat(L, -1);
            LuaDLL.lua_pop(L, 1);
            return new Vector2(num, num2);
        }
        public static Vector3 lua_tovector3(IntPtr L, int index) {
            LuaDLL.lua_getfield(L, index, "x");
            float num = LuaDLL.lua_tofloat(L, -1);
            LuaDLL.lua_pop(L, 1);
            LuaDLL.lua_getfield(L, index, "y");
            float num2 = LuaDLL.lua_tofloat(L, -1);
            LuaDLL.lua_pop(L, 1);
            LuaDLL.lua_getfield(L, index, "z");
            float num3 = LuaDLL.lua_tofloat(L, -1);
            LuaDLL.lua_pop(L, 1);
            return new Vector3(num, num2, num3);
        }
        public static Vector4 lua_tovector4(IntPtr L, int index) {
            LuaDLL.lua_getfield(L, index, "x");
            float num = LuaDLL.lua_tofloat(L, -1);
            LuaDLL.lua_pop(L, 1);
            LuaDLL.lua_getfield(L, index, "y");
            float num2 = LuaDLL.lua_tofloat(L, -1);
            LuaDLL.lua_pop(L, 1);
            LuaDLL.lua_getfield(L, index, "z");
            float num3 = LuaDLL.lua_tofloat(L, -1);
            LuaDLL.lua_pop(L, 1);
            LuaDLL.lua_getfield(L, index, "w");
            float num4 = LuaDLL.lua_tofloat(L, -1);
            LuaDLL.lua_pop(L, 1);
            return new Vector4(num, num2, num3, num4);
        }
        public static bool lua_metatableequal(IntPtr luaState, int index, string metatable) {
            LuaDLL.lua_getmetatable(luaState, index);
            LuaDLL.lua_getglobal(luaState, metatable);
            return LuaDLL.lua_equal(luaState, -1, -2) > 0 ? true : false;
        }
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_call(IntPtr luaState, int nArgs, int nResults);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_pcall(IntPtr luaState, int nArgs, int nResults, int errfunc);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_tocfunction(IntPtr luaState, int index);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern double lua_tonumber(IntPtr luaState, int index);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_toboolean(IntPtr luaState, int index);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_tolstring(IntPtr luaState, int index, out int strLen);

        public static string lua_tostring(IntPtr luaState, int index) {
            int strlen;

            IntPtr str = lua_tolstring(luaState, index, out strlen);
            if (str != IntPtr.Zero) {
                return Marshal.PtrToStringAnsi(str, strlen);
            }
            else {
                return null;
            }
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_atpanic(IntPtr luaState, LuaCSFunction panicf);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushnumber(IntPtr luaState, double number);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushboolean(IntPtr luaState, bool value);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushlstring(IntPtr luaState, string str, int size);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushstring(IntPtr luaState, string str);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_newmetatable(IntPtr luaState, string meta);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_getfield(IntPtr luaState, int stackPos, string meta);
        public static void luaL_getmetatable(IntPtr luaState, string meta) {
            LuaDLL.lua_getfield(luaState, LuaIndexes.LUA_REGISTRYINDEX, meta);
        }
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaL_getmetafield(IntPtr luaState, int stackPos, string field);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_load(IntPtr luaState, LuaChunkReader chunkReader, ref ReaderInfo data, string chunkName);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_loadbuffer(IntPtr luaState, byte[] buff, int size, string name);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_loadfile(IntPtr luaState, string filename);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_error(IntPtr luaState);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_checkstack(IntPtr luaState, int extra);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_next(IntPtr luaState, int index);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushlightuserdata(IntPtr luaState, IntPtr udata);
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaL_where(IntPtr luaState, int level);
    }
}
