using System;

namespace GetampedPaint.States
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UndoRedoAttribute : Attribute { }
}