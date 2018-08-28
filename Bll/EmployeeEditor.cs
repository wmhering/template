using System.Collections.Generic;

namespace Template.Bll
{
    public class EmployeeEditor
    {
        public int Key { get; set; }

        public byte[] Concurrency { get; set; }

        public string Name { get; set; }

        public List<Identifier> Identifiers { get; set; }

        public class Identifier
        {
            public int Key { get; set; }

            public string Value { get; set; }

            public int IdentifierTypeKey { get; set; }
        }
    }
}
