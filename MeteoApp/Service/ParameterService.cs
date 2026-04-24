public interface IParameterService
{
    int GetData();
    void SetData(int newData);
}

public class ParameterService : IParameterService
{
    private int data = 0;

    public int GetData()
    {
        return data;
    }

    public void SetData(int newData)
    {
        data = newData;
    }
}
