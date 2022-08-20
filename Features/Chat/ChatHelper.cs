using BF1.ServerAdminTools.Common.Utils;
using BF1.ServerAdminTools.Features.Core;
using BF1.ServerAdminTools.Features.Utils;

namespace BF1.ServerAdminTools.Features.Chat;

public static class ChatHelper
{
    /// <summary>
    /// 按键间隔延迟，单位：毫秒
    /// </summary>
    public static int KeyPressDelay = 50;

    /// <summary>
    /// 按键模拟
    /// </summary>
    /// <param name="winVK"></param>
    /// <param name="delay"></param>
    public static void KeyPress(WinVK winVK, int delay)
    {
        Thread.Sleep(delay);
        WinAPI.Keybd_Event(winVK, WinAPI.MapVirtualKey(winVK, 0), 0, 0);
        Thread.Sleep(delay);
        WinAPI.Keybd_Event(winVK, WinAPI.MapVirtualKey(winVK, 0), 2, 0);
        Thread.Sleep(delay);
    }

    /// <summary>
    /// 全角字符转半角字符
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ToDBC(string input)
    {
        char[] chars = input.ToCharArray();

        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i] == 12288)
            {
                chars[i] = (char)32;
                continue;
            }

            if (chars[i] > 65280 && chars[i] < 65375)
            {
                chars[i] = (char)(chars[i] - 65248);
            }
        }

        return new string(chars);
    }

    /// <summary>
    /// 设置输入法为英文
    /// </summary>
    public static void SetIMEStateToEN(int delay = 50)
    {
        Thread.Sleep(delay);
        Application.Current.Dispatcher.Invoke(() =>
        {
            InputLanguageManager.Current.CurrentInputLanguage = new CultureInfo("en-US");
        });
        Thread.Sleep(delay);
    }

    /// <summary>
    /// 发送中文到战地1聊天框
    /// </summary>
    /// <param name="msg"></param>
    public static void SendTextToBf1Game(string msg)
    {
        // 如果内容为空，则跳过
        if (string.IsNullOrEmpty(msg))
            return;

        // 切换输入法到英文状态
        SetIMEStateToEN(KeyPressDelay);

        // 将窗口置顶
        Memory.SetForegroundWindow();
        Thread.Sleep(KeyPressDelay);

        // 如果聊天框开启，让他关闭
        if (ChatMsg.GetChatIsOpen())
            KeyPress(WinVK.RETURN, KeyPressDelay);

        // 模拟按键，开启聊天框
        KeyPress(WinVK.J, KeyPressDelay);

        if (ChatMsg.GetChatIsOpen())
        {
            if (ChatMsg.ChatMessagePointer() != 0)
            {
                // 挂起战地1进程
                NtProc.SuspendProcess(Memory.GetProcessId());

                msg = ChsUtil.ToTraditionalChinese(ToDBC(msg).Trim());
                var length = PlayerUtil.GetStrLength(msg);
                Memory.WriteStringUTF8(ChatMsg.GetAllocateMemoryAddress(), null, msg);

                var startPtr = ChatMsg.ChatMessagePointer() + ChatMsg.OFFSET_CHAT_MESSAGE_START;
                var endPtr = ChatMsg.ChatMessagePointer() + ChatMsg.OFFSET_CHAT_MESSAGE_END;

                var oldStartPtr = Memory.Read<long>(startPtr);
                var oldEndPtr = Memory.Read<long>(endPtr);

                Memory.Write<long>(startPtr, ChatMsg.GetAllocateMemoryAddress());
                Memory.Write<long>(endPtr, ChatMsg.GetAllocateMemoryAddress() + length);

                // 恢复战地1进程
                NtProc.ResumeProcess(Memory.GetProcessId());
                KeyPress(WinVK.RETURN, KeyPressDelay);

                // 挂起战地1进程
                NtProc.SuspendProcess(Memory.GetProcessId());
                Memory.Write<long>(startPtr, oldStartPtr);
                Memory.Write<long>(endPtr, oldEndPtr);
                // 恢复战地1进程
                NtProc.ResumeProcess(Memory.GetProcessId());
            }
        }
    }
}
