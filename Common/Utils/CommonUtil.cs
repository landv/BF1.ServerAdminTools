namespace BF1.ServerAdminTools.Common.Utils;

public static class CommonUtil
{

    /// <summary>
    /// 判断字符串是否为数字
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsNumber(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return false;

        var pattern = "^[0-9]*$";
        Regex rx = new(pattern);

        return rx.IsMatch(str);
    }
}
