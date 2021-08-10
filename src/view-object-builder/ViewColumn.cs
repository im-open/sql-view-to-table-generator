namespace viewObjectBuilder
{
    public interface IViewColumn
    {
        string COLUMN_NAME { get; set; }

        string DATA_TYPE { get; set; }

        int? CHARACTER_MAXIMUM_LENGTH { get; set; }

        bool IS_NULLABLE { get; set; }

        string COLUMN_DEFAULT { get; set; }

        string COLLATION_NAME { get; set; }
    }

    public class ViewColumn : IViewColumn
    {
        public string COLUMN_NAME { get; set; }

        public string DATA_TYPE { get; set; }

        public int? CHARACTER_MAXIMUM_LENGTH { get; set; }

        public bool IS_NULLABLE { get; set; }

        public string COLUMN_DEFAULT { get; set; }

        public string COLLATION_NAME { get; set; }
    }

    public interface ISqlViewColumn : IViewColumn
    {
        string VIEW { get; set; }

        string VERSION { get; set; }
    }

    public class SqlViewColumn : ViewColumn, ISqlViewColumn
    {
        public string VIEW { get; set; }
        
        public string VERSION { get; set; }
    }
}
