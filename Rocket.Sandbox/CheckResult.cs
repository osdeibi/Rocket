﻿using System;
using System.Linq;
using System.Reflection;

namespace Rocket.Sandbox
{
    public class CheckResult
    {
        public ReadableInstruction IllegalInstruction { get; set; }
        public ReadableInstruction Position { get; set; }
        public bool Passed => IllegalInstruction == null;
    }

    public class ReadableInstruction
    {
        public string InstructionName { get; private set; }
        public object InstructionObject { get; }
        public InstructionType InstructionType { get; }
        public BlockReason BlockReason { get; }

        public ReadableInstruction(MethodBase method, BlockReason reason = BlockReason.RESTRICTED)
        {
            InstructionName = (method.DeclaringType == null ? "" : method.DeclaringType.FullName + ".") + method.Name;

            if (method is MethodInfo)
                InstructionName = ((MethodInfo)method).ReturnType.Name + " " + InstructionName;

            InstructionType = InstructionType.METHOD;
            if (method is ConstructorInfo)
                InstructionType = InstructionType.CONSTRUCTOR;

            InstructionObject = method;
            BlockReason = reason;
            AppendInstructionType();
        }

        public ReadableInstruction(Type type, bool failedOnBase = false, BlockReason reason = BlockReason.RESTRICTED)
        {
            InstructionName = string.IsNullOrEmpty(type.FullName) ? type.Name : type.FullName + (failedOnBase ? " on " + type.BaseType.FullName : "");
            InstructionObject = type;
            InstructionType = InstructionType.TYPE;
            BlockReason = reason;
            AppendInstructionType();
        }

        public ReadableInstruction(MethodAttributes attributes, BlockReason reason = BlockReason.RESTRICTED)
        {
            var flags = Enum.GetValues(typeof(MethodAttributes)).Cast<MethodAttributes>()
                .Where(f => (attributes & f) == 0)
                .ToList();

            InstructionName = "[" + string.Join(", ", flags.Cast<string>().ToArray()) + "]";
            InstructionObject = flags;
            InstructionType = InstructionType.METHOD_ATTRIBUTE;
            BlockReason = reason;
            AppendInstructionType();
        }

        public ReadableInstruction(MethodBase method, Instruction ins, BlockReason reason = BlockReason.RESTRICTED)
        {
            InstructionName = (method.DeclaringType == null ? "" : method.DeclaringType.FullName + ".") + method.Name + " (on operand: " + ins.OpCode + " @@ 0x" + ins.Offset.ToString("X") + ")";
            InstructionObject = new MethodInstruction(method, ins);
            InstructionType = InstructionType.OPERAND;
            BlockReason = reason;
            AppendInstructionType();
        }

        public ReadableInstruction(Assembly asm, BlockReason reason = BlockReason.RESTRICTED)
        {
            InstructionName = asm.FullName;
            InstructionObject = asm;
            InstructionType = InstructionType.ASSEMBLY;
            BlockReason = reason;
            AppendInstructionType();
        }

        public ReadableInstruction(PropertyInfo def, MethodInfo method, BlockReason reason = BlockReason.RESTRICTED) : this(method, reason)
        {
            InstructionName = (method.DeclaringType == null ? "" : method.DeclaringType.FullName + ".") + method.Name;
            InstructionObject = new PropertyInstruction {Property = def, Method = method};
            BlockReason = reason;
            InstructionType = InstructionType.PROPERTY;
            AppendInstructionType();
        }

        public override bool Equals(object o)
        {
            if (!(o is ReadableInstruction))
                return false;

            return Equals((ReadableInstruction) o);
        }

        protected bool Equals(ReadableInstruction other)
        {
            return Equals(InstructionObject, other.InstructionObject);
        }

        public override int GetHashCode()
        {
            return InstructionObject?.GetHashCode() ?? 0;
        }

        private void AppendInstructionType()
        {
            InstructionName = "[" + InstructionType + "] " + InstructionName;
        }

        public override string ToString()
        {
            return InstructionName;
        }
    }

    public class PropertyInstruction
    {
        public PropertyInfo Property { get; set; }
        public MethodInfo Method { get; set; }
    }

    public class MethodInstruction
    {
        public MethodBase Method { get; }
        public Instruction Instruction { get; }

        public MethodInstruction(MethodBase method, Instruction ins)
        {
            Method = method;
            Instruction = ins;
        }
    }

    public enum BlockReason
    {
        RESTRICTED,
        ILLEGAL_NAME
    }

    public enum InstructionType
    {
        TYPE,
        METHOD,
        OPERAND,
        METHOD_ATTRIBUTE,
        ASSEMBLY,
        CONSTRUCTOR,
        PROPERTY
    }
}