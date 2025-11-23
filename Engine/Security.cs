using System.Security.Cryptography;
using System.Text;
namespace BCEngine.Security
{
    public class Security
    {
        public static string ToSHA256(string rawString)
        {
            // 确保使用 Using 块来自动释放资源
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // 将输入字符串转换为字节数组（通常使用 UTF8 编码）
                byte[] bytes = Encoding.UTF8.GetBytes(rawString);

                // 计算哈希值，返回一个 32 字节的数组
                byte[] hashBytes = sha256Hash.ComputeHash(bytes);

                // 将字节数组转换为十六进制字符串
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    // "x2" 表示将字节格式化为两个十六进制字符（小写）
                    builder.Append(hashBytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}