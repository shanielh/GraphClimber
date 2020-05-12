﻿using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GraphClimber.Examples
{
    internal class SuperBinaryWriter : BinaryWriter
    {
        private readonly IDictionary<string, int> _writtenStrings = new Dictionary<string, int>();

        protected SuperBinaryWriter()
        {
        }

        public SuperBinaryWriter(Stream output) : base(output)
        {
        }

        public SuperBinaryWriter(Stream output, Encoding encoding) : base(output, encoding)
        {
        }

        public SuperBinaryWriter(Stream output, Encoding encoding, bool leaveOpen) : base(output, encoding, leaveOpen)
        {
        }

        public override void Write(string value)
        {
            int id;
            if (_writtenStrings.TryGetValue(value, out id))
            {
                base.Write((byte)1);
                base.Write(id);
            }
            else
            {
                _writtenStrings[value] = _writtenStrings.Count;
                base.Write((byte)0);
                base.Write(value);
            }
        }
    }
}