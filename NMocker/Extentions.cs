﻿using System.Linq;
using System.Reflection;
using System.Text;

namespace NMocker.Extentions
{
    public static class MethodBaseExtensions
    {
        public static string Dump(this MethodBase method, object[] args)
        {
            StringBuilder builder = new StringBuilder();
            if (method.IsStatic)
                builder.Append("static ");
            builder.Append(method.DeclaringType.Name)
                   .Append("::")
                   .Append(method.Name)
                   .Append('(')
                   .Append(string.Join(", ", method.GetParameters().Select((p, index) => p.ParameterType.Name + "<" + args[index] + ">").ToArray()))
                   .Append(')');
            return builder.ToString();
        }

        public static string Dump(this MethodBase method)
        {
            return string.Format("{0}::{1}({2})", method.DeclaringType.Name, method.Name,
                string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name).ToArray()));
        }
    }
}
