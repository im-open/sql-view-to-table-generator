using System.Collections.Generic;

namespace viewObjectBuilder
{
    public class ViewDefinition
    {
        public string Name { get; set; }

        public string Version { get; set; }
        
        public string SqlText { get; set; }

        public string ClassText { get; set; }

        public IEnumerable<ViewColumn> Columns { get; set; }
    }
}
