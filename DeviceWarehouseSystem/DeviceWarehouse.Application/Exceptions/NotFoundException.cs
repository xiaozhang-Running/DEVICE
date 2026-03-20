namespace DeviceWarehouse.Application.Exceptions;

/// <summary>
/// 资源未找到异常
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message">异常消息</param>
    public NotFoundException(string message) : base(message)
    {
    }
}

/// <summary>
/// 资源已存在异常
/// </summary>
public class AlreadyExistsException : Exception
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message">异常消息</param>
    public AlreadyExistsException(string message) : base(message)
    {
    }
}

/// <summary>
/// 验证异常
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message">异常消息</param>
    public ValidationException(string message) : base(message)
    {
    }
}
