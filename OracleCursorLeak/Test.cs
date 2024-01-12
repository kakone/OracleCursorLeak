using System.ComponentModel.DataAnnotations;

namespace OracleCursorLeak;

public class Test
{
    public int TestId { get; set; }

    public int Number { get; set; }

    [Timestamp]
    public byte[] Version { get; set; }
}
