using System;
using System.Collections.Generic;
using System.Text;

namespace Algorand
{
    public class JavaHelper<T>
    {
        public static T[] ArrayCopyRange(T[] original, int from, int to)
        {
            T[] ret = new T[to - from];
            for (int i = from; i < to; i++)
            {
                ret[i - from] = original[i];
            }
            return ret;
        }
        public static T[] ArrayCopyOf(T[] source, int length)
        {
            T[] ret = new T[length];
            for (int i = 0; i < length && i < source.Length; i++)
            {
                //if (i > source.Length - 1) break;
                ret[i] = source[i];
            }
            //source.CopyTo(ret, 0);
            return ret;
        }
        public static void SyatemArrayCopy(T[] src, int srcPos, T[] dest, int destPos, int length)
        {
            if(src.Length < srcPos + length || dest.Length < destPos + length)
            {
                throw new ArgumentException("length wrong");
            }
            for(int i = 0; i < length; i++)
            {
                dest[destPos + i] = src[srcPos + i];
            }
        }
        public static string ArrayToString(T[] array)
        {
            string ret = "[";
            foreach(var item in array)
            {
                ret += item.ToString() + ", ";
            }
            return ret.Substring(0, ret.Length - 2) + "]";
        }
        public static T RequireNotNull(object o, string message)
        {            
            if (o == null)
                throw new NullReferenceException(message);
            return (T)o;
        }
    }
}
