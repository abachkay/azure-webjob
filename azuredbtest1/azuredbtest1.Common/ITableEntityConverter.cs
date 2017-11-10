namespace azuredbtest1.Common
{
    public interface ITableEntityConverter<in TInput, out TOutput>
    {
        TOutput FromTableEntity(TInput arg);
    }
}
