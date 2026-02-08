[Serializable]
public class GroupRequest
{
    public string GroupName { get; set; }
    public List<string> Members { get; set; } // Danh sách IP/Username thành viên
}