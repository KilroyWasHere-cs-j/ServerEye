using System.Windows.Documents;

namespace ServerEye
{
    public class Embedded
    {

    }

    //Defines the variables for a stored procedure
    public class Stored
    {
        public string Name { get; set; }
        public int cID { get; set; }
        public Parameters Parameters { get; set; }
    }

    // Defines the parameters for stored procedures
    public class Parameters
    {
        public string Name { get; set; }
        public dynamic value { get; set; }
    }
}
