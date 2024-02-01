[Serializable]
public class Data{
    public int level;
    public int grade;
    public string nickname;

    public Data(string _nickname, int _level, int _grade){
        nickname = _nickname;
        level    = _level;
        grade    = _grade;
    }
}