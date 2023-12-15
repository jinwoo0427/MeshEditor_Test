using System;

namespace XDPaint.States
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UndoRedoAttribute : Attribute { }
}